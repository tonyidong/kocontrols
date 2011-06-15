using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace KO.Controls
{
	#region Enums
	[Flags]
	public enum SelectionTrigger
	{
		Space = 1,
		Tab = 2,
		Arrows = 4,
		SpaceTab = Space | Tab,
		SpaceArrows = Space | Arrows,
		TabArrows = Tab | Arrows,
		SpaceTabArrows = Space | Tab | Arrows
	}

	[Flags]
	public enum TaboutTrigger
	{
		Space = 1,
		Tab = 2,
		Arrows = 4,
		Enter = 8,
		Escape = 16,
		SpaceTab = Space | Tab,
		SpaceArrows = Space | Arrows,
		SpaceEnter = Space | Enter,
		SpaceEscape = Space | Escape,
		TabArrows = Tab | Arrows,
		TabEnter = Tab | Enter,
		TabEscape = Tab | Escape,
		ArrowsEnter = Arrows | Enter,
		ArrowsEscape = Arrows | Escape,
		EnterEscape = Enter | Escape,
		SpaceTabArrows = SpaceTab | Arrows,
		SpaceTabEnter = SpaceTab | Enter,
		SpaceTabEscape = SpaceTab | Escape,
		TabArrowsEnter = TabArrows | Enter,
		TabArrowsEscape = TabArrows | Escape,
		ArrowsEnterEscape = ArrowsEnter | Escape,
		SpaceTabArrowsEnter = SpaceTab | ArrowsEnter,
		SpaceTabArrowsEscape = SpaceTab | ArrowsEscape,
		TabArrowsEnterEscape = TabArrows | EnterEscape,
		SpaceTabArrowsEnterEscape = SpaceTab | ArrowsEnter | Escape
	}
	#endregion 

	//  Copy/Paste does not work.
	public class AutoSuggest : Popup
	{
		#region Overriding DataContext
		static AutoSuggest()
		{
			DataContextProperty.OverrideMetadata(typeof(AutoSuggest), new FrameworkPropertyMetadata
			(
                null,
                new PropertyChangedCallback((d, v) => { if (v != null && v.NewValue != null && v.NewValue is AutoSuggestViewModel) { InitializeText(d as AutoSuggest, v.NewValue as AutoSuggestViewModel); } }),
			    new CoerceValueCallback((d, v) => { if(v != null && !(v is AutoSuggestViewModel)) throw new NotSupportedException(); return v; })
			));
		}
		public AutoSuggestViewModel DataContextAutoSuggestVM { get { return (AutoSuggestViewModel)base.DataContext; }}
		#endregion 

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

		#region Selection Trigger
		public static DependencyProperty SelectionCommandProperty =
			DependencyProperty.Register("SelectionCommand", typeof(SelectionTrigger), typeof(AutoSuggest), 
			new PropertyMetadata(SelectionTrigger.SpaceTabArrows));
		
		public SelectionTrigger SelectionCommand { get { return (SelectionTrigger)GetValue(SelectionCommandProperty); } set { SetValue(SelectionCommandProperty, value); } }
		#endregion 

		#region Tabout On Selection
		public static DependencyProperty TaboutCommandProperty =
			DependencyProperty.Register("TaboutCommand", typeof(TaboutTrigger), typeof(AutoSuggest), 
			new PropertyMetadata(TaboutTrigger.Enter));

		public TaboutTrigger TaboutCommand { get { return (TaboutTrigger)GetValue(TaboutCommandProperty); } set { SetValue(TaboutCommandProperty, value); } }
		#endregion 

		#region Properties
		private SuggestionsControl suggestionsControl = null;
		private bool cancelWindowKeyDown = false;
		private bool selectingItemOrClosingPopup = false;
        private Window window;

		private ListView currentSuggestionsListView
		{
			get
			{
				if(suggestionsControl.itemsSuggestionsListViewContainer.Child != null && suggestionsControl.itemsSuggestionsListViewContainer.Child is ListView)
					return suggestionsControl.itemsSuggestionsListViewContainer.Child as ListView;
				return null;
			}
		}

		private bool isTargetTextBoxEditable { get { return TargetTextBox != null && !TargetTextBox.IsReadOnly; } }
		
		#endregion 

		#region Constructors
		public AutoSuggest()
		{
			
			suggestionsControl = new SuggestionsControl();

			this.Child = suggestionsControl;
            this.PreviewMouseDown += AutoSuggest_PreviewMouseDown;
            this.AllowsTransparency = true;
            this.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
		}
		#endregion 

		#region Event Handlers

		#region Popup Event Handlers
		private void AutoSuggest_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.Focus();
		}

		protected override void OnClosed(EventArgs e)
		{
            if (window != null)
			{
				window.PreviewKeyDown -= Window_PreviewKeyDown;
				window.Deactivated -= window_Deactivated;
			}

            base.OnClosed(e);
        }

		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);

            //Fixed bug from Microsoft (Window.GetWindow can return null) :-)
            if (window != null)
			{
				window.PreviewKeyDown -= Window_PreviewKeyDown;
				window.Deactivated -= window_Deactivated;
			}
            window = Window.GetWindow(this);
			if (window != null)
			{
				window.PreviewKeyDown += Window_PreviewKeyDown;
				window.Deactivated += window_Deactivated;
			}
		}
		private void window_Deactivated(object sender, EventArgs e)
		{
			IsOpen = false;
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(window != null) window.Activate();

			if (e.Handled) return;

			Dispatcher.BeginInvoke((Action)delegate
			{
				if (!cancelWindowKeyDown)
				{
					if (e.Key == Key.Escape)
					{
						IsOpen = false;
						e.Handled = true;
					}
				}
				cancelWindowKeyDown = true;
			});
		}
		#endregion 

		#region TargetTextBox Event Handlers
		private static void TargetTextBox_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			AutoSuggest autoSuggest = (AutoSuggest)sender;
			if (args.OldValue != null)
			{
				TextBox oldTextBox = (TextBox)args.OldValue;
				oldTextBox.PreviewKeyDown -= autoSuggest.TargetTextBox_PreviewKeyDown;
				oldTextBox.TextChanged -=autoSuggest.TargetTextBox_TextChanged;
				oldTextBox.LostKeyboardFocus -= autoSuggest.TargetTextBox_LostKeyboardFocus;
				oldTextBox.GotKeyboardFocus -= autoSuggest.TargetTextBox_GotKeyboardFocus;
			}

			if (args.NewValue != null)
			{
				TextBox newTextBox = (TextBox)args.NewValue;
				autoSuggest.PlacementTarget = newTextBox;
				newTextBox.PreviewKeyDown += autoSuggest.TargetTextBox_PreviewKeyDown;
				newTextBox.TextChanged += autoSuggest.TargetTextBox_TextChanged;
				newTextBox.LostKeyboardFocus += autoSuggest.TargetTextBox_LostKeyboardFocus;
				newTextBox.GotKeyboardFocus += autoSuggest.TargetTextBox_GotKeyboardFocus;

                InitializeText(autoSuggest, autoSuggest.DataContextAutoSuggestVM);
			}
		}

		private void TargetTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if(String.IsNullOrEmpty(TargetTextBox.Text))
				OpenSuggestions();
		}

		private void TargetTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (selectingItemOrClosingPopup || (DataContextAutoSuggestVM != null && DataContextAutoSuggestVM.CodeInput)) { return; }
			
			if (isTargetTextBoxEditable)
			{
				OpenSuggestions();

				if (currentSuggestionsListView != null)
				{
					if (currentSuggestionsListView.Items.Count > 0)
					{
						currentSuggestionsListView.SelectedIndex = 0;

						selectingItemOrClosingPopup = true;
						try
						{
							if (e.Changes != null && e.Changes.Count > 0)
							{
								TextChange txtChange = e.Changes.First<TextChange>();

								if (txtChange.AddedLength > 0)
								{
									int indx = TargetTextBox.CaretIndex;
									string n = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestionPreview);
									TargetTextBox.Text = n;
									TargetTextBox.CaretIndex = indx;

									if (TargetTextBox.Text.Length > indx)
									{
										int selectionLength = TargetTextBox.Text.Length - indx;
										TargetTextBox.Select(indx, selectionLength);
									}
								}
							}
						}
						finally
						{
							selectingItemOrClosingPopup = false;
						}
					}
					else
					{
						DataContextAutoSuggestVM.SelectedSuggestionPreview = null;
					}
				}
			}	
		}

		private void TargetTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Down && this.IsOpen)
			{
				if (currentSuggestionsListView != null && currentSuggestionsListView.Items.Count > 0 && currentSuggestionsListView.Items.Count > currentSuggestionsListView.SelectedIndex)
					currentSuggestionsListView.SelectedIndex += 1;
			}
			else if (e.Key == System.Windows.Input.Key.Up && this.IsOpen)
			{
				if (currentSuggestionsListView != null && currentSuggestionsListView.Items.Count > 0 && currentSuggestionsListView.SelectedIndex > 0)
					currentSuggestionsListView.SelectedIndex -= 1;
			}
			//TBD: How do we find out if the user selected the text or we did.
			//The idea for this is that when we autocomplete the text and the user presses backspace we are deleting the autocompleted text only where the user really wants to delete one more character.
			//The below code resolves that problem but introduces another problem. If the user manually selects 5 characters and presses the back button to delete them because of this code we delete one more character after the selection.
			//else if (e.Key == Key.Back || e.Key == Key.Delete)
			//{
			//    if (!String.IsNullOrEmpty(TargetTextBox.SelectedText) && TargetTextBox.Text.IndexOf(TargetTextBox.SelectedText) == TargetTextBox.CaretIndex)
			//    {
			//        TargetTextBox.Select(TargetTextBox.CaretIndex-1,TargetTextBox.SelectedText.Length+1);
			//    }
			//}
			else if(this.IsOpen)
			{
				if (e.Key == Key.Space && (SelectionCommand & SelectionTrigger.Space) == SelectionTrigger.Space
					&& (currentSuggestionsListView == null || currentSuggestionsListView.Items.Count != 1))
					return;

				e.Handled = SelectItemAndTaboutByKeyDownValue(e.Key);
			}
		}

		private void TargetTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			HandleLostFocus();
		}
		#endregion 

		#region Suggestions ListView Event Handlers

		private static void SuggestionsListView_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			AutoSuggest autoSuggest = (AutoSuggest)sender;
			if (args.OldValue != null)
			{
				ListView oldListView = (ListView)args.NewValue;

				oldListView.PreviewKeyDown -= autoSuggest.ListView_PreviewKeyDown;
				oldListView.LostKeyboardFocus -= autoSuggest.ListView_LostKeyboardFocus;
				oldListView.SelectionChanged -= autoSuggest.ListView_SelectionChanged;

				oldListView.RemoveHandler(ListViewItem.MouseDoubleClickEvent, (RoutedEventHandler)autoSuggest.ListViewItemDoubleClickHandler);
			}

			if (args.NewValue != null)
			{
				ListView newListView = (ListView)args.NewValue;
				autoSuggest.suggestionsControl.itemsSuggestionsListViewContainer.Child = newListView;

				newListView.PreviewKeyDown += autoSuggest.ListView_PreviewKeyDown;
				newListView.LostKeyboardFocus += autoSuggest.ListView_LostKeyboardFocus;
				newListView.SelectionChanged += autoSuggest.ListView_SelectionChanged;

				newListView.AddHandler(ListViewItem.MouseDoubleClickEvent, (RoutedEventHandler)autoSuggest.ListViewItemDoubleClickHandler,true);
			}
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
				DataContextAutoSuggestVM.SelectedSuggestionPreview = e.AddedItems[0];
		}

		private void ListViewItemDoubleClickHandler(object sender, RoutedEventArgs args)
		{
			if (currentSuggestionsListView != null && currentSuggestionsListView.SelectedIndex > 0)
			{
				selectingItemOrClosingPopup = true;
				TargetTextBox.Text =  DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(currentSuggestionsListView.SelectedItem);
				selectingItemOrClosingPopup = false;

				SelectItemAndClose();
			}
		}

		private void ListView_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			HandleLostFocus();
		}

		private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			ListView lv = (ListView)sender;
			if (lv.SelectedItem != null)
				e.Handled = SelectItemAndTaboutByKeyDownValue(e.Key);
		}
		#endregion 

		#endregion 

		#region Private Methods
		private static void InitializeText(AutoSuggest autoSuggest, AutoSuggestViewModel autoSuggestVM)
		{
			if (autoSuggest != null)
			{
				autoSuggest.SetSuggestedText();

				if (autoSuggestVM != null)
				{
					DependencyPropertyDescriptor itemSelectedPreviewDescr = DependencyPropertyDescriptor.FromProperty(AutoSuggestViewModel.SelectedSuggestionProperty, typeof(AutoSuggestViewModel));
					if (itemSelectedPreviewDescr != null)
					{
						itemSelectedPreviewDescr.AddValueChanged(autoSuggestVM, delegate
						{
							autoSuggest.SetSuggestedText();
						});
					}
				}
			}
		}

		private void SetSuggestedText()
		{
			TargetTextBox.TextChanged -= TargetTextBox_TextChanged;

			if (DataContextAutoSuggestVM != null && TargetTextBox != null)
			{
				if (DataContextAutoSuggestVM.SelectedSuggestion != null)
					TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion);
				else
					TargetTextBox.Text = "";
			}

			TargetTextBox.TextChanged += TargetTextBox_TextChanged;
		}

		private void OpenSuggestions()
		{
			if (isTargetTextBoxEditable)
			{
				if (DataContextAutoSuggestVM.FilterItems != null)
					DataContextAutoSuggestVM.FilterItems.Execute(TargetTextBox.Text);

				//IF there are suggestions or command open the popup
				if (currentSuggestionsListView != null && (currentSuggestionsListView.Items.Count > 0 
							|| (DataContextAutoSuggestVM != null && DataContextAutoSuggestVM.Commands.Count > 0)))
					this.IsOpen = true;
				else
					this.IsOpen = false;
			}
		}

		private void HandleLostFocus()
		{
			if ((!this.IsKeyboardFocused && !this.IsKeyboardFocusWithin)
				&& (TargetTextBox != null && !TargetTextBox.IsKeyboardFocused)
				&& (currentSuggestionsListView != null && !currentSuggestionsListView.IsKeyboardFocused))
			{
				if (currentSuggestionsListView.SelectedIndex >= 0)
				{
					ListViewItem item = currentSuggestionsListView.ItemContainerGenerator.ContainerFromIndex(currentSuggestionsListView.SelectedIndex) as ListViewItem;
					if (item != null && (item.IsKeyboardFocused || item.IsKeyboardFocusWithin))
						return;
				}

				this.IsOpen = false;

				if (String.IsNullOrEmpty(TargetTextBox.Text))
				{
					DataContextAutoSuggestVM.SelectedSuggestion = null;
				}
				else
				{
					if (DataContextAutoSuggestVM.IsInvalidTextAllowed)
					{
						if (DataContextAutoSuggestVM.SelectedSuggestionPreview == null)
						{
							if (DataContextAutoSuggestVM.SelectedSuggestion != null)
							{
								if (TargetTextBox.Text != DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion))
									DataContextAutoSuggestVM.SetSuggestionToNullButLeaveTextUnchanged();
							}
						}
						else
						{
							if (TargetTextBox.Text != DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestionPreview))
							{
								DataContextAutoSuggestVM.SetSuggestionToNullButLeaveTextUnchanged();
							}
							else
							{
								selectingItemOrClosingPopup = true;
								if (DataContextAutoSuggestVM.SelectedSuggestion != null)
									TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion);
								else
									TargetTextBox.Text = "";
								selectingItemOrClosingPopup = false;
							}
						}
					}
					else
					{
						selectingItemOrClosingPopup = true;
						if (DataContextAutoSuggestVM.SelectedSuggestion != null)
							TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion);
						else
							TargetTextBox.Text = "";
						selectingItemOrClosingPopup = false;
					}
				}
				DataContextAutoSuggestVM.SelectedSuggestionPreview = null;
			}
		}

		private bool SelectItemAndTaboutByKeyDownValue(Key keyDown)
		{
			bool handled = false;
			//Determine whether to select and close the autosuggest
			if (keyDown == Key.Enter)
			{
				SelectItemAndClose();
				if ((TaboutCommand & TaboutTrigger.Enter) == TaboutTrigger.Enter)
					TabOutNext();
				handled = true;
			}
			else if (keyDown == Key.Escape)
			{
				this.IsOpen = false;
				//selectingItemOrClosingPopup = true;
                cancelWindowKeyDown = true;
			}
			else if (keyDown == Key.Right && keyDown == Key.Left)
			{
				if ((SelectionCommand & SelectionTrigger.Arrows) == SelectionTrigger.Arrows)
				{
					SelectItemAndClose();
					handled = true;
				}

				if ((TaboutCommand & TaboutTrigger.Arrows) == TaboutTrigger.Arrows)
				{
					if (keyDown == Key.Right)
						TabOutNext();
					else
						TabOutPrevious();
					handled = true;
				}
			}
			else if (keyDown == Key.Space)
			{
				if ((SelectionCommand & SelectionTrigger.Space) == SelectionTrigger.Space)
				{
					SelectItemAndClose();
					handled = true;
				}

				if ((TaboutCommand & TaboutTrigger.Space) == TaboutTrigger.Space)
				{
					TabOutNext();
					handled = true;
				}
			}
			else if (keyDown == Key.Tab)
			{
				if ((SelectionCommand & SelectionTrigger.Tab) == SelectionTrigger.Tab)
				{
					SelectItemAndClose();
					handled = true;
				}

				if ((TaboutCommand & TaboutTrigger.Tab) == TaboutTrigger.Tab)
				{
					TabOutNext();
					handled = true;
				}
			}

			return handled;
		}

		//private void FocusSelectedListViewElement()
		//{
		//    if (currentSuggestionsListView.SelectedIndex >= 0)
		//    {
		//        ListViewItem item = currentSuggestionsListView.ItemContainerGenerator.ContainerFromIndex(currentSuggestionsListView.SelectedIndex) as ListViewItem;
		//        if (item != null)
		//            item.Focus();
		//    }
		//}

		private void SelectItemAndClose()
		{
			selectingItemOrClosingPopup = true;

			DataContextAutoSuggestVM.SelectedSuggestionPreview = currentSuggestionsListView.SelectedItem;
			DataContextAutoSuggestVM.SelectedSuggestion = currentSuggestionsListView.SelectedItem;

			SetSuggestedText(); //This is a minor duplication of code with the Value Changed event of the SelectedSuggestion property but we need it in cases where the user deletes part of the text then selects the same value. In that case Value Changed is not fired and the Text in the textbox gets cleared on lost focus.

			this.IsOpen = false;
			TargetTextBox.CaretIndex = TargetTextBox.Text.Length;
			
			selectingItemOrClosingPopup = false;
		}

		private void TabOutNext()
		{
			TargetTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			selectingItemOrClosingPopup = true;
			this.IsOpen = false;
			selectingItemOrClosingPopup = false;
		}

		private void TabOutPrevious()
		{
			TargetTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
			selectingItemOrClosingPopup = true;
			this.IsOpen = false;
		}
		#endregion 
	}
}
