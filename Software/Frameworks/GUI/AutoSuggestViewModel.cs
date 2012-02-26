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
		public AutoSuggestViewModel()
		{
			Commands = new ObservableCollection<CommandBase>();

			DelayTimer = new DispatcherTimer();
			DelayTimer.Interval = TimeSpan.FromMilliseconds(0);
			DelayTimer.IsEnabled = false;
			DelayTimer.Tick += (a, b) =>
			{
				DelayTimer.IsEnabled = false;
				ApplyFilter();
			};
			ApplyFilterCommand = new Command(ApplyFilter);
			ConfirmCommand = new Command(Confirm);
			RefreshCommand = new Command(Refresh);
			CancelCommand = new Command(Cancel);
			ClearCommand = new Command(Clear);
		}

		public AutoSuggestViewModel(ISelector selector, IValueConverter suggestionToStringConverter = null, IValueConverter suggestionPreviewToStringConverter = null, object styleModel = null)
			:this()
		{
			Selector = selector;
			SuggestionToStringConverter = suggestionToStringConverter ?? DefaultSuggestionToStringConverter.Instance;
			SuggestionPreviewToStringConverter = suggestionPreviewToStringConverter ?? SuggestionToStringConverter;
			StyleModel = styleModel??AutoSuggestControlStyleViewModel.CreateDefaultInstance();
		}

		#region Suggestion
		public static readonly DependencyProperty SuggestionProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("Suggestion", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionProperty, v));
		public object Suggestion { get { return GetValue(SuggestionProperty); } set { SetValue(SuggestionProperty, value); } }
		#endregion

		#region CancelCommand
		private static readonly DependencyPropertyKey CancelCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("CancelCommand");
		public static DependencyProperty CancelCommandProperty { get { return CancelCommandPropertyKey.DependencyProperty; } }
		public Command CancelCommand { get { return (Command)GetValue(CancelCommandPropertyKey); } private set { SetValue(CancelCommandPropertyKey, value); } }
		public void Cancel() { Cancel(null); }
		private void Cancel(object parameter)
		{
			DelayTimer.IsEnabled = false;

			SuggestionPreview = Suggestion;

			OnCancelled();
		}

		public event Action Cancelled = () => { };
		protected virtual void OnCancelled()
		{
			Cancelled();
		}
		#endregion
		#region ClearCommand
		private static readonly DependencyPropertyKey ClearCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("ClearCommand");
		public static DependencyProperty ClearCommandProperty { get { return ClearCommandPropertyKey.DependencyProperty; } }
		public Command ClearCommand { get { return (Command)GetValue(ClearCommandPropertyKey); } private set { SetValue(ClearCommandPropertyKey, value); } }
		public void Clear() { Clear(null); }
		private void Clear(object parameter)
		{
			DelayTimer.IsEnabled = false;

			SuggestionPreview = null;
			Suggestion = null;
		}
		#endregion

		#region IsConfirmed
		private static readonly DependencyPropertyKey IsConfirmedPropertyKey = ViewModel.RegisterReadOnlyProperty<bool, AutoSuggestViewModel>("IsConfirmed");
		public static DependencyProperty IsConfirmedProperty { get { return IsConfirmedPropertyKey.DependencyProperty; } }
		public bool IsConfirmed { get { return (bool)GetValue(IsConfirmedPropertyKey); } private set { SetValue(IsConfirmedPropertyKey, value); } }
		#endregion

		#region Suggestions
		private static readonly DependencyPropertyKey SuggestionsPropertyKey = ViewModel.RegisterReadOnlyProperty<IEnumerable, AutoSuggestViewModel>("Suggestions", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionsProperty, v));
		public static DependencyProperty SuggestionsProperty { get { return SuggestionsPropertyKey.DependencyProperty; } }
		public IEnumerable Suggestions { get { return (IEnumerable)GetValue(SuggestionsPropertyKey); } protected set { SetValue(SuggestionsPropertyKey, value); } }
		#endregion

		#region SuggestionPreview
		public static readonly DependencyProperty SuggestionPreviewProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("SuggestionPreview", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionPreviewProperty, v));
		public object SuggestionPreview { get { return GetValue(SuggestionPreviewProperty); } set { SetValue(SuggestionPreviewProperty, value); } }
		#endregion

		#region Filter
		public static readonly DependencyProperty SelectorProperty = ViewModel.RegisterProperty<ISelector, AutoSuggestViewModel>("Selector", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SelectorProperty, v));
		public ISelector Selector { get { return (ISelector)GetValue(SelectorProperty); } set { SetValue(SelectorProperty, value); } }
		#endregion

		#region Delay
		public static readonly DependencyProperty DelayProperty = ViewModel.RegisterProperty<TimeSpan, AutoSuggestViewModel>("Delay", new TimeSpan(0), DelayValueChanged, null);
		public TimeSpan Delay { get { return (TimeSpan)GetValue(DelayProperty); } set { SetValue(DelayProperty, value); } }

		protected DispatcherTimer DelayTimer;

		private static void DelayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AutoSuggestViewModel asVM = d as AutoSuggestViewModel;
			if (asVM != null)
			{
				TimeSpan ts = (TimeSpan)e.NewValue;
				if (ts != null)
				{
					if (ts.TotalMilliseconds == 0)
						asVM.DelayTimer.IsEnabled = false;
					asVM.DelayTimer.Interval = ts;
				}
			}
		}
		#endregion

		#region SuggestionPreviewToStringConverter
		public static readonly DependencyProperty SuggestionPreviewToStringConverterProperty = ViewModel.RegisterProperty<IValueConverter, AutoSuggestViewModel>("SuggestionPreviewToStringConverter", DefaultSuggestionToStringConverter.Instance, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionPreviewToStringConverterProperty, v));
		public IValueConverter SuggestionPreviewToStringConverter { get { return (IValueConverter)GetValue(SuggestionPreviewToStringConverterProperty); } set { SetValue(SuggestionPreviewToStringConverterProperty, value); } }
		public string SuggestionPreviewToString(object suggestion) { return (string)SuggestionPreviewToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture); }
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

		#region StyleModel
		public static readonly DependencyProperty StyleModelProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("StyleModel", null);
		public object StyleModel { get { return GetValue(StyleModelProperty); } set { SetValue(StyleModelProperty, value); } }
		#endregion

		#region EmptyValue
		public static readonly DependencyProperty EmptyValueProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("EmptyValue", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(EmptyValueProperty, v));
		public object EmptyValue { get { return (object)GetValue(EmptyValueProperty); } set { SetValue(EmptyValueProperty, value); } }
		#endregion

		#region ApplyFilterCommand
		//You may bind the command to a search button next to the textbox. This can be used in combination with the ApplyFilterTrigger Option.
		private static readonly DependencyPropertyKey ApplyFilterCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("ApplyFilterCommand");
		public static DependencyProperty ApplyFilterCommandProperty { get { return ApplyFilterCommandPropertyKey.DependencyProperty; } }
		public Command ApplyFilterCommand { get { return (Command)GetValue(ApplyFilterCommandPropertyKey); } private set { SetValue(ApplyFilterCommandPropertyKey, value); } }
		private void ApplyFilter(object filterInput){ApplyFilter(filterInput as string);}
		private void ApplyFilter()
		{
			Suggestions = Selector.Select(_applyingFilterInput);
			FilterApplied();

			if (!ValidateSuggestionAgainstCurrentSuggestions(SuggestionPreview))
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
		private static readonly DependencyPropertyKey ConfirmCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("ConfirmCommand");
		public static DependencyProperty ConfirmCommandProperty { get { return ConfirmCommandPropertyKey.DependencyProperty; } }
		public Command ConfirmCommand { get { return (Command)GetValue(ConfirmCommandPropertyKey); } private set { SetValue(ConfirmCommandPropertyKey, value); } }
		private void Confirm(object filterInput) { Confirm(filterInput as string); }
		public virtual void Confirm(string filterInput)
		{
			DelayTimer.IsEnabled = false;

			if (SuggestionPreview == null || SuggestionPreview.Equals(EmptyValue))
			{
				if (IsEmptyValueAllowed && (SuggestionPreview == EmptyValue || SuggestionPreview.Equals(EmptyValue)) && String.IsNullOrWhiteSpace(filterInput))
				{
					Suggestion = SuggestionPreview;
				}
				else if (IsFreeTextAllowed && FreeTextToSuggestionConverter != null && (Suggestions == null || Suggestions.Count() == 0 || (Suggestions.Count() > 0 && String.Compare(filterInput, SuggestionToString(Suggestion),  StringComparison.CurrentCultureIgnoreCase) != 0)))
				{
					object suggestion;
					if ( FreeTextToSuggestionConverter(filterInput, out suggestion))
						Suggestion = SuggestionPreview = suggestion;
					else
						Cancel();
				}
				else
				{
					Cancel();
				}
			}
			else
			{
				if (ValidateSuggestion(SuggestionPreview))
					Suggestion = SuggestionPreview;
				else
					Cancel();
			}

			OnConfirmed(filterInput);
		}

		public event Action<object> Confirmed = (object filterInput) => { };
		protected virtual void OnConfirmed(object filterInput)
		{
			Confirmed(filterInput);
		}
		#endregion

		#region RefreshCommand - Manual notification for property changes in Suggestion or SuggestionPreview (must happen after suggestion edit).
		private static readonly DependencyPropertyKey RefreshCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("RefreshCommand");
		public static DependencyProperty RefreshCommandProperty { get { return RefreshCommandPropertyKey.DependencyProperty; } }
		public Command RefreshCommand { get { return (Command)GetValue(RefreshCommandPropertyKey); } private set { SetValue(RefreshCommandPropertyKey, value); } }
		public void Refresh() { Refresh(null); }
		private void Refresh(object parameter)
		{
			DelayTimer.IsEnabled = false;

			if (Refreshed != null) Refreshed();
		}
		public event Action Refreshed;
		#endregion

		public event Action FilterApplied = () => { };

		protected object OnPropertyChanging(DependencyProperty property, object value)
		{
			if (property == SuggestionProperty)
			{
				if (Equals(value, null) || string.Empty.Equals(value)) value = EmptyValue;

				if (IsFreeTextAllowed)
				{
					if (IsEmptyValueAllowed)
					{ }
					else
						if (Equals(value, EmptyValue))
							value = Suggestion;
				}
				else
				{
					if (IsEmptyValueAllowed)
					{
						if (!Equals(value, EmptyValue) && !ValidateSuggestion(value))
							value = Suggestion;
					}
					else
					{
						if (Equals(value, EmptyValue) || !ValidateSuggestion(value))
							value = Suggestion;
					}
				}
			}
			else if (property == SuggestionPreviewProperty)
			{
				if (Equals(value, null) || string.Empty.Equals(value)) value = EmptyValue;
			}
			return value;
		}

		protected bool ValidateSuggestionAgainstCurrentSuggestions(object suggestion)
		{
			if (Equals(suggestion, Suggestion)) return true;
			if (IsEmptyValueAllowed && Equals(suggestion, EmptyValue)) return true;
			if (Equals(suggestion, null)) return false;

			if (Suggestions != null)
			{
				foreach (var next in Suggestions)
					if (Equals(next, suggestion))
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

			private readonly IEnumerable _suggestionsSource;
			private readonly IValueConverter _suggestionToStringConverter;

			private string SuggestionToString(object suggestion)
			{
				return _suggestionToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture) + "";
			}

			public IEnumerable Select(object filterInput)
			{
				var filterInputAsString = filterInput as string ?? String.Empty;
				Type t = ReflectionHelper.GetEnumerableGenericType(_suggestionsSource.GetType());

				IList l = ReflectionHelper.CreateListInstanceWithT(t);
				foreach (object suggestion in _suggestionsSource)
					if(SuggestionToString(suggestion).StartsWith(filterInputAsString, StringComparison.CurrentCultureIgnoreCase))
						l.Add(suggestion);
				
				return l;
			}
		}
		#endregion

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == SuggestionPreviewProperty || e.Property == SuggestionProperty)
			{
				if (e.Property == SuggestionProperty)
					Cancel();

				IsConfirmed = Equals(Suggestion, SuggestionPreview);
			}

			base.OnPropertyChanged(e);
		}
		private bool ValidateSuggestion(object suggestion)
		{
			if (!ValidateSuggestionAgainstCurrentSuggestions(suggestion))
			{
				var suggestions = Selector.Select(SuggestionToString(suggestion));
				if (suggestions != null)
					foreach (var next in suggestions)
						if (Equals(next, suggestion))
							return true;
			}
			else
			{
				return true;
			}
			return false;
		}
	}
}
