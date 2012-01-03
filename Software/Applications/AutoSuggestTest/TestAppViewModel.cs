using System;
using System.Windows;

namespace ControlTestApp
{
	public class TestAppViewModel : DependencyObject
	{
		public static readonly DependencyProperty AutoSuggestConsumerViewModelProperty = DependencyProperty.Register("AutoSuggestConsumerViewModel", typeof(AutoSuggestConsumerViewModel), typeof(TestAppViewModel));
		public AutoSuggestConsumerViewModel AutoSuggestConsumerViewModel { get { return (AutoSuggestConsumerViewModel)GetValue(AutoSuggestConsumerViewModelProperty); } set { SetValue(AutoSuggestConsumerViewModelProperty, value); } }

		public static readonly DependencyProperty CitiesViewModelProperty = DependencyProperty.Register("CitiesViewModel", typeof(CitiesViewModel), typeof(TestAppViewModel));
		public CitiesViewModel CitiesViewModel { get { return (CitiesViewModel)GetValue(CitiesViewModelProperty); } set { SetValue(CitiesViewModelProperty, value); } }
	}
}
