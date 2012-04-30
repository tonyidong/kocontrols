using System;
using System.Windows;

namespace ControlTestApp
{
	public class TestAppViewModel : DependencyObject
	{
		public static readonly DependencyProperty AutoSuggestConsumerViewModelProperty = DependencyProperty.Register("AutoSuggestConsumerViewModel", typeof(AutoSuggestConsumerViewModel), typeof(TestAppViewModel));
		public AutoSuggestConsumerViewModel AutoSuggestConsumerViewModel { get { return (AutoSuggestConsumerViewModel)GetValue(AutoSuggestConsumerViewModelProperty); } set { SetValue(AutoSuggestConsumerViewModelProperty, value); } }

		public static readonly DependencyProperty AutoSuggestConsumerViewModelCountiesProperty = DependencyProperty.Register("AutoSuggestConsumerViewModelCounties", typeof(AutoSuggestConsumerViewModelCounties), typeof(TestAppViewModel));
		public AutoSuggestConsumerViewModelCounties AutoSuggestConsumerViewModelCounties { get { return (AutoSuggestConsumerViewModelCounties)GetValue(AutoSuggestConsumerViewModelCountiesProperty); } set { SetValue(AutoSuggestConsumerViewModelCountiesProperty, value); } }

		public static readonly DependencyProperty AutoSuggestConsumerViewModelComboBoxProperty = DependencyProperty.Register("AutoSuggestConsumerViewModelComboBox", typeof(AutoSuggestConsumerViewModelComboBox), typeof(TestAppViewModel));
		public AutoSuggestConsumerViewModelComboBox AutoSuggestConsumerViewModelComboBox { get { return (AutoSuggestConsumerViewModelComboBox)GetValue(AutoSuggestConsumerViewModelComboBoxProperty); } set { SetValue(AutoSuggestConsumerViewModelComboBoxProperty, value); } }

		public static readonly DependencyProperty AutoSuggestConsumerViewModelComboBoxMSProperty = DependencyProperty.Register("AutoSuggestConsumerViewModelComboBoxMS", typeof(AutoSuggestConsumerViewModelComboBox), typeof(TestAppViewModel));
		public AutoSuggestConsumerViewModelComboBox AutoSuggestConsumerViewModelComboBoxMS { get { return (AutoSuggestConsumerViewModelComboBox)GetValue(AutoSuggestConsumerViewModelComboBoxMSProperty); } set { SetValue(AutoSuggestConsumerViewModelComboBoxMSProperty, value); } }

		public static readonly DependencyProperty AutoSuggestConsumerViewModelDataGridProperty = DependencyProperty.Register("AutoSuggestConsumerViewModelDataGrid", typeof(AutoSuggestConsumerViewModelDataGrid), typeof(TestAppViewModel));
		public AutoSuggestConsumerViewModelDataGrid AutoSuggestConsumerViewModelDataGrid { get { return (AutoSuggestConsumerViewModelDataGrid)GetValue(AutoSuggestConsumerViewModelDataGridProperty); } set { SetValue(AutoSuggestConsumerViewModelDataGridProperty, value); } }
	}
}
