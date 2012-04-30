using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using KOControls.Core;
using KOControls.GUI.Core;
using System.ComponentModel;

namespace KOControls.GUI
{
	using __type=AutoSuggestViewModel;
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
			CancelCommand = new Command(x => Cancel());
			ClearCommand = new Command(x => Clear());

			Suggestions = new SelectedSuggestionsObservableCollection();
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
		public static readonly DependencyProperty SuggestionProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("Suggestion", null, (d, v) => ((AutoSuggestViewModel)d).AddSuggestionToSuggestions(v.NewValue), (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionProperty, v));
		public object Suggestion { get { return GetValue(SuggestionProperty); } set { SetValue(SuggestionProperty, value); } }
		#endregion
		#region Suggestions
		public static readonly DependencyProperty SuggestionsProperty = ViewModel.RegisterProperty<SelectedSuggestionsObservableCollection, AutoSuggestViewModel>("Suggestions", null);
		public SelectedSuggestionsObservableCollection Suggestions { get { return (SelectedSuggestionsObservableCollection)GetValue(SuggestionsProperty); } set { SetValue(SuggestionsProperty, value); } }
		private void AddSuggestionToSuggestions(object suggestion)
		{
			if(suggestion != null)
			{
				Suggestions.Clear();
				Suggestions.Add(suggestion);
			}
		}
		#endregion

