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

namespace ControlTestApp.AutoSuggestTextBox
{
	public class AutoSuggestConsumerViewModel : DependencyObject
	{
		//public static readonly DependencyProperty CitiesProperty =
		//     DependencyProperty.Register("Cities", typeof(ObservableCollection<City>),
		//     typeof(AutoSuggestConsumerViewModel), new FrameworkPropertyMetadata(new PropertyChangedCallback(Cities_Changed)));

		//public ObservableCollection<City> Cities
		//{
		//    get{return (ObservableCollection<City>)GetValue(CitiesProperty);}
		//    set { SetValue(CitiesProperty, value); }
		//}

		public static readonly DependencyProperty SelectedCityProperty =
			 DependencyProperty.Register("SelectedCity", typeof(City),
			 typeof(AutoSuggestConsumerViewModel),new PropertyMetadata());

		public City SelectedCity
		{
			get { return (City)GetValue(SelectedCityProperty); }
			set { SetValue(SelectedCityProperty, value); }
		}

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

		public static readonly DependencyProperty IsAllowInvalidProperty =
		 DependencyProperty.Register("IsAllowInvalid", typeof(bool),
		 typeof(AutoSuggestConsumerViewModel), new PropertyMetadata());

		public bool IsAllowInvalid
		{
			get { return (bool)GetValue(IsAllowInvalidProperty); }
			set { SetValue(IsAllowInvalidProperty, value); }
		}

		private IList<City> AllCities { get; set; }
		public AutoSuggestViewModel<City> AutoSuggestVM { get; private set; }

		public AutoSuggestConsumerViewModel()
		{
			SelectedCity = null;
			
			AutoSuggestVM = new AutoSuggestViewModel<City>();
			AllCities = TestDataService.GetCities();
			AutoSuggestVM.FilterItems = new RelayCommand((x) => { AutoSuggestVM.ItemsSource = AllCities.Where(y => y.Name.StartsWith(x.ToString())).ToList<City>(); });
		}

		private static void Cities_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{

		}

		public void InvokeEdit()
		{
			if (SelectedCity != null)
			{
				CityEditVM cityEdit = new CityEditVM(SelectedCity);
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
