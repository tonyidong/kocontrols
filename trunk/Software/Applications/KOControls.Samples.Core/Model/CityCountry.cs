using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KOControls.Samples.Core.Model
{
	public class CityCountry : BaseEntity
	{
		private long cityKey = -1;
		private string cityName = "";
		private long countryKey = -1;
		private string countryName = "";

		public long CityKey { get { return cityKey; } set { if (cityKey != value) { cityKey = value; OnPropertyChanged("CityKey"); } } }
		public string CityName { get { return cityName; } set { if (cityName != value) { cityName = value; OnPropertyChanged("CityName"); } } }
		public long CountryKey { get { return countryKey; } set { if (countryKey != value) { countryKey = value; OnPropertyChanged("CountryKey"); } } }
		public string CountryName { get { return countryName; } set { if (countryName != value) { countryName = value; OnPropertyChanged("CountryName"); } } }

		public static int GetNextKey()
		{
			return GetNextKey("CityCountry");
		}
	}
}
