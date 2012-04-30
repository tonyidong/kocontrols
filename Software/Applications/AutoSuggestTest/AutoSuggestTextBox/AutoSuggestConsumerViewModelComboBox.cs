using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using KOControls.GUI.Core;
using KOControls.Samples.Core.Model;
using KOControls.Core;
using KOControls.Samples.Core.Services;

namespace ControlTestApp
{
	public class AutoSuggestConsumerViewModelComboBox : DependencyObject
	{
		public AutoSuggestConsumerViewModelComboBox()
		{
			AllCountries = TestDataService.GetCountries();
			AllCities = TestDataService.GetCities();

			SelectedCountry = AllCountries[0];
			SetCurrentValue(SelectedCountryProperty, SelectedCountry);
			SetCurrentValue(SelectedCountryValueProperty, SelectedCountry.Key);

			SelectedCountry = AllCountries[1];

			CitySuggestionsFilter = null;
			SetCitySuggestionsFilterCommand = new Command(x => SetCitySuggestionsFilter());
			ClearCitySuggestionsFilterCommand = new Command(x => ClearCitySuggestionsFilter());
		}

		#region AllCities
		private static readonly DependencyPropertyKey AllCitiesPropertyKey = ViewModel.RegisterReadOnlyProperty<IList<City>, AutoSuggestConsumerViewModelComboBox>("AllCities");
		public static DependencyProperty AllCitiesProperty { get { return AllCitiesPropertyKey.DependencyProperty; } }
		public IList<City> AllCities { get { return (IList<City>)GetValue(AllCitiesProperty); } private set { SetValue(AllCitiesPropertyKey, value); } }
		#endregion

		#region Cities
		public static readonly DependencyProperty CitiesProperty = ViewModel.RegisterProperty<IList<City>, AutoSuggestConsumerViewModelComboBox>("Cities");
		public IList<City> Cities { get { return (IList<City>)GetValue(CitiesProperty); } set { SetValue(CitiesProperty, value); } }
		#endregion

		#region SelectedCity
		public static readonly DependencyProperty SelectedCityProperty = ViewModel.RegisterProperty<City, AutoSuggestConsumerViewModelComboBox>("SelectedCity", null, (d, a) =>
		{
		   
		});
		public City SelectedCity { get { return (City)GetValue(SelectedCityProperty); } set { SetValue(SelectedCityProperty, value); } }
		#endregion

		#region SelectedCityValue
		public static readonly DependencyProperty SelectedCityValueProperty = ViewModel.RegisterProperty<long, AutoSuggestConsumerViewModelComboBox>("SelectedCityValue", -1, (d, a) =>
			{
			});
		public long SelectedCityValue { get { return (long)GetValue(SelectedCityValueProperty); } set { SetValue(SelectedCityValueProperty, value); } }
		#endregion

		#region CitySuggestionsFilter
		private static readonly DependencyPropertyKey CitySuggestionsFilterPropertyKey = ViewModel.RegisterReadOnlyProperty<Func<object, string, bool>, AutoSuggestConsumerViewModelComboBox>("CitySuggestionsFilter");
		public static DependencyProperty CitySuggestionsFilterProperty { get { return CitySuggestionsFilterPropertyKey.DependencyProperty; } }
		public Func<object, string, bool> CitySuggestionsFilter { get { return (Func<object, string, bool>)GetValue(CitySuggestionsFilterProperty); } private set { SetValue(CitySuggestionsFilterPropertyKey, value); } }
		#endregion

		#region SetCitySuggestionsFilterCommand
		private static readonly DependencyPropertyKey SetCitySuggestionsFilterCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<ICommand, AutoSuggestConsumerViewModelComboBox>("SetCitySuggestionsFilterCommand");
		public static DependencyProperty SetCitySuggestionsFilterCommandProperty { get { return SetCitySuggestionsFilterCommandPropertyKey.DependencyProperty; } }
		public ICommand SetCitySuggestionsFilterCommand { get { return (ICommand)GetValue(SetCitySuggestionsFilterCommandProperty); } private set { SetValue(SetCitySuggestionsFilterCommandPropertyKey, value); } }

