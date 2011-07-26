using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using ControlTestApp.Model;
using ControlTestApp.Services;
using KO.Controls;
using KO.Controls.Common.Command;
using System.Windows.Data;
using System.Windows.Threading;

namespace ControlTestApp.AutoSuggestTextBox
{
	public class AutoSuggestConsumerViewModel : DependencyObject
	{
		#region IsInvokeNewAllowed
		public static readonly DependencyProperty IsInvokeNewAllowedProperty =
		 DependencyProperty.Register("IsInvokeNewAllowed", typeof(bool),
		 typeof(AutoSuggestConsumerViewModel), new PropertyMetadata());

		public bool IsInvokeNewAllowed
		{
			get { return (bool)GetValue(IsInvokeNewAllowedProperty); }
			set { SetValue(IsInvokeNewAllowedProperty, value); }
		}
		#endregion 

		#region IsInvokeEditAllowed
		public static readonly DependencyProperty IsInvokeEditAllowedProperty =
		 DependencyProperty.Register("IsInvokeEditAllowed", typeof(bool),
		 typeof(AutoSuggestConsumerViewModel), new PropertyMetadata());

		public bool IsInvokeEditAllowed
		{
			get { return (bool)GetValue(IsInvokeEditAllowedProperty); }
			set { SetValue(IsInvokeEditAllowedProperty, value); }
		}
		#endregion 

		public IList<City> AllCities { get; set; }
		public AutoSuggestViewModel AutoSuggestVM { get; private set; }

		public AutoSuggestConsumerViewModel()
		{
			AllCities = TestDataService.GetCities();

			AutoSuggestVM = new AutoSuggestViewModel((x,y) => { if (x == null)return ""; else return ((City)x).Name; });
			AutoSuggestVM.FilterItems = new RelayCommand((x) => { AutoSuggestVM.ItemsSource = AllCities.Where(y => y.Name.StartsWith(x.ToString(), StringComparison.CurrentCultureIgnoreCase)).ToList<City>(); });
			AutoSuggestVM.IsInvalidTextAllowed = true;
			
			editCommand = new CommandViewModel("Edit", new RelayCommand((x) => { InvokeEdit(); }));
			newCommand = new CommandViewModel("New", new RelayCommand((x) => { InvokeNew(); }));

            AutoSuggestVM.SelectedSuggestion = AllCities[1];
		}
		private CommandViewModel editCommand;
		private CommandViewModel newCommand;

		public void InvokeEdit()
		{
			if (AutoSuggestVM != null && AutoSuggestVM.SelectedSuggestion != null)
			{
				City city = (City)AutoSuggestVM.SelectedSuggestion;
				CityEditVM cityEdit = new CityEditVM(city);
				CityEditWindow win = new CityEditWindow(cityEdit);
				win.ShowDialog();
			}
		}

		public void InvokeNew()
		{
			//long maxCityId = Cities.Max(x => x.Key);
			//City city = new City() { Key= maxCityId+1, Name="", Country = null};
			//CityEditVM cityEdit = new CityEditVM(city);
			//cityEdit.CitySaved += new EventHandler(cityEdit_CitySaved);
			//CityEditWindow win = new CityEditWindow(cityEdit);
			//win.ShowDialog();
		}

		void cityEdit_CitySaved(object sender, EventArgs e)
		{
			//CityEditVM vm = (CityEditVM)sender;
			//if (vm.City != null && vm.City.IsValid)
			//{
			//    Cities.Add(vm.City);
			//}
			//vm.CitySaved -= new EventHandler(cityEdit_CitySaved);
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if(e.Property == IsInvokeEditAllowedProperty)
			{
				if(true.Equals(e.NewValue)) AutoSuggestVM.Commands.Add(editCommand);
				else AutoSuggestVM.Commands.Remove(editCommand);
			}
			else if(e.Property == IsInvokeNewAllowedProperty)
			{
				if (true.Equals(e.NewValue)) AutoSuggestVM.Commands.Add(newCommand);
				else AutoSuggestVM.Commands.Remove(newCommand);
			}
		}
	}
}
