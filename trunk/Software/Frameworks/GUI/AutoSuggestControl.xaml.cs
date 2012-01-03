using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using KOControls.Core;
using KOControls.GUI.Core;
using System.Diagnostics;

namespace KOControls.GUI
{
	#region enums
	[Flags]
	public enum ApplyFilterTriggers
	{
		None = 0,
		FilterInputChanged = 1,
		Enter = 2
	}

	[Flags]
	public enum ConfirmTriggers
	{
		None = 0,
		Space = 1,
		Tab = 2,
		Arrows = 4,
		Enter = 8,

		SpaceTab = Space | Tab,
		SpaceArrows = Space | Arrows,
		SpaceEnter = Space | Enter,
		TabArrows = Tab | Arrows,
		TabEnter = Tab | Enter,
		ArrowsEnter = Arrows | Enter,
		SpaceTabArrows = SpaceTab | Arrows,
		SpaceTabEnter = SpaceTab | Enter,
		TabArrowsEnter = TabArrows | Enter,

		All = Space | Tab | Arrows | Enter,
	}

	[Flags]
	public enum TaboutTriggers
	{
		None = 0,
		Space = 1,
		Arrows = 2,
		Enter = 4,

		SpaceArrows = Space | Arrows,
		SpaceEnter = Space | Enter,
		ArrowsEnter = Arrows | Enter,

		All = Space | Arrows | Enter
	}
	#endregion

	public class AutoSuggestControl : Control
	{
		#region Construction
		protected static readonly ResourceDictionary ResourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/KOControls.GUI;component/AutoSuggestControl.xaml") };
		private static readonly ControlTemplate Default_Template;
		private static readonly ControlTemplate Default_CommandsTemplate;
		private static readonly ControlTemplate Default_SuggestionsTemplate;

		static AutoSuggestControl()
		{
			Default_Template = (ControlTemplate)ResourceDictionary["AutoSuggestControl_DefaultTemplate"];
			Default_CommandsTemplate = (ControlTemplate)ResourceDictionary["AutoSuggestControl_Default_CommandsTemplate"];
			Default_SuggestionsTemplate = (ControlTemplate)ResourceDictionary["AutoSuggestControl_Default_SuggestionsTemplate"];

			ViewModel.OverrideProperty<AutoSuggestControl>(FrameworkElement.DataContextProperty, null, Dependents_Changed, (d, v) => { Dependents_Changing(d, v);  return v as AutoSuggestViewModel; });
			ViewModel.OverrideProperty<AutoSuggestControl>(TemplateProperty, Default_Template, Dependents_Changed, (d, v) => { Dependents_Changing(d, v); return v; });

			DataContextProperty = ViewModel.IsInDesignMode ? ViewModel.RegisterProperty<AutoSuggestControl, AutoSuggestViewModel>("DataContext") : FrameworkElement.DataContextProperty;
		}

		public AutoSuggestControl()
		{
			Loaded += OnLoaded;
			//TBD: This line below is going to brake changes in CommandsTemplate in datagrid templated column!!!!
			CommandsTemplate = Default_CommandsTemplate;

			ApplyTemplate();
		}
		private void OnLoaded(object sender, RoutedEventArgs args)
		{
			Dependents_Changed(this, new DependencyPropertyChangedEventArgs());
		}
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Dependents_Changed(this, new DependencyPropertyChangedEventArgs());
		}
		private Control _commandsContentPresenter;
		private Control _suggestionsContentPresenter;
		private Label _searchCriteria;

		private Selector _selector;
		#endregion

		#region SuggestionsTemplate
		public static readonly DependencyProperty SuggestionsTemplateProperty = ViewModel.RegisterProperty<ControlTemplate, AutoSuggestControl>("SuggestionsTemplate", Default_SuggestionsTemplate, Dependents_Changed, Dependents_Changing);
		public ControlTemplate SuggestionsTemplate { get { return (ControlTemplate)GetValue(SuggestionsTemplateProperty); } set { SetValue(SuggestionsTemplateProperty, value); } }
		#endregion

		#region CommandsTemplate
		public static readonly DependencyProperty CommandsTemplateProperty = ViewModel.RegisterProperty<ControlTemplate, AutoSuggestControl>("CommandsTemplate", Default_CommandsTemplate);
		public ControlTemplate CommandsTemplate { get { return (ControlTemplate)GetValue(CommandsTemplateProperty); } set { SetValue(CommandsTemplateProperty, value); } }
		#endregion

