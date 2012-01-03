using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using KOControls.Core;
using KOControls.GUI.Core;

namespace KOControls.GUI
{
	public abstract class AutoSuggestBaseViewModel: ViewModel
	{
		public AutoSuggestBaseViewModel()
		{
			Commands = new ObservableCollection<CommandBase>();
			ConfirmCommand = new Command(Confirm);
			RefreshCommand = new Command(Refresh);

			DelayTimer = new DispatcherTimer();
			DelayTimer.Interval = TimeSpan.FromMilliseconds(0);
			DelayTimer.IsEnabled = false;

			DelayTimer.Tick += (a, b) =>
			{
				DelayTimer.IsEnabled = false;
				ApplyFilter();
			};
		}
		
		public AutoSuggestBaseViewModel(ISelector selector, IValueConverter suggestionPreviewToStringConverter = null, object styleModel = null)
			: this()
		{
			Selector = selector;
			SuggestionPreviewToStringConverter = suggestionPreviewToStringConverter;
			StyleModel = styleModel;
		}

		#region Suggestions
		private static readonly DependencyPropertyKey SuggestionsPropertyKey = ViewModel.RegisterReadOnlyProperty<IEnumerable, AutoSuggestBaseViewModel>("Suggestions", null, null, (d, v) => ((AutoSuggestBaseViewModel)d).OnPropertyChanging(SuggestionsProperty, v));
		public static DependencyProperty SuggestionsProperty { get { return SuggestionsPropertyKey.DependencyProperty; } }
		public IEnumerable Suggestions { get { return (IEnumerable)GetValue(SuggestionsPropertyKey); } protected set { SetValue(SuggestionsPropertyKey, value); } }
		#endregion

		#region SuggestionPreview
		public static readonly DependencyProperty SuggestionPreviewProperty = ViewModel.RegisterProperty<object, AutoSuggestBaseViewModel>("SuggestionPreview", null, null, (d, v) => ((AutoSuggestBaseViewModel)d).OnPropertyChanging(SuggestionPreviewProperty, v));
		public object SuggestionPreview { get { return GetValue(SuggestionPreviewProperty); } set { SetValue(SuggestionPreviewProperty, value); } }
		#endregion

		#region Filter
		public static readonly DependencyProperty SelectorProperty = ViewModel.RegisterProperty<ISelector, AutoSuggestBaseViewModel>("Selector", null, null, (d, v) => ((AutoSuggestBaseViewModel)d).OnPropertyChanging(SelectorProperty, v));
		public ISelector Selector { get { return (ISelector)GetValue(SelectorProperty); } set { SetValue(SelectorProperty, value); } }
		#endregion

		#region Delay
		public static readonly DependencyProperty DelayProperty = ViewModel.RegisterProperty<TimeSpan, AutoSuggestBaseViewModel>("Delay", new TimeSpan(0), DelayValueChanged, null);
		public TimeSpan Delay { get { return (TimeSpan)GetValue(DelayProperty); } set { SetValue(DelayProperty, value); } }

		protected DispatcherTimer DelayTimer;

		private static void DelayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AutoSuggestBaseViewModel asVM = d as AutoSuggestBaseViewModel;
			if(asVM != null)
			{
				TimeSpan ts = (TimeSpan)e.NewValue;
				if(ts != null)
				{
					if(ts.TotalMilliseconds == 0)
						asVM.DelayTimer.IsEnabled = false;
					asVM.DelayTimer.Interval = ts;
				}
			}
		}
		#endregion

