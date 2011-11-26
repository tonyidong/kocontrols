using System;
using System.Collections.Generic;
using System.ComponentModel;
using KOControls.Core;

namespace KOControls.Samples.Core.Model
{
	public class BaseEntity : ViewModel, INotifyPropertyChanged
	{
		private static IDictionary<string, int> Keys = new Dictionary<string, int>();

		protected static int GetNextKey(string entityName)
		{
			if(Keys.ContainsKey(entityName))
				Keys[entityName]++;
			else
				Keys.Add(entityName, 1);

			return Keys[entityName];
		}

		private long key = -1;

		public long Key { get { return key; } set { if(key != value) { key = value; OnPropertyChanged("Key"); } } }

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
			if(PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