		private void SetCitySuggestionsFilter()
		{
			CitySuggestionsFilter = (potentialSuggestion, filter) =>
			{
				if (String.IsNullOrEmpty(filter)) return true;

				var city = potentialSuggestion as City;
				if (city == null) return false;

				if (city.Name.StartsWith(filter, true, CultureInfo.CurrentCulture)) return true;

				var tokens = city.Name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string token in tokens)
					if (token.StartsWith(filter, true, CultureInfo.CurrentCulture)) return true;
				return false;
			};
			CityIsAutoCompleteOn = false;
		}
		#endregion

		#region ClearCitySuggestionsFilterCommand
		private static readonly DependencyPropertyKey ClearCitySuggestionsFilterCommandPropertyKey = ViewModel.RegisterReadOnlyProperty<ICommand, AutoSuggestConsumerViewModelComboBox>("ClearCitySuggestionsFilterCommand");
		public static DependencyProperty ClearCitySuggestionsFilterCommandProperty { get { return ClearCitySuggestionsFilterCommandPropertyKey.DependencyProperty; } }
		public ICommand ClearCitySuggestionsFilterCommand { get { return (ICommand)GetValue(ClearCitySuggestionsFilterCommandProperty); } private set { SetValue(ClearCitySuggestionsFilterCommandPropertyKey, value); } }

		private void ClearCitySuggestionsFilter()
		{
			CitySuggestionsFilter = null;
			CityIsAutoCompleteOn = true;
		}
		#endregion

		#region CityIsAutoCompleteOn
		public static readonly DependencyProperty CityIsAutoCompleteOnProperty = ViewModel.RegisterProperty<bool, AutoSuggestConsumerViewModelComboBox>("CityIsAutoCompleteOn", true);
		public bool CityIsAutoCompleteOn { get { return (bool)GetValue(CityIsAutoCompleteOnProperty); } set { SetValue(CityIsAutoCompleteOnProperty, value); } }
		#endregion

		#region AllCountries
		private static readonly DependencyPropertyKey AllCountriesPropertyKey = ViewModel.RegisterReadOnlyProperty<IList<Country>, AutoSuggestConsumerViewModelComboBox>("AllCountries");
		public static DependencyProperty AllCountriesProperty { get { return AllCountriesPropertyKey.DependencyProperty; } }
		public IList<Country> AllCountries { get { return (IList<Country>)GetValue(AllCountriesProperty); } private set { SetValue(AllCountriesPropertyKey, value); } }
		#endregion

		#region SelectedCountry
		public static readonly DependencyProperty SelectedCountryProperty = ViewModel.RegisterProperty<Country, AutoSuggestConsumerViewModelComboBox>("SelectedCountry", null, (d, a) =>
		{
			var vm = (AutoSuggestConsumerViewModelComboBox)d;
			var selectedCountry = a.NewValue as Country;
			if (vm == null) return;

			if (selectedCountry != null)
			{
				vm.Cities = vm.AllCities.Where(x => x.Country.Key == selectedCountry.Key).ToList();
				vm.SelectedCity = vm.Cities.First();
			}
			else
				vm.Cities = vm.AllCities;
		});
		public Country SelectedCountry { get { return (Country)GetValue(SelectedCountryProperty); } set { SetValue(SelectedCountryProperty, value); } }
		#endregion

		#region SelectedCountryValue
		public static readonly DependencyProperty SelectedCountryValueProperty = ViewModel.RegisterProperty<long, AutoSuggestConsumerViewModelComboBox>("SelectedCountryValue", -1, (d, a) =>
		{
		});
		public long SelectedCountryValue { get { return (long)GetValue(SelectedCountryValueProperty); } set { SetValue(SelectedCountryValueProperty, value); } }
		#endregion
	}
}
