using Alternet.Common.DotNet.DefaultAssemblies;
using Alternet.Editor.Common.Wpf;
using Alternet.Editor.Roslyn.Wpf;
using System.IO;

namespace ScriptTester.ViewModels;

public class DebuggerViewModel
{
	private readonly ScriptCodeEdit _scriptCodeEditCtl;
	private readonly IDefaultAssembliesProvider _defaultAssembliesProvider = DefaultAssembliesProviderFactory.CreateDefaultAssembliesProvider();
	public DebuggerViewModel(ScriptCodeEdit scriptCodeEditCtl)
	{
		_scriptCodeEditCtl = scriptCodeEditCtl;
		scriptCodeEditCtl.RegisterAssemblies(_defaultAssembliesProvider.GetDefaultAssemblies(Alternet.Common.TechnologyEnvironment.Wpf));

		LoadFile(@"C:\Users\rfes\source\repos\ScriptTester\ScriptTester\ViewModels\DebuggerViewModel.cs");
	}

	private void LoadFile(string fileName)
	{
		if (new FileInfo(fileName).Exists)
			_scriptCodeEditCtl.LoadFile(fileName);

		_scriptCodeEditCtl.FileName = fileName;
		_scriptCodeEditCtl.RegisterAssemblies(_defaultAssembliesProvider.GetDefaultAssemblies(Alternet.Common.TechnologyEnvironment.Wpf));
	}
}