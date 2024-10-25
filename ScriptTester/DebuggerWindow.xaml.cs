using Alternet.Common.Projects.DotNet;
using Alternet.Editor.Roslyn.Wpf;
using Alternet.Scripter;
using System.IO;
using System.Windows;
using Alternet.FormDesigner.Integration.Wpf;
using Alternet.FormDesigner.Wpf;
using Alternet.Scripter.Debugger;
using Alternet.Scripter.Integration.Wpf;

namespace ScriptTester
{
	/// <summary>
	/// Interaction logic for DebuggerWindow.xaml
	/// </summary>
	public partial class DebuggerWindow
	{
		#region Fields

		private static readonly string StartupProjectFileSubPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\ScriptLib\ScriptLib.csproj");

		private readonly DebugCodeEditContainer _codeEditContainer;

		private readonly ScriptRun _scriptRun = new();

		#endregion

		#region Properties

		private DotNetProject Project { get; } = new();

		#endregion

		#region Methods

		public void Test()
		{
			try
			{
				double var1 = 5.5;
				double var2 = 35.5;
				var data = _scriptRun.RunMethod("Scripts.Sum", null, new object[] { var1, var2 });
				MessageBox.Show($"Sum = {data}");
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		public DebuggerWindow()
		{
			InitializeComponent();

			_scriptRun.ScriptMode = ScriptMode.Debug;
			_scriptRun.ScriptHost.GenerateModulesOnDisk = false;
			var debugger = new ScriptDebugger
			{
				ScriptRun = _scriptRun,
			};
			
			_codeEditContainer = new DebugCodeEditContainer(EditorsTabControl);
			_codeEditContainer.EditorRequested += OnEditorRequested;

			OpenProject(StartupProjectFileSubPath);


			DebuggerControlToolbar.Debugger = debugger;
			DebuggerControlToolbar.DebuggerPreStartup += OnDebuggerPreStartup;

			DebugMenu.Debugger = debugger;
			DebugMenu.DebuggerPreStartup += OnDebuggerPreStartup;

			var controller = new DebuggerUIController(Dispatcher, _codeEditContainer);
			controller.Debugger = debugger;
			controller.DebuggerPanels = DebuggerPanelsTabControl;
			_codeEditContainer.Debugger = debugger;

			DebugMenu.InstallKeyboardShortcuts(CommandBindings);
			FileMenu.SubmenuOpened += FileMenu_SubmenuOpened;
		}

		private void SaveAllModifiedFiles()
		{
			foreach (var edit in _codeEditContainer.Editors)
			{
				if (edit.Modified)
				{
					edit.SaveFile(edit.FileName);
				}
			}
		}

		private static string GetFirstFile(IList<string> files, string langExt)
		{
			string result = files.Count > 0 ? files[0] : string.Empty;

			foreach (string file in files)
			{
				if (file.ToLower().Contains("program.cs"))
				{
					return file;
				}

				if (file.ToLower().Contains("main") && file.EndsWith(langExt))
				{
					return file;
				}
			}

			return result;
		}

		private void OnDebuggerPreStartup(object? sender, EventArgs e)
		{
			SaveAllModifiedFiles();
			SetScriptSource();
		}

		private void SetScriptSource()
		{
			if (Project.HasProject)
			{
				return;
			}

			if (_codeEditContainer.ActiveEditor != null)
			{
				string fileName = _codeEditContainer.ActiveEditor.FileName;
				if (new FileInfo(fileName).Exists)
				{
					_scriptRun.ScriptSource.FromScriptFile(fileName);
				}
			}
		}

		private void OpenProject(string projectFilePath)
		{
			if (Project.HasProject)
			{
				CloseProject(Project);
			}

			Project.Load(projectFilePath);
			_scriptRun.ScriptSource.FromScriptProject(Project.ProjectFileName);
			var extension = Project.ProjectExtension;
			CodeEditExtensions.OpenProject(extension, Project.ProjectName, Project.ProjectFileName);

			var codeFiles = Project.Files.Where(x => Path.GetExtension(x) == ".cs" || Path.GetExtension(x) == ".vb").ToList();

			if (codeFiles.Count > 0)
			{
				foreach (var file in codeFiles.ToArray())
				{
					if (FormFilesUtility.IsXamlCodeBehindFile(file, out var formId))
					{
						codeFiles.Add(XamlGeneratedCodeFileService.GetGeneratedCodeFile(new FormDesignerDataSource(formId, FormFilesUtility.DetectLanguageFromFileName(file))));
					}
				}

				CodeEditExtensions.RegisterCode(extension, codeFiles.ToArray(), Project.ProjectName);
				_codeEditContainer.TryActivateEditor(GetFirstFile(codeFiles, Project.DefaultExtension));
			}

			var references = Project.References.Concat(Project.AutoReferences).Select(x => x.FullName).Concat(
					Project.FrameworkReferences.SelectMany(x => x.Assemblies).Select(x => x.HintPath)).ToArray();

			CodeEditExtensions.RegisterAssemblies(
				 extension,
				 Project.TryResolveAbsolutePaths(references).ToArray(),
				 projectName: Project.ProjectName,
				 targetFramework: Project.TargetFramework);

			DebuggerPanelsTabControl.Errors.Clear();
		}

		private void CloseProject(DotNetProject project)
		{
			foreach (string fileName in project.Files)
			{
				_codeEditContainer.CloseFile(fileName);
			}

			foreach (string fileName in project.Resources)
			{
				_codeEditContainer.CloseFile(fileName);
			}

			CodeEditExtensions.CloseProject($".{project.DefaultExtension}", project.ProjectName);
			Project.Reset();
			_scriptRun.ScriptSource?.Reset();
		}

		private string GetProjectName(string fileName)
		{
			if (Project.HasProject)
			{
				if (Project.Files.Contains(fileName, StringComparer.OrdinalIgnoreCase))
				{
					return Project.ProjectName;
				}
			}

			return null;
		}

		private void OnEditorRequested(object? sender, DebugEditRequestedEventArgs e)
		{
			var edit = new DebugCodeEdit();
			var projectName = GetProjectName(e.FileName);
			edit.SetFileNameAndProject(e.FileName, projectName);
			edit.LoadFile(e.FileName);
			e.DebugEdit = edit;
		}

		private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (_codeEditContainer.ActiveEditor != null)
			{
				_codeEditContainer.ActiveEditor.SaveFile(_codeEditContainer.ActiveEditor.FileName);
			}
		}

		private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var edit = _codeEditContainer.ActiveEditor;
			if (edit != null)
			{
				_codeEditContainer.CloseFile(edit.FileName);
				edit.FileName = string.Empty;
			}

			if (!Project.HasProject && _codeEditContainer.Editors.Count == 0)
			{
				Project.Reset();
				_scriptRun.ScriptSource?.Reset();
			}
		}

		private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void FileMenu_SubmenuOpened(object sender, RoutedEventArgs e)
		{
			CloseMenuItem.IsEnabled = _codeEditContainer.ActiveEditor != null;
		}

		#endregion
	}
}
