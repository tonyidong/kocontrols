using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KOControls.Samples.Core.Model;

namespace KOControls.Samples.Core.Services
{
	public class TestDataService
	{
		private static Country Bulgaria = new Country() { Key = Country.GetNextKey(), Name = "Bulgaria" };
		private static Country USA = new Country() { Key = Country.GetNextKey(), Name = "USA" };
		private static Country UK = new Country() { Key = Country.GetNextKey(), Name = "UK" };

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
			cities.Add(new City() { Key = City.GetNextKey(), Country = Bulgaria, Name = "Kostenec" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = Bulgaria, Name = "Kostinbrod" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = Bulgaria, Name = "Kotel" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = Bulgaria, Name = "Kustendil" });

			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "New York" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "New Jersey" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "New Orleans" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "San Diego" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "San Jose" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "Chicago" });

			return cities;
		}
	}
}
