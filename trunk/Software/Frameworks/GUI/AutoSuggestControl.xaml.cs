using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using KOControls.Core;
using KOControls.GUI.Core;

namespace KOControls.GUI
{
	using __type=AutoSuggestControl;

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
		protected static readonly ResourceDictionary ResourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/KOControls.GUI;component/AutoSuggestControl.xaml") };
		private static readonly Style Default_Style;
		private static readonly ControlTemplate Default_Template;
		private static readonly ControlTemplate Default_CommandsTemplate;
		private static readonly ControlTemplate Default_SuggestionsTemplate;

		static AutoSuggestControl()
		{
			Default_Style = (Style)ResourceDictionary["AutoSuggestControl_Default_Style"];
			Default_Template = (ControlTemplate)ResourceDictionary["AutoSuggestControl_Default_Template"];
			Default_CommandsTemplate = (ControlTemplate)ResourceDictionary["AutoSuggestControl_Default_CommandsTemplate"];
			Default_SuggestionsTemplate = (ControlTemplate)ResourceDictionary["AutoSuggestControl_Default_SuggestionsTemplate"];

			ViewModel.OverrideProperty<AutoSuggestControl>(DataContextProperty, null, Components_Changed, (d, v) => { Components_Changing(d, DataContextProperty, v); return v as AutoSuggestViewModel; });
			ViewModel.OverrideProperty<AutoSuggestControl>(TemplateProperty, Default_Template, Components_Changed, (d, v) => { Components_Changing(d, TemplateProperty, v); return v; });
		}

		public AutoSuggestControl()
		{
			SetCurrentValue(StyleProperty, Default_Style);
			SetCurrentValue(SuggestionsTemplateProperty, Default_SuggestionsTemplate);

			Loaded += OnLoaded;
			//TBD: This line below is going to brake changes in CommandsTemplate in datagrid templated column!!!!
			CommandsTemplate = Default_CommandsTemplate;

			ApplyTemplate();
		}
		protected bool ComponentsInitialized;
		private Label _searchCriteria;
		private Selector _selector;
		private void OnLoaded(object sender, RoutedEventArgs args)
		{
			if(!ComponentsInitialized) Components_Changed(this, new DependencyPropertyChangedEventArgs());
		}
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_searchCriteria = (Label)Template.FindName("PART_SearchCriteria", this);

			_suggestionsContentPresenter = (Control)Template.FindName("_suggestionsContentPresenter", this);
			_suggestionsContentPresenter.ApplyTemplate();
			_suggestionsContentPresenter.AddValueChanged(TemplateProperty, _suggestionsContentPresenter_TemplateChanged);

			_commandsContentPresenter = (Control)Template.FindName("_commandsContentPresenter", this);

