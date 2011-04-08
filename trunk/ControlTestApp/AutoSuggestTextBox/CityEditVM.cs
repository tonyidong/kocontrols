using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using ControlTestApp.Model;
using ControlTestApp.Services;

namespace ControlTestApp.AutoSuggestTextBox
{
	public class CityEditVM : DependencyObject
	{
		public event EventHandler CitySaved;

		public static readonly DependencyProperty CountriesProperty =
			 DependencyProperty.Register("Countries", typeof(ObservableCollection<Country>),
			 typeof(CityEditVM), new FrameworkPropertyMetadata());

		public ObservableCollection<Country> Countries
		{
			get { return (ObservableCollection<Country>)GetValue(CountriesProperty); }
			set { SetValue(CountriesProperty, value); }
		}

		public static readonly DependencyProperty CityProperty =
			 DependencyProperty.Register("City", typeof(City),
			 typeof(CityEditVM), new FrameworkPropertyMetadata());

		public City City
		{
			get { return (City)GetValue(CityProperty); }
			set { SetValue(CityProperty, value); }
		}

		public CityEditVM(City city)
		{
			City = city;

			Countries = new ObservableCollection<Country>(TestDataService.GetCountries());
		}

		internal void SaveCity()
		{
			if (CitySaved != null)
				CitySaved(this, EventArgs.Empty);
		}
	}
}
