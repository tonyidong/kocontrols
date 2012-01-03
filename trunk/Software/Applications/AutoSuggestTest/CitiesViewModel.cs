using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KOControls.Samples.Core.Model;
using System.Collections.ObjectModel;
using System.Windows.Data;
using KOControls.GUI.Core;
using KOControls.Core;
using KOControls.GUI;

namespace ControlTestApp
{
	public class CitiesViewModel : AutoSuggestConsumerViewModelBase
	{
		public ObservableCollection<CityCountry> CityCountries { get; private set; }
		private bool SelectedCityChanging = false;

		#region Selected CityCountry
		public static readonly DependencyProperty SelectedCityCountryProperty = ViewModel.RegisterProperty<CityCountry, CitiesViewModel>("SelectedCityCountry", null,
			(d, e) =>
			{
				if (e.NewValue != null)
				{
					CityCountry cityCountry = (CityCountry)e.NewValue;
					CitiesViewModel countryCityVM = (CitiesViewModel)d;
					countryCityVM.SelectedCityChanging = true;
					countryCityVM.AutoSuggestVM.Suggestion = countryCityVM.AllCities.FirstOrDefault(x => x.Key == cityCountry.CityKey);
					countryCityVM.SelectedCityChanging = false;
				}
			});
		public CityCountry SelectedCityCountry { get { return (CityCountry)GetValue(SelectedCityCountryProperty); } set { SetValue(SelectedCityCountryProperty, value); } }
		#endregion

		public CitiesViewModel()
		{			
			CityCountries = new ObservableCollection<CityCountry>();

			AutoSuggestVM.AddValueChanged(AutoSuggestViewModel.SuggestionProperty, delegate
			{
				if (SelectedCityCountry != null && !SelectedCityChanging)
				{
					if (AutoSuggestVM.Suggestion == null)
					{
						SelectedCityCountry.CityKey = -1;
						SelectedCityCountry.CityName = "";
						SelectedCityCountry.CountryKey = -1;
						SelectedCityCountry.CountryName = "";
					}
					else
					{
						City city = (City)AutoSuggestVM.Suggestion;

						SelectedCityCountry.CityKey = city.Key;
						SelectedCityCountry.CityName = city.Name;
						SelectedCityCountry.CountryKey = city.Country.Key;
						SelectedCityCountry.CountryName = city.Country.Name;
					}
				}
			});
		}
	}
}