			InitializeSelector();
		}
		private void _suggestionsContentPresenter_TemplateChanged(object sender, EventArgs e)
		{
			_suggestionsContentPresenter.ApplyTemplate();

			InitializeSelector();
		}
		private Control _suggestionsContentPresenter;
		private Control _commandsContentPresenter;

		private void InitializeSelector()
		{
			var selector = (Selector)_suggestionsContentPresenter.Template.FindName("PART_Selector", _suggestionsContentPresenter);
			if(_selector == selector) return;

			Components_Changing(this, null, new DependencyPropertyChangedEventArgs());
			_selector = selector;
			Components_Changed(this, new DependencyPropertyChangedEventArgs());
		}

		#region VM
		public AutoSuggestViewModel VM { get { return (AutoSuggestViewModel)DataContext; } set { DataContext = value; } }
		#endregion

		#region StyleModel
		private AutoSuggestControlStyleViewModel StyleModel { get { return VM.StyleModel as AutoSuggestControlStyleViewModel; } }
		#endregion

		#region SuggestionsTemplate
		private static readonly DependencyProperty SuggestionsTemplateProperty = ViewModel.RegisterProperty<ControlTemplate, AutoSuggestControl>("SuggestionsTemplate", Default_SuggestionsTemplate, Components_Changed, (d, v) => Components_Changing(d, SuggestionsTemplateProperty, v));
		public ControlTemplate SuggestionsTemplate { get { return (ControlTemplate)GetValue(SuggestionsTemplateProperty); } set { SetValue(SuggestionsTemplateProperty, value); } }
		#endregion

		#region CommandsTemplate
		public static readonly DependencyProperty CommandsTemplateProperty = ViewModel.RegisterProperty<ControlTemplate, AutoSuggestControl>("CommandsTemplate", Default_CommandsTemplate);
		public ControlTemplate CommandsTemplate { get { return (ControlTemplate)GetValue(CommandsTemplateProperty); } set { SetValue(CommandsTemplateProperty, value); } }
		#endregion

		#region TextTokensControlTarget
		public static readonly DependencyProperty TextTokensControlTargetProperty = ViewModel.RegisterProperty<FrameworkElement, __type>("TextTokensControlTarget", null, Components_Changed, (d, v) => { if(!(v is TextBox)) throw new NotSupportedException(); return Components_Changing(d, TextTokensControlTargetProperty, v); });
		public FrameworkElement TextTokensControlTarget { get { return (FrameworkElement)GetValue(TextTokensControlTargetProperty); } set { SetValue(TextTokensControlTargetProperty, value); } }
		#endregion

		#region TargetTextBox - Obsolete
		[Obsolete] public static readonly DependencyProperty TargetTextBoxProperty = ViewModel.RegisterProperty<TextBoxBase, AutoSuggestControl>("TargetTextBox", null, (d, a) => d.SetCurrentValue(TextTokensControlTargetProperty, a.NewValue));
		[Obsolete] public TextBoxBase TargetTextBox { get { return (TextBoxBase)GetValue(TargetTextBoxProperty); } set { SetValue(TargetTextBoxProperty, value); } }
		#endregion

		#region TextTokensControl
		protected ITextTokensControl TextTokensControl { get; private set; }
		#endregion

		#region OwnerPopup
		public static readonly DependencyProperty OwnerPopupProperty = ViewModel.RegisterProperty<Popup, AutoSuggestControl>("OwnerPopup");
		public Popup OwnerPopup { get { return (Popup)GetValue(OwnerPopupProperty); } set { SetValue(OwnerPopupProperty, value); } }

		private void OpenOwnerPopup()
		{
			if(OwnerPopup == null) return;

			OwnerPopup.IsOpen = (VM.SuggestionPreviews != null && VM.SuggestionPreviews.Count() > 0) || VM.Commands.Count > 0;
		}

		private void CloseOwnerPopup()
		{
			if(OwnerPopup == null) return;

			OwnerPopup.IsOpen = false;
		}
		#endregion

		private static object Components_Changing(DependencyObject d, DependencyProperty property, object v)
		{
			var asc = (AutoSuggestControl)d;
			if(asc.ComponentsInitialized)
			{
				asc.ComponentsInitialized = false;
				asc.ClearEvents();

				if(property == DataContextProperty || property == TextTokensControlTargetProperty)
					asc.TextTokensControl = null;
			}

			return v;
		}
		protected static void Components_Changed(DependencyObject d, DependencyPropertyChangedEventArgs a)
		{
			var asc = (AutoSuggestControl)d;
			if(a.Property == TextTokensControlTargetProperty)
			{
				asc.ClearTextBoxEvents();
				asc.TextTokensControl = null;
			}

			if(asc.ComponentsInitialized || asc.Template == null || asc.VM == null || asc.SuggestionsTemplate == null || asc.TextTokensControlTarget == null || asc._selector == null)
				return;

			if(asc.TextTokensControl == null)
			{
				if(asc.TextTokensControlTarget is TextBox)
					asc.TextTokensControl = new TextBoxTextTokensControl((TextBox)asc.TextTokensControlTarget);
				else
					throw new NotImplementedException();
			}

			asc._commandsContentPresenter.Template = asc.CommandsTemplate;
			asc._commandsContentPresenter.ApplyTemplate();

			asc.ComponentsInitialized = true;
			asc.WireUpEvents();
		}

		private void ClearTextBoxEvents()
		{
			if(TextTokensControl == null) return;

			TextTokensControl.CurrentTokenChanged -= TextTokensControl_CurrentTokenChanged;
			TextTokensControl.PreviewKeyDown -= HandleKeyDown;
			TextTokensControl.GotKeyboardFocus -= HandleGotKeyboardFocus;
			TextTokensControl.LostKeyboardFocus -= HandleLostKeyboardFocus;
			TextTokensControl.PreviewMouseDown -= TextTokensControl_PreviewMouseDown;
		}
		protected virtual void ClearEvents()
		{
			if(VM != null)
			{
				VM.Refreshed -= VM_Refreshed;
				VM.PropertyChanged -= VM_PropertyChanged;
				VM.FilterApplied -= VM_FilterApplied;
				if(StyleModel != null)
					StyleModel.RemoveValueChanged(AutoSuggestControlStyleViewModel.IsFilterTextDisplayedProperty, IsShowFilterTextInControlChanged);
			}
			if(_selector != null)
			{
				_selector.SelectionChanged -= Selector_SelectionChanged;
				_selector.RemoveHandler(MouseDoubleClickEvent, (RoutedEventHandler)Selector_MouseDoubleClick);

				_selector.PreviewKeyDown -= HandleKeyDown;
				_selector.LostKeyboardFocus -= HandleLostKeyboardFocus;
			}
			ClearTextBoxEvents();
		}

		protected virtual void WireUpEvents()
		{
			ClearEvents();

			VM.Refreshed += VM_Refreshed;
			VM.PropertyChanged += VM_PropertyChanged;
			VM.FilterApplied += VM_FilterApplied;
			if(StyleModel != null)
				StyleModel.AddValueChanged(AutoSuggestControlStyleViewModel.IsFilterTextDisplayedProperty, IsShowFilterTextInControlChanged);

			_selector.SelectionChanged += Selector_SelectionChanged;
			_selector.AddHandler(MouseDoubleClickEvent, (RoutedEventHandler)Selector_MouseDoubleClick, true);
			_selector.PreviewKeyDown += HandleKeyDown;
			_selector.LostKeyboardFocus += HandleLostKeyboardFocus;

			TextTokensControl.CurrentTokenChanged += TextTokensControl_CurrentTokenChanged;
			TextTokensControl.PreviewKeyDown += HandleKeyDown;
			TextTokensControl.GotKeyboardFocus += HandleGotKeyboardFocus;
			TextTokensControl.LostKeyboardFocus += HandleLostKeyboardFocus;
			TextTokensControl.PreviewMouseDown += TextTokensControl_PreviewMouseDown;

			SetSuggestionDisplayText();
			if (!TextTokensControl.Focused)
				CloseOwnerPopup();
		}

		private void TextTokensControl_CurrentTokenChanged(CurrentTokenChangedArgs e)
		{
			if(_VMPropertyChanging || _suggestionPreviewChanging) return;

			_deletingText = e.ChangeType == CurrentTokenChangedArgs.ChangeTypes.Remove;//Do not bitwise test. We only want to catch deleting text not replacing text.

			if (_deletingText)
			{
				CloseOwnerPopup();
				_suggestionPreviewChanging = true;//Prevent SuggestionPreview being fired and fire it explicitly at the end to make sure it actually gets fired
				try
				{
					_selector.SelectedIndex = -1;
					_suggestionPreviewChanging = false;
					SetSuggestionPreview();
				}
				finally { _suggestionPreviewChanging = false; }
			}
			else
			{
				ApplyFilterAndShowSuggestions();
			}
		}
		private bool _deletingText;

		private void ApplyFilterAndShowSuggestions(bool showAllSuggestions = false)
		{
			if(_changingTexBoxText) return;

			VM.ApplyFilter(showAllSuggestions ? "" : TextTokensControl.GetFilterText());

			if(StyleModel != null && StyleModel.IsFilterTextDisplayed && _searchCriteria != null)
				_searchCriteria.Content = CurrentTokenText;

			OpenOwnerPopup();
		}
		private bool _changingTexBoxText;

		private void SetTargetTextBoxTextToSuggestionPreviewString()
		{
			if(_changingTexBoxText) return;

			_changingTexBoxText = true;
			try
			{
				if(VM.SuggestionPreview == null) return;

				var fullText = VM.SuggestionPreviewToString(VM.SuggestionPreview) ?? "";
				var text = TextTokensControl.GetFilterText();
				if(StyleModel != null && StyleModel.IsAutoCompleteOn)
				{
					if(fullText.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
					{
						TextTokensControl.InsertAutoCompleteText(fullText.Substring(text.Length));
						//_textTokensControl.TargetTokenCaretIndex = text.Length;
						//_textTokensControl.TargetTokenSelect(_textTokensControl.TargetTokenCaretIndex, fullText.Length - text.Length);
					}
					//else
					//{
					//_textTokensControl.TargetTokenCaretIndex = 0;
					//_textTokensControl.TargetTokenSelect(0, fullText.Length);
					//}
				}
				//else
				//{
				//    _textTokensControl.TargetTokenText = fullText;
				//    _textTokensControl.TargetTokenCaretIndex = fullText.Length;
				//}
			}
			finally { _changingTexBoxText = false; }
		}
		private void SetSuggestionDisplayText()
		{
			TextTokensControl.ReplaceTargetTokenText(VM.SuggestionToString(VM.Suggestion));
		}

		private void VM_FilterApplied(string filterInput)
		{
			_suggestionPreviewChanging = true;//Prevent SuggestionPreview being fired and fire it explicitly at the end to make sure it actually gets fired
			try
			{
				if(_deletingText)
				{
					_selector.SelectedIndex = -1;
				}
				else if(_selector.ItemsSource.Count() > 0)
				{
					Trace.WriteLine("CurrentTokenText: " + CurrentTokenText + " filterInput: " + filterInput);
					if((String.IsNullOrEmpty(CurrentTokenText) && VM.IsEmptyValueAllowed) || (StyleModel != null && !StyleModel.IsAutoCompleteOn))
						_selector.SelectedIndex = -1;
					else if(filterInput != CurrentTokenText && VM.Suggestion != null)
						_selector.SelectedItem = VM.Suggestion;
					else 
						_selector.SelectedIndex = 0;
				}

				_suggestionPreviewChanging = false;
				SetSuggestionPreview();
			}
			finally { _suggestionPreviewChanging = false; _deletingText = false; }
		}
		private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SetSuggestionPreview();
		}


		private void SetSuggestionPreview()
		{
			if(_suggestionPreviewChanging || _VMPropertyChanging) return;

			_suggestionPreviewChanging = true;
			try
			{
				if(_selector.SelectedIndex == -1)
				{
					if(String.IsNullOrEmpty(CurrentTokenText) && VM.IsEmptyValueAllowed)
						VM.SuggestionPreview = VM.EmptyValue;
					else
						VM.SuggestionPreview = null;
				}
				else
				{
					VM.SuggestionPreview = _selector.SelectedItem;
				}
				if(!_deletingText)
					SetTargetTextBoxTextToSuggestionPreviewString();

				if(StyleModel != null && StyleModel.IsAutoCompleteOn)
				{
					Dispatcher.BeginInvoke((Action)delegate
					{
						if(IsKeyBoardFocusWithinAutoSuggestControls)
							TextTokensControl.Focused = true;
					});
				}
			}
			finally { _suggestionPreviewChanging = false; }
		}
		private bool _suggestionPreviewChanging;

		private void VM_Refreshed()
		{
			SetSuggestionDisplayText();
			ApplyFilterAndShowSuggestions();
		}

		private void VM_PropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			if(_VMPropertyChanging) return;

			_VMPropertyChanging = true;
			try
			{
				if(args.Property == AutoSuggestViewModel.SuggestionPreviewsProperty)
				{
					_selector.ItemsSource = VM.SuggestionPreviews;
				}
				else if(args.Property == AutoSuggestViewModel.SuggestionPreviewProperty)
				{
					if(!_suggestionPreviewChanging)
					{
						_selector.SelectedItem = VM.SuggestionPreview;
						if(!_deletingText)
							SetTargetTextBoxTextToSuggestionPreviewString();
					}
				}
				else if(args.Property == AutoSuggestViewModel.SuggestionProperty)
				{
					if(VM.IsConfirmed)
					{
						_selector.SelectedItem = VM.Suggestion;
						SetSuggestionDisplayText();
					}
				}
			}
			finally { _VMPropertyChanging = false; }
		}
		private bool _VMPropertyChanging;

		#region Confirm
		private bool _confirming;
		private void Confirm()
		{
			if (_confirming) return;
			_confirming = true;

			try
			{
				VM.Confirm(CurrentTokenText);

				SetSuggestionDisplayText();//If it is confirmed make sure that the displayed text matches the suggestion.
				CloseOwnerPopup();
			}
			finally { _confirming = false; }
		}
		#endregion 

		#region Cancel
		private void Cancel()
		{
			VM.Cancel();

			SetSuggestionDisplayText();//If it is confirmed make sure that the displayed text matches the suggestion.
			CloseOwnerPopup();
		}
		#endregion 

		private void Selector_MouseDoubleClick(object sender, RoutedEventArgs args)
		{
			Confirm();
			args.Handled = true;
			TabOutNext();
		}

		protected virtual void HandleKeyDown(object sender, KeyEventArgs e)
		{
			if(sender == TextTokensControlTarget)
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
				}
			}

			switch(e.Key)
			{
				case Key.Enter:
				{
					if(StyleModel != null && (StyleModel.TaboutTrigger & TaboutTriggers.Enter) == TaboutTriggers.Enter
					&& !OwnerPopup.IsOpen)
					{
						TabOutNext();
					}
					else
					{
						Confirm();
						if(StyleModel != null && (StyleModel.TaboutTrigger & TaboutTriggers.Enter) == TaboutTriggers.Enter
						&& (VM.Separators == null || VM.Separators.Count == 0))
							TabOutNext();
					}
					e.Handled = true;
					break;
				}
				case Key.Left:
				{
					if (TextTokensControl.IsCurosorAtTheBegginingOfText())
					{
						if (StyleModel != null && (StyleModel.ConfirmTrigger & ConfirmTriggers.Arrows) == ConfirmTriggers.Arrows)
						{
							Confirm();
							e.Handled = true;
						}
						if (StyleModel != null && (StyleModel.TaboutTrigger & TaboutTriggers.Arrows) == TaboutTriggers.Arrows)
						{
							TabOutPrevious();
							e.Handled = true;
						}
					}
					break;
				}
				case Key.Right:
				{
					if (TextTokensControl.IsCursorAtTheEndOfText())
					{
						if (StyleModel != null && (StyleModel.ConfirmTrigger & ConfirmTriggers.Arrows) == ConfirmTriggers.Arrows)
						{
							Confirm();
							e.Handled = true;
						}
						if (StyleModel != null && (StyleModel.TaboutTrigger & TaboutTriggers.Arrows) == TaboutTriggers.Arrows)
						{
							TabOutNext();
							e.Handled = true;
						}
					}
					break;
				}
				case Key.Space:
				{
					if(VM.SuggestionPreviews.Count() == 1 && !VM.IsFreeTextAllowed && VM.SuggestionPreview != null)
					{
						if(StyleModel != null && (StyleModel.ConfirmTrigger & ConfirmTriggers.Space) == ConfirmTriggers.Space)
						{
							Confirm();
							e.Handled = true;
						}
						if(StyleModel != null && (StyleModel.TaboutTrigger & TaboutTriggers.Space) == TaboutTriggers.Space)
						{
							TabOutNext();
							e.Handled = true;
						}
					}
					break;
				}
				case Key.Tab:
				{
					if(StyleModel != null && (StyleModel.ConfirmTrigger & ConfirmTriggers.Tab) == ConfirmTriggers.Tab)
					{
						Confirm();
					}
					break;
				}
				case Key.Escape:
				{
					Cancel();
					e.Handled = true;
					break;
				}
			}
		}
		private void HandleLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (IsKeyBoardFocusWithinAutoSuggestControls)
				return;

			Confirm();
		}
		private void TabOutNext() { TextTokensControlTarget.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)); }
		private void TabOutPrevious() { TextTokensControlTarget.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous)); }

		private void TextTokensControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(OwnerPopup == null || OwnerPopup.IsOpen) return;

			ApplyFilterAndShowSuggestions(true);
		}
		protected virtual void HandleGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if(_VMPropertyChanging || (OwnerPopup != null && OwnerPopup.IsOpen)) return;

			WireUpEvents();

			ApplyFilterAndShowSuggestions(true);
		}

		private string CurrentTokenText
		{
			get
			{
				if(TextTokensControl == null) return "";
				if(TextTokensControl.CurrentToken == null) return "";
				return TextTokensControl.CurrentToken.Text ?? "";
			}
		}
		private bool IsKeyBoardFocusWithinAutoSuggestControls
		{
			get
			{
				return TextTokensControl.Focused || IsKeyboardFocusWithin ||
						((Keyboard.FocusedElement is DependencyObject) && ((DependencyObject)Keyboard.FocusedElement).VisualChildOf(_selector));
			}
		}
		private void IsShowFilterTextInControlChanged(object sender, EventArgs args)
		{
			if(_searchCriteria == null) return;

			if(StyleModel != null)
				_searchCriteria.Height = StyleModel.IsFilterTextDisplayed ? 25 : 0;

			_searchCriteria.Content = TextTokensControl.GetFilterText();
		}
	}
}
