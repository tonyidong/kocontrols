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
			vm.AutoSuggestConsumerViewModelComboBox = new AutoSuggestConsumerViewModelComboBox();
			vm.AutoSuggestConsumerViewModelComboBoxMS = new AutoSuggestConsumerViewModelComboBox();
			vm.AutoSuggestConsumerViewModelDataGrid = new AutoSuggestConsumerViewModelDataGrid();
			vm.AutoSuggestConsumerViewModelCounties = new AutoSuggestConsumerViewModelCounties();

			DataContext = vm;
		}
	}
}
