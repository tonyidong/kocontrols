using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KO.Controls.Common.Command;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace KO.Controls
{
	public class AutoSuggestViewModel<T>:DependencyObject
	{
		#region IsButtonPanelVisible
		public static DependencyProperty IsButtonPanelVisibleProperty =
			DependencyProperty.Register("IsButtonPanelVisible", typeof(bool), typeof(AutoSuggestViewModel<T>),
			new PropertyMetadata());

		public bool IsButtonPanelVisible { get { return (bool)GetValue(IsButtonPanelVisibleProperty); } set { SetValue(IsButtonPanelVisibleProperty, value); } }
		#endregion 

		public ObservableCollection<CommandViewModel> Commands { get; private set; }

		#region Items Source
		public static DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IList<T>), typeof(AutoSuggestViewModel<T>));

		public IList<T> ItemsSource { get { return (IList<T>)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
		#endregion 
		
		#region Filter Items Command
		public static DependencyProperty FilterItemsProperty =
			DependencyProperty.Register("FilterItems", typeof(ICommand), typeof(AutoSuggestViewModel<T>));

		public ICommand FilterItems { get { return (ICommand)GetValue(FilterItemsProperty); } set { SetValue(FilterItemsProperty, value); } }
		#endregion 

		public AutoSuggestViewModel()
		{ 				
			Commands = new ObservableCollection<CommandViewModel>();
			Commands.CollectionChanged += (s, a) =>
			{
				IsButtonPanelVisible = Commands != null && Commands.Count > 0;
			};
			IsButtonPanelVisible = false;
		}

		protected virtual void DoFilterItems(string x)
		{

		}
	}
}
