using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace KO.Controls
{
	[Flags]
	public enum SelectionTrigger
	{
		Space = 1,
		Tab = 2,
		Arrows = 4,
		SpaceTab = Space | Tab
	}

	public class AutoSuggest : Popup
	{
		#region TargetTextBox
		public static DependencyProperty TargetTextBoxProperty =
			DependencyProperty.Register("TargetTextBox",typeof(TextBox),typeof(AutoSuggest),
			new PropertyMetadata(new PropertyChangedCallback(TargetTextBox_Changed)));

		public TextBox TargetTextBox{get{return (TextBox)GetValue(TargetTextBoxProperty);}	set	{SetValue(TargetTextBoxProperty, value);}}
		#endregion 

		#region SuggestionsListView
		public static DependencyProperty SuggestionsListViewProperty =
			DependencyProperty.Register("SuggestionsListView", typeof(ListView), typeof(AutoSuggest),
			new PropertyMetadata(new PropertyChangedCallback(SuggestionsListView_Changed)));

		public ListView SuggestionsListView { get { return (ListView)GetValue(SuggestionsListViewProperty); } set { SetValue(SuggestionsListViewProperty, value); } }
		#endregion 

		#region SelectedSuggestion
		public static DependencyProperty SelectedSuggestionProperty =
			DependencyProperty.Register("SelectedSuggestion", typeof(object), typeof(AutoSuggest));

		public object SelectedSuggestion { get { return GetValue(SelectedSuggestionProperty); } set { SetValue(SelectedSuggestionProperty, value); } }
		#endregion 

		#region SelectedSuggestionPreview
		public static DependencyProperty SelectedSuggestionPreviewProperty =
			DependencyProperty.Register("SelectedSuggestionPreview", typeof(object), typeof(AutoSuggest));

		public object SelectedSuggestionPreview { get { return GetValue(SelectedSuggestionPreviewProperty); } set { SetValue(SelectedSuggestionPreviewProperty, value); } }
		#endregion 

		//Tabout property 
		
		//Always exit on escape
		//Always select on Enter

		// TaboutOnSelection SelectionTrigger
		#region SuggestionsListView
		//public static DependencyProperty SuggestionsListViewProperty =
		//    DependencyProperty.Register("SuggestionsListView", typeof(ListView), typeof(AutoSuggest),
		//    new PropertyMetadata(new PropertyChangedCallback(SuggestionsListView_Changed)));

		//public ListView SuggestionsListView { get { return (ListView)GetValue(SuggestionsListViewProperty); } set { SetValue(SuggestionsListViewProperty, value); } }
		#endregion 

		private bool IsTargetTextBoxEditable { get { return TargetTextBox != null && !TargetTextBox.IsReadOnly; } }
		private SuggestionsControl suggestionsControl = null;

		public AutoSuggest()
		{
			suggestionsControl = new SuggestionsControl();
			this.Child = suggestionsControl;

			//TBD: Create the bindings for suggestionsControl assuming DataContext of tpe AutoSuggestViewModel.
			//
		}

		#region Event Handlers
		private static void TargetTextBox_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			AutoSuggest autoSuggest = (AutoSuggest)sender;
			if (args.OldValue != null)
			{
				TextBox oldTextBox = (TextBox)args.OldValue;
				oldTextBox.PreviewKeyDown -= new System.Windows.Input.KeyEventHandler(autoSuggest.TargetTextBox_PreviewKeyDown);
				oldTextBox.KeyUp -= new System.Windows.Input.KeyEventHandler(autoSuggest.TargetTextBox_KeyUp);
				oldTextBox.LostKeyboardFocus -= new System.Windows.Input.KeyboardFocusChangedEventHandler(autoSuggest.TargetTextBox_LostKeyboardFocus);
			}

			if (args.NewValue != null)
			{
				TextBox newTextBox = (TextBox)args.NewValue;
				autoSuggest.PlacementTarget = newTextBox;
				newTextBox.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(autoSuggest.TargetTextBox_PreviewKeyDown);
				newTextBox.KeyUp += new System.Windows.Input.KeyEventHandler(autoSuggest.TargetTextBox_KeyUp);
				newTextBox.LostKeyboardFocus += new System.Windows.Input.KeyboardFocusChangedEventHandler(autoSuggest.TargetTextBox_LostKeyboardFocus);
			}
		}

		private static void SuggestionsListView_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			AutoSuggest autoSuggest = (AutoSuggest)sender;
			if (args.OldValue != null)
			{
				ListView oldListView = (ListView)args.NewValue;
				//Remove events
			}

			if (args.NewValue != null)
			{
				ListView newListView = (ListView)args.NewValue;
				autoSuggest.suggestionsControl.itemsSuggestionsListViewContainer.Child = newListView;
			}
		}

		private void TargetTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Down && this.IsOpen)
			{
				//this.lv.Focus();
			}
		}

		private void TargetTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			
		}

		private void TargetTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (IsTargetTextBoxEditable)
			{
				//TBD: Open the popup
			}
		}
		#endregion 
	}
}
