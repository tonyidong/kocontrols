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

namespace ControlTestApp.AutoSuggestTextBox
{
	public class AutoSuggestConsumerViewModel : DependencyObject
	{
		public static readonly DependencyProperty IsAllowInvokeNewProperty =
		 DependencyProperty.Register("IsAllowInvokeNew", typeof(bool),
		 typeof(AutoSuggestConsumerViewModel), new PropertyMetadata());

		public bool IsAllowInvokeNew
		{
			get { return (bool)GetValue(IsAllowInvokeNewProperty); }
			set { SetValue(IsAllowInvokeNewProperty, value); }
		}

		public static readonly DependencyProperty IsAllowInvokeEditProperty =
		 DependencyProperty.Register("IsAllowInvokeEdit", typeof(bool),
		 typeof(AutoSuggestConsumerViewModel), new PropertyMetadata());

		public bool IsAllowInvokeEdit
		{
			get { return (bool)GetValue(IsAllowInvokeEditProperty); }
			set { SetValue(IsAllowInvokeEditProperty, value); }
		}

		public IList<City> AllCities { get; set; }
		public AutoSuggestViewModel AutoSuggestVM { get; private set; }

		public AutoSuggestConsumerViewModel()
		{
			AllCities = TestDataService.GetCities();

			AutoSuggestVM = new AutoSuggestViewModel((x) => { if (x == null)return ""; else return ((City)x).Name; });
			AutoSuggestVM.FilterItems = new RelayCommand((x) => { AutoSuggestVM.ItemsSource = AllCities.Where(y => y.Name.StartsWith(x.ToString(), StringComparison.CurrentCultureIgnoreCase)).ToList<City>(); });
			AutoSuggestVM.IsAllowInvalidText = true;
            CommandViewModel cvm = new CommandViewModel("Edit",new RelayCommand((x) => {MessageBox.Show("test");}));
            AutoSuggestVM.Commands.Add(cvm);

            AutoSuggestVM.SelectedSuggestion = AllCities[1];
		}

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
	}
}
