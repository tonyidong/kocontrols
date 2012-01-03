using System;
using System.Windows;
using ControlTestApp.AutoSuggestTextBox;

namespace ControlTestApp
{
	public class TestAppViewModel : DependencyObject
	{
		public static readonly DependencyProperty AutoSuggestConsumerViewModelProperty = DependencyProperty.Register("AutoSuggestConsumerViewModel", typeof(AutoSuggestConsumerViewModel), typeof(TestAppViewModel));
		public AutoSuggestConsumerViewModel AutoSuggestConsumerViewModel { get { return (AutoSuggestConsumerViewModel)GetValue(AutoSuggestConsumerViewModelProperty); } set { SetValue(AutoSuggestConsumerViewModelProperty, value); } }
	}
}
