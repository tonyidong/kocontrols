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
	//InitializeTex hooks up an event handler multiple times. It seems that the RemoveValueChanged extention method does not work properly.
	public class AutoSuggest : Popup
	{
		#region Overriding DataContext
		static AutoSuggest()
		{
			DataContextProperty.OverrideMetadata(typeof(AutoSuggest), new FrameworkPropertyMetadata
			(
                null,
				new PropertyChangedCallback((d, v) => { if (v.NewValue != null) InitializeText(d as AutoSuggest, v.NewValue as AutoSuggestViewModel, v.OldValue as AutoSuggestViewModel); }),
			    new CoerceValueCallback((d, v) => { return v as AutoSuggestViewModel; })
			));
		}
		public AutoSuggestViewModel DataContextAutoSuggestVM { get { return (AutoSuggestViewModel)base.DataContext; }}
		#endregion 

		//#region Tabout Next Count
		//public static DependencyProperty TaboutNextCountProperty =
		//    DependencyProperty.Register("TaboutNextCount", typeof(int), typeof(AutoSuggest),
		//    new PropertyMetadata(1));

		//public int TaboutNextCount { get { return (int)GetValue(TaboutNextCountProperty); } set { SetValue(TaboutNextCountProperty, value); } }
		//#endregion 

		#region TargetTextBox
		public static DependencyProperty TargetTextBoxProperty =
			DependencyProperty.Register("TargetTextBox",typeof(TextBox),typeof(AutoSuggest),
			new PropertyMetadata(new PropertyChangedCallback(TargetTextBox_Changed)));

		public TextBox TargetTextBox{get{return (TextBox)GetValue(TargetTextBoxProperty);}	set	{SetValue(TargetTextBoxProperty, value);}}
		#endregion 

		#region SuggestionsSelector
		public static DependencyProperty SuggestionsSelectorProperty =
			DependencyProperty.Register("SuggestionsSelector", typeof(Selector), typeof(AutoSuggest),
			new PropertyMetadata(new PropertyChangedCallback(SuggestionsSelector_Changed)));

		public Selector SuggestionsSelector { get { return (Selector)GetValue(SuggestionsSelectorProperty); } set { SetValue(SuggestionsSelectorProperty, value); } }
		#region Selector Event Handlers

		private static void SuggestionsSelector_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			AutoSuggest autoSuggest = (AutoSuggest)sender;
			if (args.OldValue != null)
			{
				Selector oldSelector = (Selector)args.NewValue;

				oldSelector.PreviewKeyDown -= autoSuggest.Selector_PreviewKeyDown;
				oldSelector.LostKeyboardFocus -= autoSuggest.Selector_LostKeyboardFocus;
				oldSelector.SelectionChanged -= autoSuggest.Selector_SelectionChanged;

				oldSelector.RemoveHandler(Control.MouseDoubleClickEvent, (RoutedEventHandler)autoSuggest.SelectorItemDoubleClickHandler);
			}

			if (args.NewValue != null)
			{
				Selector newSelector = (Selector)args.NewValue;
				autoSuggest.suggestionsControl.itemsSuggestionsSelectorContainer.Child = newSelector;

				newSelector.PreviewKeyDown += autoSuggest.Selector_PreviewKeyDown;
				newSelector.LostKeyboardFocus += autoSuggest.Selector_LostKeyboardFocus;
				newSelector.SelectionChanged += autoSuggest.Selector_SelectionChanged;

				newSelector.AddHandler(Control.MouseDoubleClickEvent, (RoutedEventHandler)autoSuggest.SelectorItemDoubleClickHandler, true);
			}
		}

		private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0 && DataContextAutoSuggestVM != null)
				DataContextAutoSuggestVM.SelectedSuggestionPreview = e.AddedItems[0];
		}

		private void SelectorItemDoubleClickHandler(object sender, RoutedEventArgs args)
		{
			if (currentSuggestionsSelector != null && currentSuggestionsSelector.SelectedIndex >= 0)
			{
				selectingItemOrClosingPopup = true;
				TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(currentSuggestionsSelector.SelectedItem);
				selectingItemOrClosingPopup = false;

				SelectItemAndClose();
			}
		}

		private void Selector_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			HandleLostFocus();
		}

		private void Selector_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			Selector lv = (Selector)sender;
			if (lv.SelectedItem != null)
				e.Handled = SelectItemAndTaboutByKeyDownValue(e.Key);
		}
		#endregion 
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

		private Selector currentSuggestionsSelector
		{
			get
			{
				if(suggestionsControl.itemsSuggestionsSelectorContainer.Child != null && suggestionsControl.itemsSuggestionsSelectorContainer.Child is Selector)
					return suggestionsControl.itemsSuggestionsSelectorContainer.Child as Selector;
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

				autoSuggest.SetSuggestedText();
			}
		}

		private void TargetTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if(String.IsNullOrEmpty(TargetTextBox.Text))
				OpenSuggestions();
		}

		private void TargetTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (selectingItemOrClosingPopup || DataContextAutoSuggestVM == null || DataContextAutoSuggestVM.CodeInput) { return; }
			
			if (isTargetTextBoxEditable)
			{
				OpenSuggestions();

				if (currentSuggestionsSelector != null)
				{
					if (currentSuggestionsSelector.Items.Count > 0)
					{
						currentSuggestionsSelector.SelectedIndex = 0;

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
						if(DataContextAutoSuggestVM != null)
							DataContextAutoSuggestVM.SelectedSuggestionPreview = null;
					}
				}
			}	
		}

		private void TargetTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Down && this.IsOpen)
			{
				if (currentSuggestionsSelector != null && currentSuggestionsSelector.Items.Count > 0 && currentSuggestionsSelector.Items.Count > currentSuggestionsSelector.SelectedIndex)
					currentSuggestionsSelector.SelectedIndex += 1;
			}
			else if (e.Key == System.Windows.Input.Key.Up && this.IsOpen)
			{
				if (currentSuggestionsSelector != null && currentSuggestionsSelector.Items.Count > 0 && currentSuggestionsSelector.SelectedIndex > 0)
					currentSuggestionsSelector.SelectedIndex -= 1;
			}
			else if(this.IsOpen)
			{
				if (e.Key == Key.Space && (SelectionCommand & SelectionTrigger.Space) == SelectionTrigger.Space
					&& (currentSuggestionsSelector == null || currentSuggestionsSelector.Items.Count != 1))
					return;

				e.Handled = SelectItemAndTaboutByKeyDownValue(e.Key);
			}
		}

		private void TargetTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			HandleLostFocus();
		}
		#endregion 

		#endregion 

		#region Private Methods
		private static void InitializeText(AutoSuggest autoSuggest, AutoSuggestViewModel autoSuggestVM, AutoSuggestViewModel autoSuggestVMOld)
		{
			if (autoSuggest != null)
			{
				autoSuggest.SetSuggestedText();

				DependencyPropertyDescriptor itemSelectedPreviewDescr = DependencyPropertyDescriptor.FromProperty(AutoSuggestViewModel.SelectedSuggestionProperty, typeof(AutoSuggestViewModel));
				if (itemSelectedPreviewDescr != null)
				{
					if (autoSuggestVMOld != null)
						itemSelectedPreviewDescr.RemoveValueChanged(autoSuggestVMOld, autoSuggest.SetSuggestedTextDelegate);

					if (autoSuggestVM != null)
						itemSelectedPreviewDescr.AddValueChanged(autoSuggestVM, autoSuggest.SetSuggestedTextDelegate);
				}
			}
		}

		public void CleanUp()
		{
			if(DataContextAutoSuggestVM != null)
			{
				DependencyPropertyDescriptor itemSelectedPreviewDescr = DependencyPropertyDescriptor.FromProperty(AutoSuggestViewModel.SelectedSuggestionProperty, typeof(AutoSuggestViewModel));
				if (itemSelectedPreviewDescr != null)
					itemSelectedPreviewDescr.RemoveValueChanged(DataContextAutoSuggestVM, SetSuggestedTextDelegate);

			}
		}

		private void SetSuggestedTextDelegate(object sender, EventArgs args)
		{
			SetSuggestedText();
		}

		private void SetSuggestedText()
		{
			TargetTextBox.TextChanged -= TargetTextBox_TextChanged;

			if (DataContextAutoSuggestVM != null && TargetTextBox != null)
			{
				if (DataContextAutoSuggestVM.SelectedSuggestion != null)
					TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion,true);
				else
					TargetTextBox.Text = "";
			}

			TargetTextBox.TextChanged += TargetTextBox_TextChanged;
		}

		private void OpenSuggestions()
		{
			if (isTargetTextBoxEditable && DataContextAutoSuggestVM != null)
			{
				if (DataContextAutoSuggestVM.FilterItems != null)
					DataContextAutoSuggestVM.FilterItems.Execute(TargetTextBox.Text);

				//IF there are suggestions or command open the popup
				if (currentSuggestionsSelector != null && (currentSuggestionsSelector.Items.Count > 0 
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
				&& (currentSuggestionsSelector != null && !currentSuggestionsSelector.IsKeyboardFocused))
			{
				if (currentSuggestionsSelector.SelectedIndex >= 0)
				{
					FrameworkElement item = currentSuggestionsSelector.ItemContainerGenerator.ContainerFromIndex(currentSuggestionsSelector.SelectedIndex) as FrameworkElement;
					if (item != null && (item.IsKeyboardFocused || item.IsKeyboardFocusWithin))
						return;
				}

				this.IsOpen = false;

				if (DataContextAutoSuggestVM == null)
					return;

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
									TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion,true);
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
							TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion,true);
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
			else if (keyDown == Key.Right || keyDown == Key.Left)
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

		private void SelectItemAndClose()
		{
			selectingItemOrClosingPopup = true;

			DataContextAutoSuggestVM.SelectedSuggestionPreview = currentSuggestionsSelector.SelectedItem;
			DataContextAutoSuggestVM.SelectedSuggestion = currentSuggestionsSelector.SelectedItem;

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
			selectingItemOrClosingPopup = false;
		}
		#endregion 
	}
}
