using System;
using System.Windows;

namespace ControlTestApp
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			var vm = new TestAppViewModel();
			vm.AutoSuggestConsumerViewModel = new AutoSuggestConsumerViewModel();

			DataContext = vm;
		}
	}
}