		#region SuggestionPreview
		public static readonly DependencyProperty SuggestionPreviewProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("SuggestionPreview", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionPreviewProperty, v));
		public object SuggestionPreview { get { return GetValue(SuggestionPreviewProperty); } set { SetValue(SuggestionPreviewProperty, value); } }
		#endregion
		#region SuggestionPreviews
		private static readonly DependencyPropertyKey SuggestionPreviewsPropertyKey = ViewModel.RegisterReadOnlyProperty<IEnumerable, AutoSuggestViewModel>("SuggestionPreviews", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionPreviewsProperty, v));
		public static DependencyProperty SuggestionPreviewsProperty { get { return SuggestionPreviewsPropertyKey.DependencyProperty; } }
		public IEnumerable SuggestionPreviews { get { return (IEnumerable)GetValue(SuggestionPreviewsPropertyKey); } protected set { SetValue(SuggestionPreviewsPropertyKey, value); } }
		#endregion

		#region Filter
		public static readonly DependencyProperty SelectorProperty = ViewModel.RegisterProperty<ISelector, AutoSuggestViewModel>("Selector", null, (d, a) => ((__type)d).ApplyFilter(""), (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SelectorProperty, v));
		public ISelector Selector { get { return (ISelector)GetValue(SelectorProperty); } set { SetValue(SelectorProperty, value); } }
		#endregion

		#region CancelCommand
		private static readonly DependencyPropertyKey CancelCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("CancelCommand");
		public static DependencyProperty CancelCommandProperty { get { return CancelCommandPropertyKey.DependencyProperty; } }
		public Command CancelCommand { get { return (Command)GetValue(CancelCommandPropertyKey); } private set { SetValue(CancelCommandPropertyKey, value); } }
		public void Cancel()
		{
			DelayTimer.IsEnabled = false;

			SuggestionPreview = Suggestion;

			OnCancelled();
		}

		public event Action Cancelled = () => { };
		protected virtual void OnCancelled() { Cancelled(); }
		#endregion
		#region ClearCommand
		private static readonly DependencyPropertyKey ClearCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("ClearCommand");
		public static DependencyProperty ClearCommandProperty { get { return ClearCommandPropertyKey.DependencyProperty; } }
		public Command ClearCommand { get { return (Command)GetValue(ClearCommandPropertyKey); } private set { SetValue(ClearCommandPropertyKey, value); } }
		public void Clear()
		{
			DelayTimer.IsEnabled = false;

			SuggestionPreview = null;
			Suggestion = null;
		}
		#endregion

		#region IsConfirmed
		private static readonly DependencyPropertyKey IsConfirmedPropertyKey = RegisterReadOnlyProperty<bool, AutoSuggestViewModel>("IsConfirmed");
		public static DependencyProperty IsConfirmedProperty { get { return IsConfirmedPropertyKey.DependencyProperty; } }
		public bool IsConfirmed { get { return (bool)GetValue(IsConfirmedPropertyKey); } private set { SetValue(IsConfirmedPropertyKey, value); } }
		#endregion


		#region Delay
		public static readonly DependencyProperty DelayProperty = ViewModel.RegisterProperty<TimeSpan, AutoSuggestViewModel>("Delay", new TimeSpan(0), DelayValueChanged, null);
		public TimeSpan Delay { get { return (TimeSpan)GetValue(DelayProperty); } set { SetValue(DelayProperty, value); } }

		protected DispatcherTimer DelayTimer;

		private static void DelayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var asVM = d as AutoSuggestViewModel;
			if(asVM == null) return;

			var ts = (TimeSpan)e.NewValue;

			if(ts.TotalMilliseconds == 0)
				asVM.DelayTimer.IsEnabled = false;
			asVM.DelayTimer.Interval = ts;
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
		public static readonly DependencyProperty EmptyValueProperty = RegisterProperty<object, AutoSuggestViewModel>("EmptyValue", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(EmptyValueProperty, v));
		public object EmptyValue { get { return (object)GetValue(EmptyValueProperty); } set { SetValue(EmptyValueProperty, value); } }
		#endregion

		#region ApplyFilterCommand
		//You may bind the command to a search button next to the textbox. This can be used in combination with the ApplyFilterTrigger Option.
		private static readonly DependencyPropertyKey ApplyFilterCommandPropertyKey = RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("ApplyFilterCommand");
		public static DependencyProperty ApplyFilterCommandProperty { get { return ApplyFilterCommandPropertyKey.DependencyProperty; } }
		public Command ApplyFilterCommand { get { return (Command)GetValue(ApplyFilterCommandPropertyKey); } private set { SetValue(ApplyFilterCommandPropertyKey, value); } }
		private void ApplyFilter(object filterInput) { ApplyFilter(filterInput as string); }
		private void ApplyFilter()
		{
			SuggestionPreviews = Selector.Select(_applyingFilterInput);
			//What to do with validating the SuggestionPreview. Should we possibly do it after Filter Applied?
			//SuggestionPreview = null;
			FilterApplied(_applyingFilterInput);
		}
		public event Action<string> FilterApplied = (y) => { };

		public void ApplyFilter(string filterInput)
		{
			_applyingFilterInput = filterInput;

			DelayTimer.IsEnabled = false;
			if(Delay.TotalMilliseconds == 0)
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

			if(SuggestionPreview == null || SuggestionPreview.Equals(EmptyValue))
			{
				if(IsEmptyValueAllowed && Equals(SuggestionPreview, EmptyValue) && filterInput.IsNullOrWhiteSpace())
				{
					Suggestion = EmptyValue;
				}
				else if(IsFreeTextAllowed && FreeTextToSuggestionConverter != null)
				{
					object suggestion;
					if(FreeTextToSuggestionConverter(filterInput, out suggestion))
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
				if (IsConfirmed) return;

				if(ValidateSuggestion(SuggestionPreview))
					Suggestion = SuggestionPreview;					
				else
					Cancel();
			}

			OnConfirmed(filterInput);
		}

		public event Action<object> Confirmed = filterInput => { };
		protected virtual void OnConfirmed(object filterInput) { Confirmed(filterInput); }
		#endregion

		#region RefreshCommand - Manual notification for property changes in Suggestion or SuggestionPreview (must happen after suggestion edit).
		private static readonly DependencyPropertyKey RefreshCommandPropertyKey = RegisterReadOnlyProperty<Command, AutoSuggestViewModel>("RefreshCommand");
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

		#region Separators
		public static readonly DependencyProperty SeparatorsProperty = ViewModel.RegisterProperty<IList<string>, __type>("Separators");
		public IList<string> Separators { get { return (IList<string>)GetValue(SeparatorsProperty); } set { SetValue(SeparatorsProperty, value); } }
		#endregion

		protected object OnPropertyChanging(DependencyProperty property, object value)
		{
			if(property == SuggestionProperty)
			{
				if(Equals(value, null) || string.Empty.Equals(value)) value = EmptyValue;

				if(IsFreeTextAllowed)
				{
					if(IsEmptyValueAllowed)
					{
					}
					else
					{
						if(Equals(value, EmptyValue))
							value = Suggestion;
					}
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

		#region Commands - optional
		private static readonly DependencyPropertyKey CommandsPropertyKey = ViewModel.RegisterReadOnlyProperty<ObservableCollection<CommandBase>, AutoSuggestViewModel>("Commands");
		public static DependencyProperty CommandsProperty { get { return CommandsPropertyKey.DependencyProperty; } }
		public ObservableCollection<CommandBase> Commands { get { return (ObservableCollection<CommandBase>)GetValue(CommandsPropertyKey); } private set { SetValue(CommandsPropertyKey, value); } }
		#endregion

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

		protected bool ValidateSuggestionAgainstCurrentSuggestions(object suggestion, IEnumerable suggestions)
		{
			if (Equals(suggestion, Suggestion)) return true;
			if (IsEmptyValueAllowed && Equals(suggestion, EmptyValue)) return true;
			if (Equals(suggestion, null)) return false;

			if (suggestions != null)
				foreach (var next in suggestions)
					if (Equals(next, suggestion))
						return true;

			return false;
		}
		private bool ValidateSuggestion(object suggestion)
		{
			if (ValidateSuggestionAgainstCurrentSuggestions(suggestion, SuggestionPreviews)) return true;

			var suggestions = Selector.Select(SuggestionToString(suggestion));
			return ValidateSuggestionAgainstCurrentSuggestions(suggestion, suggestions);
		}

		#region Nested classes
		public class DefaultSuggestionToStringConverter : IValueConverter
		{
			public static readonly DefaultSuggestionToStringConverter Instance = new DefaultSuggestionToStringConverter();

			private DefaultSuggestionToStringConverter() { }

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return value + string.Empty; }
			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return value; }
		}
		public class PropertyPathSuggestionToStringConverter : DependencyObject, IValueConverter
		{
			public PropertyPathSuggestionToStringConverter(string propertyPath) { _propertyPath = propertyPath; }
			private readonly string _propertyPath;

			#region Value
			public static readonly DependencyProperty ValueProperty = ViewModel.RegisterProperty<object, PropertyPathSuggestionToStringConverter>("Value");
			public object Value { get { return (object)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }
			#endregion

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if(value == null) return null;
				if(String.IsNullOrEmpty(_propertyPath)) return value.ToString();

				BindingOperations.SetBinding(this, ValueProperty, new Binding(_propertyPath) { Source = value, Mode = BindingMode.OneWay });
				var tmpValue = Value;
				BindingOperations.ClearBinding(this, ValueProperty);

				return tmpValue;
			}
			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				BindingOperations.SetBinding(this, ValueProperty, new Binding(_propertyPath) { Source = value, Mode = BindingMode.OneWayToSource });
				BindingOperations.ClearBinding(this, ValueProperty);

				return Value;
			}
		}

		public class DefaultSelector : ISelector
		{
			public DefaultSelector(Func<object, string, bool> suggestionFilter, IEnumerable suggestionsSource)
			{
				_suggestionFilter = suggestionFilter;
				_suggestionsSource = suggestionsSource;
				_suggestionType = ReflectionHelper.GetEnumerableGenericType(_suggestionsSource.GetType());
			}
			public DefaultSelector(IValueConverter suggestionToStringConverter, IEnumerable suggestionsSource)
			{
				_suggestionFilter = (potentialSuggestion, filterInput) => (suggestionToStringConverter.Convert(potentialSuggestion, typeof(string), null, CultureInfo.CurrentCulture) + "").StartsWith(filterInput, StringComparison.CurrentCultureIgnoreCase);
				_suggestionsSource = suggestionsSource;
				_suggestionType = ReflectionHelper.GetEnumerableGenericType(_suggestionsSource.GetType());
			}

			private readonly IEnumerable _suggestionsSource;
			private readonly Func<object, string, bool> _suggestionFilter;
			private readonly Type _suggestionType;

			public IEnumerable Select(object filterInput)
			{
				if(_suggestionsSource == null) return null;

				var filterInputAsString = filterInput as string ?? String.Empty;

				var l = ReflectionHelper.CreateListInstanceWithT(_suggestionType);
				foreach (object suggestion in _suggestionsSource)
					 if(_suggestionFilter(suggestion, filterInputAsString))
						l.Add(suggestion);
				
				return l;
			}
		}

		public class NoFilterDefaultSelector : ISelector
		{
			private readonly IEnumerable _suggestionsSource;
			public NoFilterDefaultSelector(IEnumerable suggestionsSource)
			{
				_suggestionsSource = suggestionsSource;
			}
			public IEnumerable Select(object filterInput) { return _suggestionsSource; }
		}

		public class SelectedSuggestionsObservableCollection : ObservableCollection<object>
		{
			#region RuntimeVersion
			public int RuntimeVersion
			{
				get { return _runtimeVersion; }
				private set
				{
					if(_runtimeVersion != value)
					{
						_runtimeVersion = value;
						OnPropertyChanged(new PropertyChangedEventArgs("RuntimeVersion"));
					}
				}
			}
			private int _runtimeVersion;
			#endregion

			protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				base.OnCollectionChanged(e);

				if(RuntimeVersion >= (Int32.MaxValue - 1))
					RuntimeVersion = 0;
				else
					++RuntimeVersion;
			}
		}
		#endregion
	}
}