		#region DataContext
		public new static readonly DependencyProperty DataContextProperty;
		public new AutoSuggestViewModel DataContext { get { return (AutoSuggestViewModel)base.DataContext; } set { base.DataContext = value; } }
		#endregion

		#region StyleModel
		private AutoSuggestControlStyleViewModel StyleModel { get { if (DataContext != null) return DataContext.StyleModel as AutoSuggestControlStyleViewModel; else return null; } }
		#endregion 

		#region TargetTextBox
		public static readonly DependencyProperty TargetTextBoxProperty = ViewModel.RegisterProperty<TextBox, AutoSuggestControl>("TargetTextBox", null, Dependents_Changed, Dependents_Changing);
		public TextBox TargetTextBox { get { return (TextBox)GetValue(TargetTextBoxProperty); } set { SetValue(TargetTextBoxProperty, value); } }
		#endregion

		#region OwnerPopup
		public static readonly DependencyProperty OwnerPopupProperty = ViewModel.RegisterProperty<Popup, AutoSuggestControl>("OwnerPopup");
		public Popup OwnerPopup { get { return (Popup)GetValue(OwnerPopupProperty); } set { SetValue(OwnerPopupProperty, value); } }

		private void OpenOwnerPopup()
		{
			if(OwnerPopup != null)
			{
				var open = (DataContext.Suggestions != null && DataContext.Suggestions.Count() > 0) ||
						DataContext.Commands.Count > 0;

				OwnerPopup.IsOpen = open;
			}
		}

		private void CloseOwnerPopup()
		{
			if(OwnerPopup != null)
				OwnerPopup.IsOpen = false;
		}
		#endregion

		protected bool _initialized;
		private static object Dependents_Changing2(DependencyObject d, object v)
		{
			return Dependents_Changing(d, v);
		}

		private static object Dependents_Changing(DependencyObject d, object v)
		{
			var asc = (AutoSuggestControl)d;
			if(asc._initialized)
			{
				asc._initialized = false;
				asc.ClearEvents();
			}

			return v;
		}
		
		protected static void Dependents_Changed(DependencyObject d, DependencyPropertyChangedEventArgs a)
		{
			var asc = (AutoSuggestControl)d;
			if(asc._initialized || !asc.IsLoaded || asc.Template == null || asc.DataContext == null || asc.SuggestionsTemplate == null || asc.TargetTextBox == null)
				return;

			asc._suggestionsContentPresenter = (Control)asc.Template.FindName("_suggestionsContentPresenter", asc);
			asc._suggestionsContentPresenter.Template = asc.SuggestionsTemplate;
			asc._suggestionsContentPresenter.ApplyTemplate();
			asc._selector = (Selector)asc._suggestionsContentPresenter.Template.FindName("PART_Selector", asc._suggestionsContentPresenter);
			asc._searchCriteria = asc.Template.FindName("PART_SearchCriteria", asc) as Label;
			 
			asc._commandsContentPresenter	 = (Control)asc.Template.FindName("_commandsContentPresenter", asc);
			asc._commandsContentPresenter.Template = asc.CommandsTemplate;
			asc._commandsContentPresenter.ApplyTemplate();

			asc._initialized = true;
			asc.WireUpEvents();
		}

		protected virtual void ClearEvents()
		{
			if(DataContext != null)
			{
				DataContext.Refreshed -= DataContext_Refreshed;
				DataContext.PropertyChanged -= DataContext_PropertyChanged;
				DataContext.FilterApplied -= DataContext_FilterApplied;
				if(StyleModel != null)
					StyleModel.RemoveValueChanged(AutoSuggestControlStyleViewModel.IsFilterTextDisplayedProperty, IsShowFilterTextInControlChanged);
			}
			if(_selector != null)
			{
				_selector.SelectionChanged -= Selector_SelectionChanged;
				_selector.RemoveHandler(MouseDoubleClickEvent, (RoutedEventHandler)Selector_MouseDoubleClick);

				_selector.PreviewKeyDown -= HandleKeyDown;
				_selector.PreviewKeyUp -= HandleKeyUp;
				_selector.LostKeyboardFocus -= HandleLostKeyboardFocus;
			}
			if(TargetTextBox != null)
			{
				TargetTextBox.PreviewTextInput -= TargetTextBox_TextInput;
				TargetTextBox.TextChanged -= TargetTextBox_TextChanged;
				TargetTextBox.PreviewKeyDown -= HandleKeyDown;
				TargetTextBox.PreviewKeyUp -= HandleKeyUp;
				TargetTextBox.GotKeyboardFocus -= HandleGotKeyboardFocus;
				TargetTextBox.LostKeyboardFocus -= HandleLostKeyboardFocus;
			}
		}

