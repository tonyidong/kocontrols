using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace ControlTestApp
{
	public partial class AutoSuggestTextBoxTest : UserControl
	{
		public AutoSuggestTextBoxTest()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var dataContext = DataContext as TestAppViewModel;
			if(dataContext != null && dataContext.AutoSuggestConsumerViewModel != null)
				dataContext.AutoSuggestConsumerViewModel.AutoSuggestVM.Suggestion = dataContext.AutoSuggestConsumerViewModel.AllCities.First(x => x.Key == 3);
		}
	}
}
