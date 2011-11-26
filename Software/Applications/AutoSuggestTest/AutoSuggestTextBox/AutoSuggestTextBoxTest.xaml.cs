using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using KOControls.Core;

namespace ControlTestApp
{
	public partial class AutoSuggestTextBoxTest : UserControl
	{
		static AutoSuggestTextBoxTest()
		{
			DataContextProperty = ViewModel.IsInDesignMode ? ViewModel.RegisterProperty<AutoSuggestConsumerViewModel, AutoSuggestTextBoxTest>("DataContext") : FrameworkElement.DataContextProperty;
		}
		public AutoSuggestTextBoxTest()
		{
			InitializeComponent();
		}

		#region DataContext
		public new static readonly DependencyProperty DataContextProperty;
		public new AutoSuggestConsumerViewModel DataContext { get { return (AutoSuggestConsumerViewModel)GetValue(DataContextProperty); } set { SetValue(DataContextProperty, value); } }
		#endregion

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DataContext.AutoSuggestVM.Suggestion = DataContext.AllCities.First(x => x.Key == 3);
		}
	}
}
