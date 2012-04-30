using System.Linq;
using System.Windows;
using KOControls.Samples.Core.Model;
using System.Collections.ObjectModel;
using KOControls.Core;
using KOControls.GUI;

namespace ControlTestApp
{
	public class AutoSuggestConsumerViewModelDataGrid : AutoSuggestConsumerViewModelBase
	{
		public AutoSuggestConsumerViewModelDataGrid()
		{			
			CityCountries = new ObservableCollection<CityCountry>();
			AutoSuggestVM.IsEmptyValueAllowed = true;
			AutoSuggestVM.AddValueChanged(AutoSuggestViewModel.SuggestionProperty, delegate
			{
			    if (SelectedCityCountry == null ||_selectedCityChanging) return;

			    if (AutoSuggestVM.Suggestion == null)
			    {
			        SelectedCityCountry.CityKey = -1;
			        SelectedCityCountry.CityName = "";
			        SelectedCityCountry.CountryKey = -1;
			        SelectedCityCountry.CountryName = "";
			    }
			    else
			    {
			        var city = (City)AutoSuggestVM.Suggestion;

			        SelectedCityCountry.CityKey = city.Key;
			        SelectedCityCountry.CityName = city.Name;
			        SelectedCityCountry.CountryKey = city.Country.Key;
			        SelectedCityCountry.CountryName = city.Country.Name;
			    }
			});
		}

		public ObservableCollection<CityCountry> CityCountries { get; private set; }
		private bool _selectedCityChanging;

		#region Selected CityCountry
		public static readonly DependencyProperty SelectedCityCountryProperty = ViewModel.RegisterProperty<CityCountry, AutoSuggestConsumerViewModelDataGrid>("SelectedCityCountry", null,
			(d, e) =>
			{
				if (e.NewValue != null)
				{
					var cityCountry = (CityCountry)e.NewValue;
					var countryCityVM = (AutoSuggestConsumerViewModelDataGrid)d;
					countryCityVM._selectedCityChanging = true;
					countryCityVM.AutoSuggestVM.Suggestion = countryCityVM.AllCities.FirstOrDefault(x => x.Key == cityCountry.CityKey);
					countryCityVM._selectedCityChanging = false;
				}
			});
		public CityCountry SelectedCityCountry { get { return (CityCountry)GetValue(SelectedCityCountryProperty); } set { SetValue(SelectedCityCountryProperty, value); } }
		#endregion
	}
}
