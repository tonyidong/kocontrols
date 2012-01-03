using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlTestApp.AutoSuggestTextBox
{
	/// <summary>
	/// Interaction logic for AutoSuggestTextBoxTestDataGrid.xaml
	/// </summary>
	public partial class AutoSuggestTextBoxTestDataGrid : UserControl
	{
		public AutoSuggestTextBoxTestDataGrid()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			AutoSuggestConsumerViewModel vm = (AutoSuggestConsumerViewModel)this.DataContext;
			vm.AutoSuggestVM.SelectedSuggestion = vm.AllCities[2];
		}
	}
}
