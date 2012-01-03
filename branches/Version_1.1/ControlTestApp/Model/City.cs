using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlTestApp.Model
{
	public class City : BaseEntity
	{
		private string name = "";
		private Country country = null;

		public string Name { get { return name; } set { if (name != value) { name = value; OnPropertyChanged("Name"); } } }
		public Country Country { get { return country; } set { if (country != value) { country = value; OnPropertyChanged("Country"); } } }

		public bool IsValid
		{
			get
			{
				return Key > 0 && Country != null && !String.IsNullOrWhiteSpace(Name);
			}
		}
	}
}
