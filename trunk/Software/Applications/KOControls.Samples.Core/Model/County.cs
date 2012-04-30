using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KOControls.Samples.Core.Model
{
	public class County : BaseEntity
	{
		public string Name { get { return State + " State " + countyName + " County " + Code ; } }

		private string state = "";
		public string State { get { return state; } set { if (state != value) { state = value; OnPropertyChanged("State"); } } }

		private string countyName = "";
		public string CountyName { get { return countyName; } set { if (countyName != value) { countyName = value; OnPropertyChanged("CountyName"); } } }

		private string code = "";
		public string Code { get { return code; } set { if (code != value) { code = value; OnPropertyChanged("Code"); } } }

		public static int GetNextKey()
		{
			return GetNextKey("County");
		}
	}
}
