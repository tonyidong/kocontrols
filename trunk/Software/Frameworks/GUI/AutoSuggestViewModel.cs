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
	public class AutoSuggestViewModel : AutoSuggestBaseViewModel
	{
		public AutoSuggestViewModel()
			: base()
		{
			CancelCommand = new Command(Cancel);
			ClearCommand = new Command(Clear);
		}

		public AutoSuggestViewModel(ISelector selector, IValueConverter suggestionToStringConverter = null, IValueConverter suggestionPreviewToStringConverter = null, object styleModel = null)
			: base(selector,suggestionPreviewToStringConverter,styleModel)
		{
			SuggestionToStringConverter = suggestionToStringConverter;
			CancelCommand = new Command(Cancel);
			ClearCommand = new Command(Clear);
		}

		#region Suggestion
		public static readonly DependencyProperty SuggestionProperty = ViewModel.RegisterProperty<object, AutoSuggestViewModel>("Suggestion", null, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionProperty, v));
		public object Suggestion { get { return GetValue(SuggestionProperty); } set { SetValue(SuggestionProperty, value); } }
		#endregion

		#region SuggestionToStringConverter
		public static readonly DependencyProperty SuggestionToStringConverterProperty = ViewModel.RegisterProperty<IValueConverter, AutoSuggestViewModel>("SuggestionToStringConverter", DefaultSuggestionToStringConverter.Instance, null, (d, v) => ((AutoSuggestViewModel)d).OnPropertyChanging(SuggestionToStringConverterProperty, v));
		public IValueConverter SuggestionToStringConverter { get { return (IValueConverter)GetValue(SuggestionToStringConverterProperty); } set { SetValue(SuggestionToStringConverterProperty, value); } }
		public string SuggestionToString(object suggestion) { return (string)SuggestionToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture); }
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

		#region Confirm
		public override void Confirm(string filterInput)
		{
			if (filterInput == null || SuggestionPreviewToString(SuggestionPreview) == filterInput)
			{
				if (ValidateSuggestion(SuggestionPreview))
					Suggestion = SuggestionPreview;
				else
					Cancel();
			}
			else
			{
				if (IsFreeTextAllowed && FreeTextToSuggestionConverter != null)
				{
					object suggestion;
					var conversionResult = FreeTextToSuggestionConverter(filterInput, out suggestion);
					if (conversionResult)
						Suggestion = SuggestionPreview = suggestion;
					else
						Cancel();
				}
				else
					Cancel();
			}
		}
		#endregion

		protected override object OnPropertyChanging(DependencyProperty property, object value)
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
			return base.OnPropertyChanging(property, value);
		}
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
		protected override bool ValidateSuggestionAgainsCurrentSuggestions(object suggestion)
		{
			if(Equals(suggestion, Suggestion)) return true;
			return base.ValidateSuggestionAgainsCurrentSuggestions(suggestion);
		}
		private bool ValidateSuggestion(object suggestion)
		{
			if (!ValidateSuggestionAgainsCurrentSuggestions(suggestion))
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
