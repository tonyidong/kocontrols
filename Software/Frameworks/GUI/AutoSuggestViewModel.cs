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
	//TBD:
	//5. Copy/Paste does not work.
	public class AutoSuggestViewModel : ViewModel
	{
		public AutoSuggestViewModel(ISelector selector, IValueConverter suggestionToStringConverter=null)
		{
			Commands = new ObservableCollection<CommandBase>();
			ConfirmCommand = new Command(Confirm);
			CancelCommand = new Command(Cancel);
			ClearCommand = new Command(Clear);
			RefreshCommand = new Command(Refresh);

			Selector = selector;

			SuggestionToStringConverter = suggestionToStringConverter;

			_delayTimer = new DispatcherTimer();
			_delayTimer.Interval = TimeSpan.FromMilliseconds(0);
			_delayTimer.IsEnabled = false;

			_delayTimer.Tick += (a, b) =>
			{
				_delayTimer.IsEnabled = false;
				ApplyFilter();
			};
		}

		#region Suggestions
		private static readonly DependencyPropertyKey SuggestionsPropertyKey = ViewModel.RegisterReadOnlyProperty<IEnumerable, AutoSuggestViewModel>("Suggestions", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionsProperty, v));
		public static DependencyProperty SuggestionsProperty { get { return SuggestionsPropertyKey.DependencyProperty; } }
		public IEnumerable Suggestions { get { return (IEnumerable)GetValue(SuggestionsPropertyKey); } private set { SetValue(SuggestionsPropertyKey, value); } }
		#endregion

		#region SuggestionPreview
		public static readonly DependencyProperty SuggestionPreviewProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("SuggestionPreview", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionPreviewProperty, v));
		public object SuggestionPreview { get { return GetValue(SuggestionPreviewProperty); } set { SetValue(SuggestionPreviewProperty, value); } }

		public string SuggestionPreviewString { get { return SuggestionToString(SuggestionPreview); } }
		#endregion

		#region Suggestion
		public static readonly DependencyProperty SuggestionProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("Suggestion", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionProperty, v));
		public object Suggestion { get { return GetValue(SuggestionProperty); } set { SetValue(SuggestionProperty, value); } }

		public string SuggestionString { get { return SuggestionToString(Suggestion); } }
		#endregion

		#region ApplyFilter
		private void ApplyFilter()
		{
			Suggestions = Selector.Select(_applyingFilterInput);

			FilterApplied();

			if(!ValidateSuggestionAgainsCurrentSuggestions(SuggestionPreview))
				SuggestionPreview = null;
		}

		public void ApplyFilter(string filterInput)
		{
			_applyingFilterInput = filterInput;

			_delayTimer.IsEnabled = false;
			if(Delay.TotalMilliseconds == 0)
				ApplyFilter();
			else
				_delayTimer.IsEnabled = true;
		}
		private string _applyingFilterInput;
		#endregion

		#region Filter
		public static readonly DependencyProperty SelectorProperty = ViewModel.RegisterProperty<ISelector, AutoSuggestViewModel>("Selector", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SelectorProperty, v));
		public ISelector Selector { get { return (ISelector)GetValue(SelectorProperty); } set { SetValue(SelectorProperty, value); } }
		#endregion

		#region Delay
		public static readonly DependencyProperty DelayProperty = ViewModel.RegisterProperty<TimeSpan, AutoSuggestViewModel>("Delay", new TimeSpan(0), DelayValueChanged, null);
		public TimeSpan Delay { get { return (TimeSpan)GetValue(DelayProperty); } set { SetValue(DelayProperty, value); } }

		private DispatcherTimer _delayTimer;

		private static void DelayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AutoSuggestViewModel asVM = d as AutoSuggestViewModel;
			if(asVM != null)
			{
				TimeSpan ts = (TimeSpan)e.NewValue;
				if(ts != null)
				{
					if(ts.TotalMilliseconds == 0)
						asVM._delayTimer.IsEnabled = false;
					asVM._delayTimer.Interval = ts;
				}
			}
		}
		#endregion

		#region SuggestionToStringConverter
		public static readonly DependencyProperty SuggestionToStringConverterProperty = ViewModel.RegisterProperty<IValueConverter, AutoSuggestViewModel>("SuggestionToStringConverter", DefaultSuggestionToStringConverter.Instance, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionToStringConverterProperty, v));
		public IValueConverter SuggestionToStringConverter { get { return (IValueConverter)GetValue(SuggestionToStringConverterProperty); } set { SetValue(SuggestionToStringConverterProperty, value); } }
		public string SuggestionToString(object suggestion) { return (string)SuggestionToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture); }
		#endregion

		#region FreeTextToSuggestionConverter
		public delegate bool TryConvertFreeTextToSuggestion(string filterInput, out object suggestion);
		public static readonly DependencyProperty FreeTextToSuggestionConverterProperty = ViewModel.RegisterProperty<TryConvertFreeTextToSuggestion, AutoSuggestViewModel>("FreeTextToSuggestionConverter", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(FreeTextToSuggestionConverterProperty, v));
		public TryConvertFreeTextToSuggestion FreeTextToSuggestionConverter { get { return (TryConvertFreeTextToSuggestion)GetValue(FreeTextToSuggestionConverterProperty); } set { SetValue(FreeTextToSuggestionConverterProperty, value); } }
		#endregion

		#region IsFreeTextAllowed
		public static readonly DependencyProperty IsFreeTextAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestViewModel>("IsFreeTextAllowed");
		public bool IsFreeTextAllowed { get { return (bool)GetValue(IsFreeTextAllowedProperty); } set { SetValue(IsFreeTextAllowedProperty, value); } }
		#endregion

		#region IsEmptyValueAllowed
		public static readonly DependencyProperty IsEmptyValueAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestViewModel>("IsEmptyValueAllowed", false, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(IsEmptyValueAllowedProperty, v));
		public bool IsEmptyValueAllowed { get { return (bool)GetValue(IsEmptyValueAllowedProperty); } set { SetValue(IsEmptyValueAllowedProperty, value); } }
		#endregion

		#region EmptyValue
		public static readonly DependencyProperty EmptyValueProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("EmptyValue", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(EmptyValueProperty, v));
		public object EmptyValue { get { return (object)GetValue(EmptyValueProperty); } set { SetValue(EmptyValueProperty, value); } }
		#endregion

		#region IsConfirmed
		private static readonly DependencyPropertyKey IsConfirmedPropertyKey = ViewModel.RegisterReadOnlyProperty<bool, AutoSuggestViewModel>("IsConfirmed");
		public static DependencyProperty IsConfirmedProperty { get { return IsConfirmedPropertyKey.DependencyProperty; } }
		public bool IsConfirmed { get { return (bool)GetValue(IsConfirmedPropertyKey); } private set { SetValue(IsConfirmedPropertyKey, value); } }
		#endregion

		#region ConfirmCommand
		private static readonly DependencyPropertyKey ConfirmCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("ConfirmCommand");
		public static DependencyProperty ConfirmCommandProperty { get { return ConfirmCommandPropertyKey.DependencyProperty; } }
		public Command ConfirmCommand { get { return (Command)GetValue(ConfirmCommandPropertyKey); } private set { SetValue(ConfirmCommandPropertyKey, value); } }
		public void Confirm(string filterInput)
		{
			if(filterInput == null || SuggestionToString(SuggestionPreview) == filterInput)
			{
				if(ValidateSuggestion(SuggestionPreview))
					Suggestion = SuggestionPreview;
				else
					Cancel();
			}
			else
			{
				if(IsFreeTextAllowed && FreeTextToSuggestionConverter != null)
				{
					object suggestion;
					var conversionResult = FreeTextToSuggestionConverter(filterInput, out suggestion);
					if(conversionResult)
						Suggestion = SuggestionPreview = suggestion;
					else
						Cancel();
				}
				else
					Cancel();
			}
		}
		private void Confirm(object filterInput)
		{
			_delayTimer.IsEnabled = false;

			Confirm(filterInput as string);
		}
		#endregion
		#region CancelCommand
		private static readonly DependencyPropertyKey CancelCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("CancelCommand");
		public static DependencyProperty CancelCommandProperty { get { return CancelCommandPropertyKey.DependencyProperty; } }
		public Command CancelCommand { get { return (Command)GetValue(CancelCommandPropertyKey); } private set { SetValue(CancelCommandPropertyKey, value); } }
		public void Cancel() { Cancel(null); }
		private void Cancel(object parameter)
		{
			_delayTimer.IsEnabled = false;

			SuggestionPreview = Suggestion;
		}
		#endregion
		#region ClearCommand
		private static readonly DependencyPropertyKey ClearCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("ClearCommand");
		public static DependencyProperty ClearCommandProperty { get { return ClearCommandPropertyKey.DependencyProperty; } }
		public Command ClearCommand { get { return (Command)GetValue(ClearCommandPropertyKey); } private set { SetValue(ClearCommandPropertyKey, value); } }
		public void Clear() { Clear(null); }
		private void Clear(object parameter)
		{
			_delayTimer.IsEnabled = false;

			SuggestionPreview = null;
			Suggestion = null;
		}
		#endregion
		#region RefreshCommand
		private static readonly DependencyPropertyKey RefreshCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("RefreshCommand");
		public static DependencyProperty RefreshCommandProperty { get { return RefreshCommandPropertyKey.DependencyProperty; } }
		public Command RefreshCommand { get { return (Command)GetValue(RefreshCommandPropertyKey); } private set { SetValue(RefreshCommandPropertyKey, value); } }
		public void Refresh() { Refresh(null); }
		private void Refresh(object parameter)
		{
			_delayTimer.IsEnabled = false;

			if(Refreshed != null) Refreshed();
		}
		public event Action Refreshed;
		#endregion

		public event Action FilterApplied = () => { };

		private object OnPropertyChanging(DependencyProperty property, object value)
		{
			if(property == SuggestionProperty)
			{
				if(Equals(value, null) || string.Empty.Equals(value)) value = EmptyValue;

				if(IsFreeTextAllowed)
				{
					if(IsEmptyValueAllowed)
					{ }
					else
						if(Equals(value, EmptyValue))
							value = Suggestion;
				}
				else
				{
					if(IsEmptyValueAllowed)
					{
						if(!Equals(value, EmptyValue) && !ValidateSuggestion(value))
							value = Suggestion;
					}
					else
					{
						if(Equals(value, EmptyValue) || !ValidateSuggestion(value))
							value = Suggestion;
					}
				}
			}
			else if(property == SuggestionPreviewProperty)
			{
				if(Equals(value, null) || string.Empty.Equals(value)) value = EmptyValue;
			}

			return value;
		}
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if(e.Property == SuggestionPreviewProperty || e.Property == SuggestionProperty)
			{
				if(e.Property == SuggestionProperty)
					Cancel();

				IsConfirmed = Equals(Suggestion, SuggestionPreview);
			}

			base.OnPropertyChanged(e);
		}
		protected virtual bool ValidateSuggestion(object suggestion)
		{
			if(!ValidateSuggestionAgainsCurrentSuggestions(suggestion))
			{
				var suggestions = Selector.Select(SuggestionToString(suggestion));
				if(suggestions != null)
					foreach(var next in suggestions)
						if(Equals(next, suggestion))
							return true;
			}
			else
			{
				return true;
			}
			return false;
		}

		protected virtual bool ValidateSuggestionAgainsCurrentSuggestions(object suggestion)
		{
			if(IsEmptyValueAllowed && Equals(suggestion, EmptyValue)) return true;
			if(Equals(suggestion, null)) return false;
			if(Equals(suggestion, Suggestion) || Equals(suggestion, SuggestionPreview)) return true;

			if(Suggestions != null)
			{
				foreach(var next in Suggestions)
					if(Equals(next, suggestion))
						return true;
			}

			return false;
		}

		#region Commands - optional
		private static readonly DependencyPropertyKey CommandsPropertyKey = ViewModel.RegisterReadOnlyProperty<ObservableCollection<CommandBase>, AutoSuggestViewModel>("Commands");
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