		private void IsShowFilterTextInControlChanged(object sender, EventArgs args)
		{
			if (_searchCriteria != null)
			{
				if (StyleModel.IsFilterTextDisplayed)
					_searchCriteria.Height = 25;
				else
					_searchCriteria.Height = 0;

				_searchCriteria.Content = TargetTextBoxText;
			}
		}

		protected virtual void WireUpEvents()
		{
			ClearEvents();

			DataContext.Refreshed += DataContext_Refreshed;
			DataContext.PropertyChanged += DataContext_PropertyChanged;
			DataContext.FilterApplied += DataContext_FilterApplied;
			StyleModel.AddValueChanged(AutoSuggestControlStyleViewModel.IsFilterTextDisplayedProperty,IsShowFilterTextInControlChanged);

			_selector.SelectionChanged += Selector_SelectionChanged;
			_selector.AddHandler(Control.MouseDoubleClickEvent, (RoutedEventHandler)Selector_MouseDoubleClick, true);
			_selector.PreviewKeyDown += HandleKeyDown;
			_selector.PreviewKeyUp += HandleKeyUp;
			_selector.LostKeyboardFocus += HandleLostKeyboardFocus;

			TargetTextBox.PreviewTextInput += TargetTextBox_TextInput;
			TargetTextBox.TextChanged += TargetTextBox_TextChanged;
			TargetTextBox.PreviewKeyDown += HandleKeyDown;
			TargetTextBox.PreviewKeyUp += HandleKeyUp;
			TargetTextBox.GotKeyboardFocus += HandleGotKeyboardFocus;
			TargetTextBox.LostKeyboardFocus += HandleLostKeyboardFocus;

			SetTargetTextBoxTextToSuggestionString();
		}

		private void ApplyFilterAndShowSuggestions()
		{
			if(_changingTexBoxText) return;

			DataContext.ApplyFilter(TargetTextBoxTextWithoutTrailingSelection);
			OpenOwnerPopup();
		}

		private void DataContext_FilterApplied()
		{
			_suggestionPreviewChanging = true;//Prevent SuggestionPreview being fired from SelectedItem_Changed in _selector. We want to explictly call it in this method.
			try
			{
				if(_deletingText)
				{
					_selector.SelectedIndex = -1;
				}
				else
				{
					if(_selector.ItemsSource.Count() > 0)
					{
						if(!StyleModel.IsAutoCompleteOn 
							|| (String.IsNullOrEmpty(TargetTextBoxText) && DataContext.IsEmptyValueAllowed))
							_selector.SelectedIndex = -1;
						else
							_selector.SelectedIndex = 0;
					}
					else
					{
						_selector.SelectedIndex = -1;
					}
				}

				_suggestionPreviewChanging = false;				
				//if (StyleModel.IsAutoCompleteOn)
					SetSuggestionPreview();
			}
			finally { _suggestionPreviewChanging = false; _deletingText = false; }
		}

		protected virtual void DataContext_Refreshed()
		{
			SetTargetTextBoxTextToSuggestionString();

			ApplyFilterAndShowSuggestions();
		}
		private void DataContext_PropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			if(_dataContextPropertyChanging) return;

