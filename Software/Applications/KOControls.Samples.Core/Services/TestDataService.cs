using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KOControls.Samples.Core.Model;

namespace KOControls.Samples.Core.Services
{
	public class TestDataService
	{
		#region Countries
		private static Country USA = new Country() { Key = Country.GetNextKey(), Name = "USA" };
		private static Country UK = new Country() { Key = Country.GetNextKey(), Name = "UK" };

		public static IList<Country> GetCountries()
		{
			var countries = new List<Country>();
			countries.Add(USA);
			countries.Add(UK);

			return countries;
		}
		#endregion 

		#region Cities
		private static IList<City> _allCities;
		public static IList<City> AllCities { get {if(_allCities == null) { _allCities = GetCities(); } return _allCities;}}

		public static List<City> GetCities()
		{
			var cities = new List<City>();
			cities.Add(new City() { Key = City.GetNextKey(), Country = UK, Name = "Canterbury" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = UK, Name = "Carlisle" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = UK, Name = "Cambridge" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = UK, Name = "Norwich" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = UK, Name = "Nottingham" });

			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "New York" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "New Jersey" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "New Orleans" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "San Diego" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "San Jose" });
			cities.Add(new City() { Key = City.GetNextKey(), Country = USA, Name = "Chicago" });

			return cities;
		}
		#endregion 

		#region Counties
		public static IList<County> GetCounties()
		{
			var counties = new List<County>();
			counties.Add(new County() { Code = "DMV", State = "California", CountyName = "Marin"});
			counties.Add(new County() { Code = "DEQ", State = "California", CountyName = "Marin"});
			counties.Add(new County() { Code = "PD", State = "California", CountyName = "Marin"});
			counties.Add(new County() { Code = "DMV", State = "California", CountyName = "Sonoma"});
			counties.Add(new County() { Code = "DEQ", State = "California", CountyName = "Sonoma"});
			counties.Add(new County() { Code = "PD", State = "California", CountyName = "Sonoma"});
			counties.Add(new County() { Code = "DMV", State = "California", CountyName = "Sacramento"});
			counties.Add(new County() { Code = "DEQ", State = "California", CountyName = "Sacramento"});
			counties.Add(new County() { Code = "PD", State = "California", CountyName = "Sacramento"});

			return counties;
		}
		#endregion 

	}
}
