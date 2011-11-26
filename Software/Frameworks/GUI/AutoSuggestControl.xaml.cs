﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using KOControls.Core;
using KOControls.GUI.Core;

namespace KOControls.GUI
{
	#region enums
	[Flags]
	public enum ConfirmTriggers
	{
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
		private static readonly ResourceDictionary ResourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/KOControls.GUI;component/AutoSuggestControl.xaml") };
		public static readonly ControlTemplate Default_Template;
		public static readonly ControlTemplate Default_CommandsTemplate;
		public static readonly ControlTemplate Default_SuggestionsTemplate;

		static AutoSuggestControl()
		{
			Default_Template = (ControlTemplate)ResourceDictionary["AutoSuggestControl_DefaultTemplate"];
			Default_CommandsTemplate = (ControlTemplate)ResourceDictionary["AutoSuggestControl_Default_CommandsTemplate"];
			Default_SuggestionsTemplate = (ControlTemplate)ResourceDictionary["AutoSuggestControl_Default_SuggestionsTemplate"];

			ViewModel.OverrideProperty<AutoSuggestControl>(FrameworkElement.DataContextProperty, null, Dependents_Changed, (d, v) => { Dependents_Changing(d, v); return v as AutoSuggestViewModel; });
			ViewModel.OverrideProperty<AutoSuggestControl>(TemplateProperty, Default_Template, Dependents_Changed, (d, v) => { Dependents_Changing(d, v); return v; });

			DataContextProperty = ViewModel.IsInDesignMode ? ViewModel.RegisterProperty<AutoSuggestControl, AutoSuggestViewModel>("DataContext") : FrameworkElement.DataContextProperty;
		}
		public AutoSuggestControl()
		{
			Loaded += OnLoaded;

			CommandsTemplate = Default_CommandsTemplate;
			SuggestionsTemplate = Default_SuggestionsTemplate;

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

		#region ConfirmTrigger
		public static readonly DependencyProperty SelectionTriggerProperty = ViewModel.RegisterProperty<ConfirmTriggers, AutoSuggestViewModel>("ConfirmTrigger", ConfirmTriggers.SpaceTabArrows);
		public ConfirmTriggers ConfirmTrigger { get { return (ConfirmTriggers)GetValue(SelectionTriggerProperty); } set { SetValue(SelectionTriggerProperty, value); } }
		#endregion

		#region TaboutTrigger
		public static readonly DependencyProperty TaboutCommandProperty = ViewModel.RegisterProperty<TaboutTriggers, AutoSuggestViewModel>("TaboutTrigger", TaboutTriggers.Enter);
		public TaboutTriggers TaboutTrigger { get { return (TaboutTriggers)GetValue(TaboutCommandProperty); } set { SetValue(TaboutCommandProperty, value); } }
		#endregion

		private bool _initialized;
		private static object Dependents_Changing(DependencyObject d, object v)
		{
			var asc = (AutoSuggestControl)d;
			if(asc._initialized)
			{
				asc._initialized = false;
				ClearEvents(asc);
			}

			return v;
		}
		private static void Dependents_Changed(DependencyObject d, DependencyPropertyChangedEventArgs a)
		{
			var asc = (AutoSuggestControl)d;
			if(asc._initialized || !asc.IsLoaded || asc.Template == null || asc.DataContext == null || asc.SuggestionsTemplate == null || asc.TargetTextBox == null)
				return;

			asc._suggestionsContentPresenter = (Control)asc.Template.FindName("_suggestionsContentPresenter", asc);
			asc._suggestionsContentPresenter.Template = asc.SuggestionsTemplate;
			asc._suggestionsContentPresenter.ApplyTemplate();
			asc._selector = (Selector)asc._suggestionsContentPresenter.Template.FindName("PART_Selector", asc._suggestionsContentPresenter);

			asc._commandsContentPresenter = (Control)asc.Template.FindName("_commandsContentPresenter", asc);
			asc._commandsContentPresenter.Template = asc.CommandsTemplate;
			asc._commandsContentPresenter.ApplyTemplate();

			asc._initialized = true;
			WireUpEvents(asc);
		}

		private static void ClearEvents(AutoSuggestControl asc)
		{
			if(asc.DataContext != null)
			{
				asc.DataContext.Refreshed -= asc.DataContext_Refreshed;
				asc.DataContext.PropertyChanged -= asc.DataContext_PropertyChanged;
				asc.DataContext.FilterApplied -= asc.DataContext_FilterApplied;
			}
			if(asc._selector != null)
			{
				asc._selector.SelectionChanged -= asc.Selector_SelectionChanged;
				asc._selector.RemoveHandler(MouseDoubleClickEvent, (RoutedEventHandler)asc.Selector_MouseDoubleClick);

				asc._selector.PreviewKeyDown -= asc.HandleKeyDown;
				asc._selector.PreviewKeyUp -= asc.HandleKeyUp;
				asc._selector.LostKeyboardFocus -= asc.HandleLostKeyboardFocus;
			}
			if(asc.TargetTextBox != null)
			{
				asc.TargetTextBox.PreviewTextInput -= asc.TargetTextBox_TextInput;
				asc.TargetTextBox.TextChanged -= asc.TargetTextBox_TextChanged;
				asc.TargetTextBox.PreviewKeyDown -= asc.HandleKeyDown;
				asc.TargetTextBox.PreviewKeyUp -= asc.HandleKeyUp;
				asc.TargetTextBox.GotKeyboardFocus -= asc.HandleGotKeyboardFocus;
				asc.TargetTextBox.LostKeyboardFocus -= asc.HandleLostKeyboardFocus;
			}
		}

		private static void WireUpEvents(AutoSuggestControl asc)
		{
			ClearEvents(asc);

			asc.DataContext.Refreshed += asc.DataContext_Refreshed;
			asc.DataContext.PropertyChanged += asc.DataContext_PropertyChanged;
			asc.DataContext.FilterApplied += asc.DataContext_FilterApplied;

			asc._selector.SelectionChanged += asc.Selector_SelectionChanged;
			asc._selector.AddHandler(Control.MouseDoubleClickEvent, (RoutedEventHandler)asc.Selector_MouseDoubleClick, true);
			asc._selector.PreviewKeyDown += asc.HandleKeyDown;
			asc._selector.PreviewKeyUp += asc.HandleKeyUp;
			asc._selector.LostKeyboardFocus += asc.HandleLostKeyboardFocus;

			asc.TargetTextBox.PreviewTextInput += asc.TargetTextBox_TextInput;
			asc.TargetTextBox.TextChanged += asc.TargetTextBox_TextChanged;
			asc.TargetTextBox.PreviewKeyDown += asc.HandleKeyDown;
			asc.TargetTextBox.PreviewKeyUp += asc.HandleKeyUp;
			asc.TargetTextBox.GotKeyboardFocus += asc.HandleGotKeyboardFocus;
			asc.TargetTextBox.LostKeyboardFocus += asc.HandleLostKeyboardFocus;

			asc.SetTargetTextBoxTextToSuggestionString();
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
					_suggestionPreviewChanging = false;

					SetSuggestionPreview();
				}
				else
				{
					if(_selector.ItemsSource.Count() > 0)
					{
						if(String.IsNullOrEmpty(TargetTextBoxText) && DataContext.IsEmptyValueAllowed)
							_selector.SelectedIndex = -1;
						else
							_selector.SelectedIndex = 0;
					}
					else
					{
						_selector.SelectedIndex = -1;
					}
					_suggestionPreviewChanging = false;
					SetSuggestionPreview();
					SetTargetTextBoxTextToSuggestionPreviewString();
				}
			}
			finally { _suggestionPreviewChanging = false; _deletingText = false; }
		}

