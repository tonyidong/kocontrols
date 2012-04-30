using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KOControls.Core;
using System.Windows.Data;
using KOControls.Samples.Core.Model;
using System.Collections;
using System.Globalization;
using KOControls.GUI;

namespace ControlTestApp
{
	public static class SelectorFactory
	{
		public static ISelector GetCitynameCountrySelector(IValueConverter suggestionToStringConverter, IEnumerable<City> suggestionsSource)
		{
			return new CityNameCountrySelector(suggestionToStringConverter, suggestionsSource);
		}

		public static ISelector GetDefaultSelector(IValueConverter suggestionToStringConverter, IEnumerable suggestionsSource)
		{
			return new AutoSuggestViewModel.DefaultSelector(suggestionToStringConverter, suggestionsSource);
		}

		public static ISelector GetCountySelector(IValueConverter suggestionToStringConverter, IEnumerable<County> suggestionsSource)
		{
			return new CountySelector(suggestionToStringConverter, suggestionsSource);
		}
	}

	public class CountySelector : ISelector
	{
		public CountySelector(IValueConverter suggestionToStringConverter, IEnumerable<County> suggestionsSource)
		{
			_suggestionToStringConverter = suggestionToStringConverter;
			_suggestionsSource = suggestionsSource;
		}

		private readonly IEnumerable<County> _suggestionsSource;
		private readonly IValueConverter _suggestionToStringConverter;

		private string SuggestionToString(object suggestion)
		{
			return _suggestionToStringConverter.Convert(suggestion, typeof(string), null, CultureInfo.CurrentCulture) + "";
		}

		public IEnumerable Select(object filterInput)
		{
			string filterInputAsString = filterInput as string ?? String.Empty;
			
			var l = new List<County>();
			foreach (County suggestion in _suggestionsSource)
				if (suggestion.CountyName.StartsWith(filterInputAsString, StringComparison.CurrentCultureIgnoreCase)
						 || suggestion.Name.StartsWith(filterInputAsString, StringComparison.CurrentCultureIgnoreCase)
						 || suggestion.Name.StartsWith(suggestion.State + " State " + filterInputAsString, StringComparison.CurrentCultureIgnoreCase))
					l.Add(suggestion);
			return l;
		}
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
			string[] cityCountryNames = filterInputAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			string cityFilter = cityCountryNames.Length > 0 ? cityCountryNames[0].Trim() : String.Empty;
			string countryFilter = cityCountryNames.Length > 1 ? cityCountryNames[1].Trim() : String.Empty;

			List<City> l = new List<City>();
			foreach (City suggestion in _suggestionsSource)
				if (suggestion.Name.StartsWith(cityFilter, StringComparison.CurrentCultureIgnoreCase) && suggestion.Country.Name.StartsWith(countryFilter, StringComparison.CurrentCultureIgnoreCase))
					l.Add(suggestion);
			return l;
		}
	}
}
