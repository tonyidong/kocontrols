using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using KOControls.Samples.Core.Model;
using KOControls.Samples.Core.Services;

namespace KOControls.Samples.Core
{
	public class CityEditVM : DependencyObject
	{
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
	}
}
