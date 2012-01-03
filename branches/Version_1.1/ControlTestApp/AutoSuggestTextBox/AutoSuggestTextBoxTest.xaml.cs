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
	/// Interaction logic for AutoSuggestTextBoxTest.xaml
	/// </summary>
	public partial class AutoSuggestTextBoxTest : UserControl
	{
		public AutoSuggestTextBoxTest()
		{
			InitializeComponent();

			//autoSuggest.TaboutCommand = KO.Controls.TaboutTrigger.Enter;
			//autoSuggest.SelectionCommand = KO.Controls.SelectionTrigger.SpaceTabArrows;
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			AutoSuggestConsumerViewModel vm = (AutoSuggestConsumerViewModel)this.DataContext;
			vm.AutoSuggestVM.SelectedSuggestion = vm.AllCities[2];
		}
	}
}
