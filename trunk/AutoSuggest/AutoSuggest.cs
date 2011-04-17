using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
				null,
				(d, v) => { if(v != null && !(v is AutoSuggestViewModel)) throw new NotSupportedException(); return v; }
			));
		}
		public new AutoSuggestViewModel DataContext { get { return (AutoSuggestViewModel)base.DataContext; } set { base.DataContext = value;  } }
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
			DependencyProperty.Register("TaboutOnSelection", typeof(TaboutTrigger), typeof(AutoSuggest));

		public TaboutTrigger TaboutCommand { get { return (TaboutTrigger)GetValue(TaboutCommandProperty); } set { SetValue(TaboutCommandProperty, value); } }
		#endregion 

		#region Properties
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
		}
		#endregion 

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

				oldListView.PreviewKeyDown -= autoSuggest.newListView_PreviewKeyDown;
			}

			if (args.NewValue != null)
			{
				ListView newListView = (ListView)args.NewValue;
				autoSuggest.suggestionsControl.itemsSuggestionsListViewContainer.Child = newListView;

				newListView.PreviewKeyDown += autoSuggest.newListView_PreviewKeyDown;
			}
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
			if (CurrentSuggestionsListView != null && !CurrentSuggestionsListView.IsKeyboardFocused)
			{
				this.IsOpen = false;
			}
		}

		private void TargetTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (selectingItemOrClosingPopup) { selectingItemOrClosingPopup = false; return; }

			if (IsTargetTextBoxEditable)
			{
				if (!TargetTextBox.IsReadOnly)
				{
					if (DataContext.FilterItems != null && TargetTextBox != null)
						DataContext.FilterItems.Execute(TargetTextBox.Text);

					if (CurrentSuggestionsListView != null)
						this.IsOpen = true;
					else
						this.IsOpen = false;

					if (CurrentSuggestionsListView != null)
					{
						if (CurrentSuggestionsListView.Items.Count > 0)
						{
							CurrentSuggestionsListView.SelectedIndex = 0;
							DataContext.SelectedSuggestionPreview = CurrentSuggestionsListView.Items[0];
						}
						else
						{
							DataContext.SelectedSuggestionPreview = null;
						}
					}	
				}
			}
		}
		#endregion 

		#region Private Methods
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

		private void SelectItemAndClose()
		{
			DataContext.SelectedSuggestion = CurrentSuggestionsListView.SelectedItem;
			selectingItemOrClosingPopup = true;
			this.IsOpen = false;
		
			TargetTextBox.CaretIndex = TargetTextBox.Text.Length;
		}

		private void TabOutNext()
		{
			TargetTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			selectingItemOrClosingPopup = true;
			this.IsOpen = false;
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
