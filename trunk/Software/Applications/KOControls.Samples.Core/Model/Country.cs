using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KOControls.Samples.Core.Model
{
	public class Country : BaseEntity
	{
		private string name = "";
		public string Name { get { return name; } set { if (name != value) { name = value; OnPropertyChanged("Name"); } } }

		public static int GetNextKey()
		{
			return GetNextKey("Country");
		}
	}
}