		private void DataContext_Refreshed()
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
				var fullText = DataContext.SuggestionPreviewString ?? "";
				var text = TargetTextBoxTextWithoutTrailingSelection;
				if(fullText.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
				{
					TargetTextBox.Text = fullText;
					TargetTextBox.CaretIndex = text.Length;
					TargetTextBox.Select(text.Length, fullText.Length);
				}
			}
			finally { _changingTexBoxText = false; }
		}

		private void SetTargetTextBoxTextToSuggestionString()
		{
			TargetTextBox.Text = DataContext.SuggestionString;
		}

		private bool _dataContextPropertyChanging;

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
				SetTargetTextBoxTextToSuggestionPreviewString();
				Dispatcher.BeginInvoke((Action)delegate
				{
					if(IsKeyBoardFocusWithinAutoSuggestControls)
						TargetTextBox.Focus();
				});
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
			}
			finally { _userInput = false; }
		}

		private void TargetTextBox_TextInput(object sender, TextCompositionEventArgs e)
		{
			_userInput = true;
		}

		private void HandleKeyDown(object sender, KeyEventArgs e)
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
						HandleConfirm();
						if((TaboutTrigger & TaboutTriggers.Enter) == TaboutTriggers.Enter)
							TabOutNext();
						e.Handled = true;
						break;
					}
				case Key.Left:
					{
						if(TargetTextBox.CaretIndex == 0)
						{
							if((ConfirmTrigger & ConfirmTriggers.Arrows) == ConfirmTriggers.Arrows)
							{
								HandleConfirm();
								e.Handled = true;
							}
							if((TaboutTrigger & TaboutTriggers.Arrows) == TaboutTriggers.Arrows)
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
							if((ConfirmTrigger & ConfirmTriggers.Arrows) == ConfirmTriggers.Arrows)
							{
								HandleConfirm();
								e.Handled = true;
							}
							if((TaboutTrigger & TaboutTriggers.Arrows) == TaboutTriggers.Arrows)
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
							if((ConfirmTrigger & ConfirmTriggers.Space) == ConfirmTriggers.Space)
							{
								HandleConfirm();
								e.Handled = true;
							}
							if((TaboutTrigger & TaboutTriggers.Space) == TaboutTriggers.Space)
							{
								TabOutNext();
								e.Handled = true;
							}
						}
						break;
					}
				case Key.Tab:
					{
						if((ConfirmTrigger & ConfirmTriggers.Tab) == ConfirmTriggers.Tab)
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
		private void HandleGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if(_dataContextPropertyChanging || (OwnerPopup != null && OwnerPopup.IsOpen)) return;

			WireUpEvents(this);
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

		private void SelectAll()
		{
			TargetTextBox.CaretIndex = TargetTextBoxTextWithoutTrailingSelection.Length;
			TargetTextBox.SelectAll();
		}

		private string TargetTextBoxText
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