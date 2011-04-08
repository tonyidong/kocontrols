
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ControlTestApp.Model
{
	public class BaseEntity : INotifyPropertyChanged
	{
		private long key = -1;

		public long Key { get { return key; } set { if (key != value) { key = value; OnPropertyChanged("Key"); } } }

		#region INotifyPropertyChanged Members

		/// <summary>
		/// INotifyPropertyChanged requires a property called PropertyChanged.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the event for the property when it changes.
		/// </summary>
		public virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
