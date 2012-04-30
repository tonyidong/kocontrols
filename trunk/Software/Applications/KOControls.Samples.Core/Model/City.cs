using System;
using KOControls.Core;

namespace KOControls.Samples.Core.Model
{
	public class City : BaseEntity
	{
		public string Name { get { return name; } set { if(name != value) { name = value; OnPropertyChanged("Name"); } } }
		private string name = "";

		public Country Country { get { return country; } set { if(country != value) { country = value; OnPropertyChanged("Country"); } } }
		private Country country = null;

		public bool IsValid	{get	{return Key > 0 && Country != null && !Name.IsNullOrWhiteSpace();}	}

		public static int GetNextKey()
		{
			return GetNextKey("City");
		}
	}
}
