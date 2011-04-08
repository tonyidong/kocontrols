using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KO.Controls.Common.Command;
using System.Collections.ObjectModel;

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

		public AutoSuggestViewModel()
		{
			Commands = new ObservableCollection<CommandViewModel>();
			Commands.CollectionChanged += (s, a) =>
			{
				IsButtonPanelVisible = Commands != null && Commands.Count > 0;
			};
			IsButtonPanelVisible = false;
		}
	}
}
