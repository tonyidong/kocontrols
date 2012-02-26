using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KOControls.GUI;
using KOControls.Core;
using System.Windows.Data;
using KOControls.Samples.Core.Model;
using System.Collections;
using System.Globalization;

namespace ControlTestApp
{
	public class AutoSuggestConsumerViewModel : AutoSuggestConsumerViewModelBase
	{
		#region IsFilterByCountryCity
		public static readonly DependencyProperty IsFilterByCountryCityProperty = ViewModel.RegisterProperty<bool, AutoSuggestConsumerViewModel>("IsFilterByCountryCity",false,
			(d, e) =>
			{
				var autoSuggestConsumerVM = d as AutoSuggestConsumerViewModelBase;
				if(autoSuggestConsumerVM != null)
				{
					if (true.Equals(e.NewValue)) autoSuggestConsumerVM.AutoSuggestVM.Selector = new CityNameCountrySelector(CitySuggestionToStringValueConverter, autoSuggestConsumerVM.AllCities);
					else autoSuggestConsumerVM.AutoSuggestVM.Selector = new AutoSuggestViewModel.DefaultSelector(CitySuggestionToStringValueConverter, autoSuggestConsumerVM.AllCities);
				}
			});
		public bool IsFilterByCountryCity { get { return (bool)GetValue(IsFilterByCountryCityProperty); } set { SetValue(IsFilterByCountryCityProperty, value); } }
		#endregion

		public AutoSuggestConsumerViewModel()
		{
			AutoSuggestVM.Suggestion = AllCities[1];
		}

		public class CityNameCountrySelector : ISelector
		{
			public CityNameCountrySelector(IValueConverter suggestionToStringConverter, IEnumerable<City> suggestionsSource)
			{
				_suggestionToStringConverter = suggestionToStringConverter;
				_suggestionsSource = suggestionsSource;
			}

			private readonly IEnumerable<City> _suggestionsSource;
			private readonly IValueConverter _suggestionToStringConverter;

			private string SuggestionToString(object suggestion)
			{
				return _suggestionToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture) + "";
			}

			public IEnumerable Select(object filterInput)
			{
				string filterInputAsString = filterInput as string ?? String.Empty;
				string [] cityCountryNames = filterInputAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				string cityFilter = cityCountryNames.Length > 0?cityCountryNames[0].Trim():String.Empty;
				string countryFilter = cityCountryNames.Length > 1?cityCountryNames[1].Trim():String.Empty;

				List<City> l = new List<City>();
				foreach (City suggestion in _suggestionsSource)
					if(suggestion.Name.StartsWith(cityFilter, StringComparison.CurrentCultureIgnoreCase) && suggestion.Country.Name.StartsWith(countryFilter, StringComparison.CurrentCultureIgnoreCase))
						l.Add(suggestion);
				return l;
			}
		}
	}
}
