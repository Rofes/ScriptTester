using System.IO;
using Alternet.Scripter;
using System.Windows;
using Alternet.Common.Projects.DotNet;
using Alternet.Editor.Roslyn.Wpf;

namespace ScriptTester;

public class ScriptRunner
{
		private static readonly string StartupProjectFileSubPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\ScriptLib\ScriptLib.csproj");
	private readonly ScriptRun _scriptRun = new();
	private DotNetProject Project { get; } = new();

	public ScriptRunner()
	{
		_scriptRun.ScriptMode = ScriptMode.Debug;
		_scriptRun.ScriptHost.GenerateModulesOnDisk = false;
		OpenProject(StartupProjectFileSubPath);
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

		var references = Project.References.Concat(Project.AutoReferences).Select(x => x.FullName).Concat(
			Project.FrameworkReferences.SelectMany(x => x.Assemblies).Select(x => x.HintPath)).ToArray();

		CodeEditExtensions.RegisterAssemblies(
			extension,
			Project.TryResolveAbsolutePaths(references).ToArray(),
			projectName: Project.ProjectName,
			targetFramework: Project.TargetFramework);
	}


	private void CloseProject(DotNetProject project)
	{
		CodeEditExtensions.CloseProject($".{project.DefaultExtension}", project.ProjectName);
		Project.Reset();
		_scriptRun.ScriptSource?.Reset();
	}
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
}

