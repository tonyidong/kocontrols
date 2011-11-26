using System;
using System.Collections.Generic;
using System.Windows;
using KOControls.GUI.Core;
using KOControls.Samples.Core.Model;
using KOControls.Samples.Core;
using KOControls.GUI;
using KOControls.Core;
using KOControls.Samples.Core.Services;
using System.Windows.Data;

namespace ControlTestApp.AutoSuggestTextBox
{
	public abstract class AutoSuggestConsumerViewModelBase : DependencyObject
	{
		protected AutoSuggestConsumerViewModelBase()
		{
			AllCities = TestDataService.GetCities();

			IValueConverter valueConverter = new ValueConverter(x => x == null ? "" : ((City)x).Name);
			ISelector selector = new AutoSuggestViewModel.DefaultSelector(valueConverter, AllCities);
			AutoSuggestVM = new AutoSuggestViewModel(selector, valueConverter);

			AutoSuggestVM.FreeTextToSuggestionConverter = FreeTextToSuggestionConverter;
			EditCommand = new Command(x => InvokeEdit(), null, "Edit");
			NewCommand = new Command(x => InvokeNew(""), null, "New");

			IsInvokeEditAllowed = true;
		}
		private bool FreeTextToSuggestionConverter(string filterInput, out object suggestion)
		{
			var city = InvokeNew(filterInput);

			suggestion = city;

			return city != null;
		}

		#region IsInvokeNewAllowed
		protected readonly Command NewCommand;
		public static readonly DependencyProperty IsInvokeNewAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestConsumerViewModelBase>("IsInvokeNewAllowed", false,
			(d, e) =>
			{
				var autoSuggestConsumerVM = d as AutoSuggestConsumerViewModelBase;
				if(autoSuggestConsumerVM != null)
				{
					if(true.Equals(e.NewValue)) autoSuggestConsumerVM.AutoSuggestVM.Commands.Add(autoSuggestConsumerVM.NewCommand);
					else autoSuggestConsumerVM.AutoSuggestVM.Commands.Remove(autoSuggestConsumerVM.NewCommand);
				}
			});
		public bool IsInvokeNewAllowed { get { return (bool)GetValue(IsInvokeNewAllowedProperty); } set { SetValue(IsInvokeNewAllowedProperty, value); } }
		#endregion

		#region IsInvokeEditAllowed
		protected readonly Command EditCommand;
		public static readonly DependencyProperty IsInvokeEditAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestConsumerViewModelBase>("IsInvokeEditAllowed", false,
			(d, e) =>
			{
				var autoSuggestConsumerVM = d as AutoSuggestConsumerViewModelBase;
				if(autoSuggestConsumerVM != null)
				{
					if(true.Equals(e.NewValue)) autoSuggestConsumerVM.AutoSuggestVM.Commands.Add(autoSuggestConsumerVM.EditCommand);
					else autoSuggestConsumerVM.AutoSuggestVM.Commands.Remove(autoSuggestConsumerVM.EditCommand);
				}
			});
		public bool IsInvokeEditAllowed { get { return (bool)GetValue(IsInvokeEditAllowedProperty); } set { SetValue(IsInvokeEditAllowedProperty, value); } }
		#endregion

		#region Delay
		public static readonly DependencyProperty DelayProperty = ViewModel.RegisterProperty<string, AutoSuggestConsumerViewModelBase>("Delay", "0",
			(d, e) =>
			{
				var autoSuggestConsumerVM = d as AutoSuggestConsumerViewModelBase;
				if(autoSuggestConsumerVM != null)
				{
					var vLong = 0;
					if(e.NewValue != null && Int32.TryParse(e.NewValue.ToString(), out vLong))
					{
						autoSuggestConsumerVM.AutoSuggestVM.Delay = new TimeSpan(0, 0, 0, 0, vLong);
					}
				}
			}
			, (d, v) =>
			{
				var vStr = v as string;
				var vLong = 0;
				if(String.IsNullOrWhiteSpace(vStr) || !Int32.TryParse(vStr, out vLong))
					return "0";
				return v;
			});
		public string Delay { get { return (string)GetValue(DelayProperty); } set { SetValue(DelayProperty, value); } }
		#endregion

		public AutoSuggestViewModel AutoSuggestVM { get; protected set; }

		public IList<City> AllCities { get; set; }

		public void InvokeEdit()
		{
			if(AutoSuggestVM != null && AutoSuggestVM.Suggestion != null)
			{
				var city = (City)AutoSuggestVM.SuggestionPreview;
				var cityEdit = new CityEditVM(city);
				var win = new CityEditWindow(cityEdit);

				AutoSuggestVM.Cancel();

				if(win.ShowDialog() == true)
				{
					AutoSuggestVM.Refresh();
					AutoSuggestVM.SuggestionPreview = city;
				}
			}
		}

		public City InvokeNew(string name)
		{
			long maxCityId = City.GetNextKey();
			var city = new City() { Key = maxCityId + 1, Name = name, Country = null };
			var cityEdit = new CityEditVM(city);
			var win = new CityEditWindow(cityEdit);
			if(win.ShowDialog() == true)
			{
				if(cityEdit.City != null && cityEdit.City.IsValid)
				{
					AllCities.Add(cityEdit.City);
					return cityEdit.City;
				}
			}
			return null;
		}
	}
}
