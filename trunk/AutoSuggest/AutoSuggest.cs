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
			DependencyProperty.Register("(SelectionCommand", typeof(SelectionTrigger), typeof(AutoSuggest));

		public SelectionTrigger SelectionCommand { get { return (SelectionTrigger)GetValue(SelectionCommandProperty); } set { SetValue(SelectionCommandProperty, value); } }
		#endregion 

		#region Tabout On Selection
		public static DependencyProperty TaboutCommandProperty =
			DependencyProperty.Register("TaboutOnSelection", typeof(TaboutTrigger), typeof(AutoSuggest),new PropertyMetadata(TaboutTrigger.Enter));
		public TaboutTrigger TaboutCommand { get { return (TaboutTrigger)GetValue(TaboutCommandProperty); } set { SetValue(TaboutCommandProperty, value); } }
		#endregion 

		#region Properties
        private Window Window = null;
        private bool cancelWindowKeyDown = false;

		private int selectionLength = 0;

		private ListView CurrentSuggestionsListView
		{
			get
			{
				if(this.suggestionsControl.itemsSuggestionsListViewContainer.Child != null && this.suggestionsControl.itemsSuggestionsListViewContainer.Child is ListView)
					return this.suggestionsControl.itemsSuggestionsListViewContainer.Child as ListView;
				return null;
			}
		}

		private bool IsTargetTextBoxEditable { get { return TargetTextBox != null && !TargetTextBox.IsReadOnly; } }
		private SuggestionsControl suggestionsControl = null;

		private bool selectingItemOrClosingPopup = false;
		#endregion 

		#region Constructors
		public AutoSuggest()
		{
			suggestionsControl = new SuggestionsControl();
			this.Child = suggestionsControl;
            this.PreviewMouseDown += AutoSuggest_PreviewMouseDown;
            
            AllowsTransparency = true;
            PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
		}

        void AutoSuggest_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
           // e.Handled = true;
        }
		#endregion 

		#region Event Handlers
     
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            Window = Window.GetWindow(this);
            Window.PreviewMouseDown += (x, y) => {  };
            Window.PreviewKeyDown += Window_PreviewKeyDown;
        }

        void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Window.Activate();
           
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
				{
					string n = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion);
					TargetTextBox.Text = n;
				}
				else
				{
					TargetTextBox.Text = "";
				}
			}
			TargetTextBox.TextChanged += TargetTextBox_TextChanged;
		}

		private static void TargetTextBox_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			AutoSuggest autoSuggest = (AutoSuggest)sender;
			if (args.OldValue != null)
			{
				TextBox oldTextBox = (TextBox)args.OldValue;
				oldTextBox.PreviewKeyDown -= autoSuggest.TargetTextBox_PreviewKeyDown;
				oldTextBox.TextChanged -=autoSuggest.TargetTextBox_TextChanged;
				oldTextBox.LostKeyboardFocus -= autoSuggest.TargetTextBox_LostKeyboardFocus;
			}

			if (args.NewValue != null)
			{
				TextBox newTextBox = (TextBox)args.NewValue;
				autoSuggest.PlacementTarget = newTextBox;
				newTextBox.PreviewKeyDown += autoSuggest.TargetTextBox_PreviewKeyDown;
				newTextBox.TextChanged += autoSuggest.TargetTextBox_TextChanged;
				newTextBox.LostKeyboardFocus += autoSuggest.TargetTextBox_LostKeyboardFocus;

                InitializeText(autoSuggest, autoSuggest.DataContextAutoSuggestVM);
			}
		}

		private void TargetTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (selectingItemOrClosingPopup) { return; }
			
			if (IsTargetTextBoxEditable)
			{
				if (!TargetTextBox.IsReadOnly)
				{
					if (DataContextAutoSuggestVM.FilterItems != null && TargetTextBox != null)
						DataContextAutoSuggestVM.FilterItems.Execute(TargetTextBox.Text);

					if (CurrentSuggestionsListView != null && CurrentSuggestionsListView.Items.Count > 0)
						this.IsOpen = true;
					else
						this.IsOpen = false;

					if (CurrentSuggestionsListView != null)
					{
						if (CurrentSuggestionsListView.Items.Count > 0)
						{
							CurrentSuggestionsListView.SelectedIndex = 0;
							DataContextAutoSuggestVM.SelectedSuggestionPreview = CurrentSuggestionsListView.Items[0];

							selectingItemOrClosingPopup = true;
							if (e.Changes != null && e.Changes.Count > 0)
							{
								TextChange txtChange = e.Changes.First<TextChange>();

								int indx = TargetTextBox.CaretIndex;
								if (txtChange.AddedLength > 0 || txtChange.Offset > 0)
								{
									string n = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestionPreview);
									TargetTextBox.Text = n;
								}
								//If no added text and text changed this must be the backspace key
								if (txtChange.AddedLength == 0 && selectionLength > 0 && txtChange.RemovedLength == selectionLength)
									indx--;

								if (indx == 0)
									TargetTextBox.Text = "";

								TargetTextBox.CaretIndex = indx;
								if (TargetTextBox.Text.Length > indx)
								{
									selectionLength = TargetTextBox.Text.Length - indx;
									TargetTextBox.Select(indx, selectionLength);
								}
								else
								{
									selectionLength = 0;
								}
							}
							selectingItemOrClosingPopup = false;
						}
						else
						{
							DataContextAutoSuggestVM.SelectedSuggestionPreview = null;
						}
					}
				}
				e.Handled = true;
			}	
		}

		private static void SuggestionsListView_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			AutoSuggest autoSuggest = (AutoSuggest)sender;
			if (args.OldValue != null)
			{
				ListView oldListView = (ListView)args.NewValue;

				oldListView.PreviewKeyDown -= autoSuggest.newListView_PreviewKeyDown;
				oldListView.LostKeyboardFocus -= autoSuggest.newListView_LostKeyboardFocus;
			}

			if (args.NewValue != null)
			{
				ListView newListView = (ListView)args.NewValue;
				autoSuggest.suggestionsControl.itemsSuggestionsListViewContainer.Child = newListView;

				newListView.PreviewKeyDown += autoSuggest.newListView_PreviewKeyDown;
				newListView.LostKeyboardFocus += autoSuggest.newListView_LostKeyboardFocus;
			}
		}

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
             if (CurrentSuggestionsListView != null && CurrentSuggestionsListView.SelectedIndex > 0)
                SelectItemAndClose();
        }

		private void newListView_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			HandleLostFocus();
		}

		private void newListView_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			ListView lv = (ListView)sender;
			if (lv.SelectedItem != null)
				e.Handled =	SelectItemAndTaboutByKeyDownValue(e.Key);
		}

		private void TargetTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Down && this.IsOpen)
			{
				if (CurrentSuggestionsListView != null)
					CurrentSuggestionsListView.Focus();
			}
			if (e.Key == Key.Space && (SelectionCommand & SelectionTrigger.Space) == SelectionTrigger.Space
				&& (CurrentSuggestionsListView == null || CurrentSuggestionsListView.Items.Count != 1))
				return;

			e.Handled = SelectItemAndTaboutByKeyDownValue(e.Key);
		}

		private void TargetTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			HandleLostFocus();	
		}
		#endregion 

		#region Private Methods
		private void HandleLostFocus()
		{
			if ((!this.IsFocused && !this.IsKeyboardFocused)
				&& (TargetTextBox != null && !TargetTextBox.IsKeyboardFocused)
				&& (CurrentSuggestionsListView != null && !CurrentSuggestionsListView.IsKeyboardFocused))
			{
				if (CurrentSuggestionsListView.SelectedIndex >= 0)
				{
					var item = CurrentSuggestionsListView.ItemContainerGenerator.ContainerFromIndex(CurrentSuggestionsListView.SelectedIndex)
						 as ListViewItem;
					if (item != null && (item.IsFocused || item.IsKeyboardFocused))
						return;
				}

				this.IsOpen = false;

				if (String.IsNullOrEmpty(TargetTextBox.Text))
				{
					DataContextAutoSuggestVM.SelectedSuggestion = null;
				}
				else
				{
					if (DataContextAutoSuggestVM.IsAllowInvalidText)
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
								{
									TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion);
								}
								else
								{
									TargetTextBox.Text = "";
								}
								selectingItemOrClosingPopup = false;
							}
						}
					}
					else
					{
						selectingItemOrClosingPopup = true;
						if (DataContextAutoSuggestVM.SelectedSuggestion == null)
							TargetTextBox.Text = "";
						else
							TargetTextBox.Text = DataContextAutoSuggestVM.GetSelectedSuggestionFormattedName(DataContextAutoSuggestVM.SelectedSuggestion);
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
				selectingItemOrClosingPopup = true;
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

        private void FocusSelectedListViewElement()
        {
            if (CurrentSuggestionsListView.SelectedIndex >= 0)
            {
                var item = CurrentSuggestionsListView.ItemContainerGenerator.ContainerFromIndex(CurrentSuggestionsListView.SelectedIndex)
                     as ListViewItem;
                if (item != null)
                    item.Focus();
            }
        }

		private void SelectItemAndClose()
		{
			selectingItemOrClosingPopup = true;
			DataContextAutoSuggestVM.SelectedSuggestion = CurrentSuggestionsListView.SelectedItem;
			this.IsOpen = false;
			//TargetTextBox.Text = DataContextAutoSuggestVM.TextBoxText;
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
