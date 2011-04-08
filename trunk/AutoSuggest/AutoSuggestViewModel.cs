using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KO.Controls.Common.Command;
using System.Collections.ObjectModel;

namespace KO.Controls
{
	public class AutoSuggestViewModel:DependencyObject
	{
		#region IsButtonPanelVisible
		public static DependencyProperty IsButtonPanelVisibleProperty =
			DependencyProperty.Register("IsButtonPanelVisible", typeof(bool), typeof(AutoSuggestViewModel),
			new PropertyMetadata());

		
		public bool IsButtonPanelVisible { get { return (bool)GetValue(IsButtonPanelVisibleProperty); } set { SetValue(IsButtonPanelVisibleProperty, value); } }
		#endregion 

		public ObservableCollection<CommandViewModel> Commands { get; private set; }

		//Make this dependency property
		public IList<object> ItemsSource { get; set; }

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