			_dataContextPropertyChanging = true;
			try
			{
				if(args.Property == AutoSuggestViewModel.SuggestionsProperty)
				{
					_selector.ItemsSource = DataContext.Suggestions;
				}
				else if(args.Property == AutoSuggestViewModel.SuggestionPreviewProperty)
				{
					if(!_suggestionPreviewChanging)
					{
						_selector.SelectedItem = DataContext.SuggestionPreview;
						if (!_deletingText)
							SetTargetTextBoxTextToSuggestionPreviewString();
					}
				}
				else if(args.Property == AutoSuggestViewModel.SuggestionProperty && DataContext.IsConfirmed)
				{
					_selector.SelectedItem = DataContext.Suggestion;
					SetTargetTextBoxTextToSuggestionString();
				}
			}
			finally { _dataContextPropertyChanging = false; }
		}

		private bool _changingTexBoxText = false;

		private void SetTargetTextBoxTextToSuggestionPreviewString()
		{
			if(_changingTexBoxText) return;

			_changingTexBoxText = true;
			try
			{
				var fullText = DataContext.SuggestionPreviewToString(DataContext.SuggestionPreview) ?? "";
				var text = TargetTextBoxTextWithoutTrailingSelection;
				if (StyleModel.IsAutoCompleteOn)
				{
					if(fullText.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
					{
						TargetTextBox.Text = fullText;
						TargetTextBox.CaretIndex = text.Length;
						TargetTextBox.Select(text.Length, fullText.Length);
					}
				}
				else
				{
					if (DataContext.SuggestionPreview != null)
					{
						TargetTextBox.Text = fullText;
						TargetTextBox.CaretIndex = fullText.Length;
					}
				}
			}
			finally { _changingTexBoxText = false; }
		}

		protected virtual void SetTargetTextBoxTextToSuggestionString()
		{
			TargetTextBox.Text = DataContext.SuggestionToString(DataContext.Suggestion);
		}

		protected bool _dataContextPropertyChanging;

		private bool _userInput;

		private bool _suggestionPreviewChanging = false;

		private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetSuggestionPreview();
		}

		private bool IsKeyBoardFocusWithinAutoSuggestControls
		{
			get
			{
				return TargetTextBox.IsKeyboardFocusWithin || IsKeyboardFocusWithin ||
						((Keyboard.FocusedElement is DependencyObject) && ((DependencyObject)Keyboard.FocusedElement).VisualChildOf(_selector));
			}
		}

		private void SetSuggestionPreview()
		{
			if(_suggestionPreviewChanging) return;

			_suggestionPreviewChanging = true;
			try
			{
				if(_selector.SelectedIndex == -1)
				{
					if(String.IsNullOrEmpty(TargetTextBoxText) && DataContext.IsEmptyValueAllowed)
					{
						DataContext.SuggestionPreview = DataContext.EmptyValue;
					}
					else
					{
						DataContext.SuggestionPreview = null;
					}
				}
				else
				{
					DataContext.SuggestionPreview = _selector.SelectedItem;
				}
				if (!_deletingText)
					SetTargetTextBoxTextToSuggestionPreviewString();
				
				if (StyleModel.IsAutoCompleteOn)
				{
					Dispatcher.BeginInvoke((Action)delegate
					{
						if (IsKeyBoardFocusWithinAutoSuggestControls)
							TargetTextBox.Focus();
					});
				}
			}
			finally { _suggestionPreviewChanging = false; }
		}

		private void Selector_MouseDoubleClick(object sender, RoutedEventArgs args)
		{
			HandleConfirm();
			args.Handled = true;
		}

		private bool _deletingText = false;
		private void TargetTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_userInput || _dataContextPropertyChanging) return;

			try
			{
				_deletingText = e.Changes.First().AddedLength == 0 && e.Changes.First().RemovedLength > 0;
				ApplyFilterAndShowSuggestions();

				if(StyleModel.IsFilterTextDisplayed && _searchCriteria != null)
					_searchCriteria.Content = TargetTextBoxText;
			}
			finally { _userInput = false; }
		}

		private void TargetTextBox_TextInput(object sender, TextCompositionEventArgs e)
		{
			_userInput = true;
		}

		protected virtual void HandleKeyDown(object sender, KeyEventArgs e)
		{
			if(sender == TargetTextBox)
			{
				switch(e.Key)
				{
					case Key.Down:
						{
							if(_selector.Items.Count > 0 && _selector.Items.Count > _selector.SelectedIndex)
								_selector.SelectedIndex += 1;
							e.Handled = true;
							return;
						}
					case Key.Up:
						{
							if(_selector.Items.Count > 0 && _selector.SelectedIndex > 0)
								_selector.SelectedIndex -= 1;
							e.Handled = true;
							return;
						}
					default: _userInput = true; break;
				}
			}

			switch(e.Key)
			{
				case Key.Enter:
					{
						//if((InvokeSelectTrigger & InvokeSelectTriggers.Enter) == InvokeSelectTriggers.Enter)
						HandleConfirm();
						if((StyleModel.TaboutTrigger & TaboutTriggers.Enter) == TaboutTriggers.Enter)
							TabOutNext();
						e.Handled = true;
						break;
					}
				case Key.Left:
					{
						if(TargetTextBox.CaretIndex == 0)
						{
							if((StyleModel.ConfirmTrigger & ConfirmTriggers.Arrows) == ConfirmTriggers.Arrows)
							{
								HandleConfirm();
								e.Handled = true;
							}
							if((StyleModel.TaboutTrigger & TaboutTriggers.Arrows) == TaboutTriggers.Arrows)
							{
								TabOutPrevious();
								e.Handled = true;
							}
						}
						break;
					}
				case Key.Right:
					{
						if(TargetTextBox.CaretIndex == TargetTextBoxTextWithoutTrailingSelection.Length)
						{
							if((StyleModel.ConfirmTrigger & ConfirmTriggers.Arrows) == ConfirmTriggers.Arrows)
							{
								HandleConfirm();
								e.Handled = true;
							}
							if((StyleModel.TaboutTrigger & TaboutTriggers.Arrows) == TaboutTriggers.Arrows)
							{
								TabOutNext();
								e.Handled = true;
							}
						}
						break;
					}
				case Key.Space:
					{
						if(DataContext.Suggestions.Count() == 1 && !DataContext.IsFreeTextAllowed && DataContext.SuggestionPreview != null)
						{
							if((StyleModel.ConfirmTrigger & ConfirmTriggers.Space) == ConfirmTriggers.Space)
							{
								HandleConfirm();
								e.Handled = true;
							}
							if((StyleModel.TaboutTrigger & TaboutTriggers.Space) == TaboutTriggers.Space)
							{
								TabOutNext();
								e.Handled = true;
							}
						}
						break;
					}
				case Key.Tab:
					{
						if((StyleModel.ConfirmTrigger & ConfirmTriggers.Tab) == ConfirmTriggers.Tab)
						{
							HandleConfirm();
							e.Handled = true;
						}
						TargetTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
						e.Handled = true;
						break;
					}
				case Key.Escape:
					{
						HandleCancel();
						e.Handled = true;
						break;
					}
			}
		}

		private void TabOutNext()
		{
			TargetTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		private void TabOutPrevious()
		{
			TargetTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
		}
		private void HandleKeyUp(object sender, KeyEventArgs e)
		{
			_userInput = false;
		}
		protected virtual void HandleGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if(_dataContextPropertyChanging || (OwnerPopup != null && OwnerPopup.IsOpen)) return;

			this.WireUpEvents();
			TargetTextBox.Select(TargetTextBoxText.Length, 0);
			ApplyFilterAndShowSuggestions();
			Dispatcher.BeginInvoke((Action)SelectAll);
		}

		private void HandleLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if(IsKeyBoardFocusWithinAutoSuggestControls)
				return;

			HandleConfirm();
		}

		private void HandleConfirm()
		{
			if (_confirming) return;
			_confirming = true;

			try
			{
				if (!DataContext.IsConfirmed) DataContext.Confirm(TargetTextBoxText);

				SetTargetTextBoxTextToSuggestionString();//If it is confirmed make sure that the displayed text matches the suggestion.
				CloseOwnerPopup();
			}
			finally { _confirming = false; }
		}
		private bool _confirming;

		private void HandleCancel()
		{
			DataContext.Cancel();
			SetTargetTextBoxTextToSuggestionString();//If it is confirmed make sure that the displayed text matches the suggestion.
			CloseOwnerPopup();
		}

		protected void SelectAll()
		{
			TargetTextBox.CaretIndex = TargetTextBoxTextWithoutTrailingSelection.Length;
			TargetTextBox.SelectAll();
		}

		protected string TargetTextBoxText
		{
			get
			{
				if(string.IsNullOrEmpty(TargetTextBox.Text)) return string.Empty;

				return TargetTextBox.Text;
			}
		}

		private string TargetTextBoxTextWithoutTrailingSelection
		{
			get
			{
				if(string.IsNullOrEmpty(TargetTextBox.Text)) return string.Empty;

				if(TargetTextBox.SelectionLength == 0) return TargetTextBox.Text;

				return TargetTextBox.Text.Substring(0, TargetTextBox.SelectionStart);
			}
		}
	}
}
