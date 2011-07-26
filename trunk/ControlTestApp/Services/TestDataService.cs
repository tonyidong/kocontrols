using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ControlTestApp.Model;

namespace ControlTestApp.Services
{
	public class TestDataService
	{
		private static Country Bulgaria = new Country() { Key = 1, Name = "Bulgaria" };
		private static Country USA = new Country() { Key = 2, Name = "USA" };
		private static Country UK = new Country() { Key = 3, Name = "UK" };

		public static IList<Country> GetCountries()
		{
			List<Country> countries = new List<Country>();
			countries.Add(Bulgaria);
			countries.Add(USA);
			countries.Add(UK);

			return countries;
		}

		public static IList<City> GetCities()
		{
			List<City> cities = new List<City>();
			cities.Add(new City() { Key = 1, Country = Bulgaria, Name="Kostenec" });
			cities.Add(new City() { Key = 2, Country = Bulgaria, Name = "Kostin Brod" });
			cities.Add(new City() { Key = 3, Country = Bulgaria, Name = "Kotel" });
			cities.Add(new City() { Key = 4, Country = Bulgaria, Name = "Kustendil" });

			cities.Add(new City() { Key = 5, Country = USA, Name = "New Yrok" });
			cities.Add(new City() { Key = 6, Country = USA, Name = "New Jersey" });
			cities.Add(new City() { Key = 7, Country = USA, Name = "New Orleans" });
			cities.Add(new City() { Key = 8, Country = USA, Name = "San Diego" });
			cities.Add(new City() { Key = 9, Country = USA, Name = "San Jose" });
			cities.Add(new City() { Key = 10, Country = USA, Name = "Chicago" });

			return cities;
		}
	}
}
