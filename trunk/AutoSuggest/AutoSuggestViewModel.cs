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
	public delegate string GetSelectedSuggestionFormattedName (object SelectedSuggestion, bool isConfirm = false);

	public class AutoSuggestViewModel:DependencyObject
	{
		#region Properties
		public bool CodeInput { get; private set; }
		public bool DoNotChangeText { get; private set; }

		public ObservableCollection<CommandViewModel> Commands { get; private set; }
		public GetSelectedSuggestionFormattedName GetSelectedSuggestionFormattedName { get; set; }
		#endregion 

		#region IsButtonPanelVisible
		public static DependencyProperty IsButtonPanelVisibleProperty =
			DependencyProperty.Register("IsButtonPanelVisible", typeof(bool), typeof(AutoSuggestViewModel),
			new PropertyMetadata());

		public bool IsButtonPanelVisible { get { return (bool)GetValue(IsButtonPanelVisibleProperty); } set { SetValue(IsButtonPanelVisibleProperty, value); } }
		#endregion 

		#region IsButtonPanelVisible
		public static DependencyProperty IsInvalidTextAllowedProperty =
			DependencyProperty.Register("IsInvalidTextAllowed", typeof(bool), typeof(AutoSuggestViewModel),
			new PropertyMetadata());

		public bool IsInvalidTextAllowed { get { return (bool)GetValue(IsInvalidTextAllowedProperty); } set { SetValue(IsInvalidTextAllowedProperty, value); } }
		#endregion

		#region Items Source
		public static DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoSuggestViewModel));

		public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
		#endregion 
		
		#region SelectedSuggestionPreview
		public static DependencyProperty SelectedSuggestionPreviewProperty =
			DependencyProperty.Register("SelectedSuggestionPreview", typeof(object), typeof(AutoSuggest)
			,new PropertyMetadata(new PropertyChangedCallback((x, y) =>
			{
				AutoSuggestViewModel vm1 = (AutoSuggestViewModel)x;
				if (!vm1.DoNotChangeText)
					vm1.TextBoxText = vm1.GetSelectedSuggestionFormattedName(y.NewValue);
			})));

		public object SelectedSuggestionPreview { get { return GetValue(SelectedSuggestionPreviewProperty); } set { SetValue(SelectedSuggestionPreviewProperty, value); } }
		#endregion 

		#region SelectedSuggestion
		public static DependencyProperty SelectedSuggestionProperty =
			DependencyProperty.Register("SelectedSuggestion", typeof(object), typeof(AutoSuggestViewModel)
			,new PropertyMetadata(new PropertyChangedCallback((x, y) =>
			{
				AutoSuggestViewModel vm1 = (AutoSuggestViewModel)x;
				if (!vm1.DoNotChangeText)
				{
					vm1.CodeInput = true;
					vm1.TextBoxText = vm1.GetSelectedSuggestionFormattedName(y.NewValue,true);
					vm1.CodeInput = false;
				}
			})));

		public object SelectedSuggestion { get { return GetValue(SelectedSuggestionProperty); } set { SetValue(SelectedSuggestionProperty, value); } }
		#endregion 

		#region Filter Items Command
		public static DependencyProperty FilterItemsProperty =
			DependencyProperty.Register("FilterItems", typeof(ICommand), typeof(AutoSuggestViewModel));

		public ICommand FilterItems { get { return (ICommand)GetValue(FilterItemsProperty); } set { SetValue(FilterItemsProperty, value); } }
		#endregion 

		#region Selected Suggestion Name
		public static readonly DependencyProperty TextBoxTextProperty =
			 DependencyProperty.Register("TextBoxText", typeof(string),
			 typeof(AutoSuggestViewModel), null);

		public string TextBoxText
		{
			get { return (string)GetValue(TextBoxTextProperty); }
			set { SetValue(TextBoxTextProperty, value); }
		}
		#endregion

		#region Constructors
		public AutoSuggestViewModel()
		{
			CodeInput = false;
			Commands = new ObservableCollection<CommandViewModel>();
			Commands.CollectionChanged += (s, a) =>
			{
				IsButtonPanelVisible = Commands != null && Commands.Count > 0;
			};
			IsButtonPanelVisible = false;
		}

		public AutoSuggestViewModel(GetSelectedSuggestionFormattedName getSelectedSuggestionFormattedName)
			: this()
		{
			this.GetSelectedSuggestionFormattedName = getSelectedSuggestionFormattedName;
		}
		#endregion 

		protected virtual void DoFilterItems(string x)
		{

		}

		internal void SetSuggestionToNullButLeaveTextUnchanged()
		{
			DoNotChangeText = true;
			SelectedSuggestion = null;
			DoNotChangeText = false;
		}
	}
}
