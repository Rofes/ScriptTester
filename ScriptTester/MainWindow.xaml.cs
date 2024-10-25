
using System.Windows;

namespace ScriptTester
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private DebuggerWindow _debuggerWindow;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void OpenDebugger_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_debuggerWindow?.Close();

			_debuggerWindow = new DebuggerWindow();
			_debuggerWindow.Show();
		}

		private void Test_Click(object sender, RoutedEventArgs e)
		{
			_debuggerWindow.Test();
		}
	}
}