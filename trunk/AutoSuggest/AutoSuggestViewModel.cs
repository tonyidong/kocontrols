using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KO.Controls.Common.Command;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections;

namespace KO.Controls
{
	public delegate string GetSelectedSuggestionFormattedName (object SelectedSuggestion);

	public class AutoSuggestViewModel:DependencyObject
	{
		#region IsButtonPanelVisible
		public static DependencyProperty IsButtonPanelVisibleProperty =
			DependencyProperty.Register("IsButtonPanelVisible", typeof(bool), typeof(AutoSuggestViewModel),
			new PropertyMetadata());

		public bool IsButtonPanelVisible { get { return (bool)GetValue(IsButtonPanelVisibleProperty); } set { SetValue(IsButtonPanelVisibleProperty, value); } }
		#endregion 

		#region Items Source
		public static DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoSuggestViewModel));

		public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
		#endregion 
		
		#region SelectedSuggestionPreview
		public static DependencyProperty SelectedSuggestionPreviewProperty =
			DependencyProperty.Register("SelectedSuggestionPreview", typeof(object), typeof(AutoSuggest));

		public object SelectedSuggestionPreview { get { return GetValue(SelectedSuggestionPreviewProperty); } set { SetValue(SelectedSuggestionPreviewProperty, value); } }
		#endregion 

		#region SelectedSuggestion
		public static DependencyProperty SelectedSuggestionProperty =
			DependencyProperty.Register("SelectedSuggestion", typeof(object), typeof(AutoSuggestViewModel));

		public object SelectedSuggestion { get { return GetValue(SelectedSuggestionProperty); } set { SetValue(SelectedSuggestionProperty, value); if (GetSelectedSuggestionFormattedName != null) SelectedSuggestionName = GetSelectedSuggestionFormattedName(value); } }
		#endregion 

		#region Filter Items Command
		public static DependencyProperty FilterItemsProperty =
			DependencyProperty.Register("FilterItems", typeof(ICommand), typeof(AutoSuggestViewModel));

		public ICommand FilterItems { get { return (ICommand)GetValue(FilterItemsProperty); } set { SetValue(FilterItemsProperty, value); } }
		#endregion 

		#region Selected Suggestion Name
		public static readonly DependencyProperty SelectedSuggestionNameProperty =
			 DependencyProperty.Register("SelectedSuggestionName", typeof(string),
			 typeof(AutoSuggestViewModel), null);

		public string SelectedSuggestionName
		{
			get { return (string)GetValue(SelectedSuggestionNameProperty); }
			set { SetValue(SelectedSuggestionNameProperty, value); }
		}
		#endregion

		public ObservableCollection<CommandViewModel> Commands { get; private set; }
		public GetSelectedSuggestionFormattedName GetSelectedSuggestionFormattedName { get; set; }

		public AutoSuggestViewModel(GetSelectedSuggestionFormattedName getSelectedSuggestionFormattedName)
			: this()
		{
			this.GetSelectedSuggestionFormattedName = getSelectedSuggestionFormattedName;
		}

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