		#region SuggestionPreviewToStringConverter
		public static readonly DependencyProperty SuggestionPreviewToStringConverterProperty = ViewModel.RegisterProperty<IValueConverter, AutoSuggestBaseViewModel>("SuggestionPreviewToStringConverter", DefaultSuggestionToStringConverter.Instance, null, (d, v) => ((AutoSuggestBaseViewModel)d).OnPropertyChanging(SuggestionPreviewToStringConverterProperty, v));
		public IValueConverter SuggestionPreviewToStringConverter { get { return (IValueConverter)GetValue(SuggestionPreviewToStringConverterProperty); } set { SetValue(SuggestionPreviewToStringConverterProperty, value); } }
		public string SuggestionPreviewToString(object suggestion) { return (string)SuggestionPreviewToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture); }
		#endregion

		#region FreeTextToSuggestionConverter
		public delegate bool TryConvertFreeTextToSuggestion(string filterInput, out object suggestion);
		public static readonly DependencyProperty FreeTextToSuggestionConverterProperty = ViewModel.RegisterProperty<TryConvertFreeTextToSuggestion, AutoSuggestBaseViewModel>("FreeTextToSuggestionConverter", null, null, (d, v) => ((AutoSuggestBaseViewModel)d).OnPropertyChanging(FreeTextToSuggestionConverterProperty, v));
		public TryConvertFreeTextToSuggestion FreeTextToSuggestionConverter { get { return (TryConvertFreeTextToSuggestion)GetValue(FreeTextToSuggestionConverterProperty); } set { SetValue(FreeTextToSuggestionConverterProperty, value); } }
		#endregion

		#region IsFreeTextAllowed
		public static readonly DependencyProperty IsFreeTextAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestBaseViewModel>("IsFreeTextAllowed");
		public bool IsFreeTextAllowed { get { return (bool)GetValue(IsFreeTextAllowedProperty); } set { SetValue(IsFreeTextAllowedProperty, value); } }
		#endregion

		#region IsEmptyValueAllowed
		public static readonly DependencyProperty IsEmptyValueAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestBaseViewModel>("IsEmptyValueAllowed", false, null, (d, v) => ((AutoSuggestBaseViewModel)d).OnPropertyChanging(IsEmptyValueAllowedProperty, v));
		public bool IsEmptyValueAllowed { get { return (bool)GetValue(IsEmptyValueAllowedProperty); } set { SetValue(IsEmptyValueAllowedProperty, value); } }
		#endregion

		#region StyleModel
		public static readonly DependencyProperty StyleModelProperty = ViewModel.RegisterProperty<object, AutoSuggestBaseViewModel>("StyleModel",null);
		public object StyleModel { get { return GetValue(StyleModelProperty); } set { SetValue(StyleModelProperty, value); } }
		#endregion
	
		#region EmptyValue
		public static readonly DependencyProperty EmptyValueProperty = ViewModel.RegisterProperty<object, AutoSuggestBaseViewModel>("EmptyValue", null, null, (d, v) => ((AutoSuggestBaseViewModel)d).OnPropertyChanging(EmptyValueProperty, v));
		public object EmptyValue { get { return (object)GetValue(EmptyValueProperty); } set { SetValue(EmptyValueProperty, value); } }
		#endregion

		#region ApplyFilterCommand
		//You may bind the command to a search button next to the textbox. This can be used in combination with the ApplyFilterTrigger Option.
		private static readonly DependencyPropertyKey ApplyFilterCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestBaseViewModel>("ApplyFilterCommand");
		public static DependencyProperty ApplyFilterCommandProperty { get { return ApplyFilterCommandPropertyKey.DependencyProperty; } }
		public Command ApplyFilterCommand { get { return (Command)GetValue(ApplyFilterCommandPropertyKey); } private set { SetValue(ApplyFilterCommandPropertyKey, value); } }

		private void ApplyFilter()
		{
			Suggestions = Selector.Select(_applyingFilterInput);
			FilterApplied();

			if (!ValidateSuggestionAgainsCurrentSuggestions(SuggestionPreview))
				SuggestionPreview = null;
		}

		public void ApplyFilter(string filterInput)
		{
			_applyingFilterInput = filterInput;

			DelayTimer.IsEnabled = false;
			if (Delay.TotalMilliseconds == 0)
				ApplyFilter();
			else
				DelayTimer.IsEnabled = true;
		}
		private string _applyingFilterInput;
		#endregion
		#region ConfirmCommand
		private static readonly DependencyPropertyKey ConfirmCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestBaseViewModel>("ConfirmCommand");
		public static DependencyProperty ConfirmCommandProperty { get { return ConfirmCommandPropertyKey.DependencyProperty; } }
		public Command ConfirmCommand { get { return (Command)GetValue(ConfirmCommandPropertyKey); } private set { SetValue(ConfirmCommandPropertyKey, value); } }
		public abstract void Confirm(string filterInput);

		private void Confirm(object filterInput)
		{
			DelayTimer.IsEnabled = false;

			Confirm(filterInput as string);

			OnConfirmed(filterInput);
		}

		public event Action<object> Confirmed = (object filterInput) => { };
		protected virtual void OnConfirmed(object filterInput)
		{
			Confirmed(filterInput);
		}
		#endregion

		#region RefreshCommand - Manual notification for property changes in Suggestion or SuggestionPreview (must happen after suggestion edit). 
		private static readonly DependencyPropertyKey RefreshCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestBaseViewModel>("RefreshCommand");
		public static DependencyProperty RefreshCommandProperty { get { return RefreshCommandPropertyKey.DependencyProperty; } }
		public Command RefreshCommand { get { return (Command)GetValue(RefreshCommandPropertyKey); } private set { SetValue(RefreshCommandPropertyKey, value); } }
		public void Refresh() { Refresh(null); }
		private void Refresh(object parameter)
		{
			DelayTimer.IsEnabled = false;

			if(Refreshed != null) Refreshed();
		}
		public event Action Refreshed;
		#endregion

		public event Action FilterApplied = () => { };

		protected virtual object OnPropertyChanging(DependencyProperty property, object value)
		{
			if(property == SuggestionPreviewProperty)
			{
				if(Equals(value, null) || string.Empty.Equals(value)) value = EmptyValue;
			}

			return value;
		}

		protected virtual bool ValidateSuggestionAgainsCurrentSuggestions(object suggestion)
		{
			if(IsEmptyValueAllowed && Equals(suggestion, EmptyValue)) return true;
			if(Equals(suggestion, null)) return false;
			if(Equals(suggestion, SuggestionPreview)) return true;

			if(Suggestions != null)
			{
				foreach(var next in Suggestions)
					if(Equals(next, suggestion))
						return true;
			}

			return false;
		}

		#region Commands - optional
		private static readonly DependencyPropertyKey CommandsPropertyKey = ViewModel.RegisterReadOnlyProperty<ObservableCollection<CommandBase>, AutoSuggestBaseViewModel>("Commands");
		public static DependencyProperty CommandsProperty { get { return CommandsPropertyKey.DependencyProperty; } }
		public ObservableCollection<CommandBase> Commands { get { return (ObservableCollection<CommandBase>)GetValue(CommandsPropertyKey); } private set { SetValue(CommandsPropertyKey, value); } }
		#endregion

		#region Nested classes
		public class DefaultSuggestionToStringConverter : IValueConverter
		{
			public static readonly DefaultSuggestionToStringConverter Instance = new DefaultSuggestionToStringConverter();

			private DefaultSuggestionToStringConverter() { }

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return value + string.Empty; }
			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return value; }
		}

		public class DefaultSelector : ISelector
		{
			public DefaultSelector(IValueConverter suggestionToStringConverter, IEnumerable suggestionsSource)
			{
				_suggestionToStringConverter = suggestionToStringConverter;
				_suggestionsSource = suggestionsSource;
			}

			private IEnumerable _suggestionsSource;
			private IValueConverter _suggestionToStringConverter;

			private string SuggestionToString(object suggestion)
			{
				return _suggestionToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture) + "";
			}

			public IEnumerable Select(object filterInput)
			{
				string filterInputAsString = filterInput as string ?? String.Empty;

				return (from object next in _suggestionsSource
						where SuggestionToString(next).StartsWith(filterInputAsString, StringComparison.CurrentCultureIgnoreCase)
						select next).ToArray();
			}
		}
		#endregion
	}
}
