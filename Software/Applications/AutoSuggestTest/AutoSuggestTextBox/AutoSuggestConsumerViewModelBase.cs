using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using KOControls.GUI.Core;
using KOControls.Samples.Core.Model;
using KOControls.Samples.Core;
using KOControls.GUI;
using KOControls.Core;
using KOControls.Samples.Core.Services;
using System.Windows.Data;

namespace ControlTestApp
{
	public abstract class AutoSuggestConsumerViewModelBase : DependencyObject
	{
		public static IValueConverter CitySuggestionToStringValueConverter = new ValueConverter(x => x == null ? "" : ((City)x).Name);

		protected AutoSuggestConsumerViewModelBase()
		{
			AllCities = TestDataService.GetCities();

			var styleModel = AutoSuggestControlStyleViewModel.CreateDefaultInstance();
			var selector = SelectorFactory.GetDefaultSelector(CitySuggestionToStringValueConverter, AllCities);
			AutoSuggestVM = new AutoSuggestViewModel(selector, CitySuggestionToStringValueConverter, CitySuggestionToStringValueConverter, styleModel);
			AutoSuggestVM.Separators = new [] { ";" };
			AutoSuggestVM.FreeTextToSuggestionConverter = FreeTextToSuggestionConverter;

			EditCommand = new Command(x => InvokeEdit(), (obj) => CanInvokeEdit(), "Edit");
			NewCommand = new Command(x => InvokeNew(""), null, "New");

			IsInvokeEditAllowed = true;
		}

		public AutoSuggestViewModel AutoSuggestVM { get; protected set; }
		public List<City> AllCities { get; set; }	

		#region Invoke New
		public static readonly DependencyProperty IsInvokeNewAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestConsumerViewModelBase>("IsInvokeNewAllowed", false,
			(d, e) =>
			{
				var autoSuggestConsumerVM = d as AutoSuggestConsumerViewModelBase;
				if(autoSuggestConsumerVM != null)
				{
					if (true.Equals(e.NewValue))
						autoSuggestConsumerVM.AutoSuggestVM.Commands.Add(autoSuggestConsumerVM.NewCommand);
					else
						autoSuggestConsumerVM.AutoSuggestVM.Commands.Remove(autoSuggestConsumerVM.NewCommand);
				}
			});
		public bool IsInvokeNewAllowed { get { return (bool)GetValue(IsInvokeNewAllowedProperty); } set { SetValue(IsInvokeNewAllowedProperty, value); } }

		protected readonly Command NewCommand;
		public City InvokeNew(string name)
		{
			long maxCityId = City.GetNextKey();
			var city = new City() { Key = maxCityId + 1, Name = name, Country = null };
			var cityEdit = new CityEditVM(city);
			var win = new CityEditWindow(cityEdit);
			if (win.ShowDialog() == true)
			{
				if (cityEdit.City != null && cityEdit.City.IsValid)
				{
					AllCities.Add(cityEdit.City);
					return cityEdit.City;
				}
			}
			return null;
		}
		#endregion

		#region Invoke Edit
		public static readonly DependencyProperty IsInvokeEditAllowedProperty = ViewModel.RegisterProperty<bool, AutoSuggestConsumerViewModelBase>("IsInvokeEditAllowed", false,
			(d, e) =>
			{
				var autoSuggestConsumerVM = d as AutoSuggestConsumerViewModelBase;
				if(autoSuggestConsumerVM != null)
				{
					if (true.Equals(e.NewValue))
						autoSuggestConsumerVM.AutoSuggestVM.Commands.Add(autoSuggestConsumerVM.EditCommand);
					else
						autoSuggestConsumerVM.AutoSuggestVM.Commands.Remove(autoSuggestConsumerVM.EditCommand);
				}
			});
		public bool IsInvokeEditAllowed { get { return (bool)GetValue(IsInvokeEditAllowedProperty); } set { SetValue(IsInvokeEditAllowedProperty, value); } }
		protected readonly Command EditCommand;
		private bool CanInvokeEdit()
		{
			return AutoSuggestVM != null && AutoSuggestVM.SuggestionPreview != null;
		}
		public void InvokeEdit()
		{
			if (AutoSuggestVM != null && AutoSuggestVM.Suggestion != null)
			{
				var city = (City)AutoSuggestVM.SuggestionPreview;
				var cityEdit = new CityEditVM(city);
				var win = new CityEditWindow(cityEdit);

				AutoSuggestVM.Cancel();

				if (win.ShowDialog() == true)
				{
					AutoSuggestVM.Refresh();
					AutoSuggestVM.SuggestionPreview = city;
				}
			}
		}
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
						autoSuggestConsumerVM.AutoSuggestVM.Delay = new TimeSpan(0, 0, 0, 0, vLong);
				}
			}
			, (d, v) =>
			{
				var vStr = v as string;
				var vLong = 0;
				if (vStr.IsNullOrWhiteSpace() || !Int32.TryParse(vStr, out vLong))
					return "0";
				return v;
			});
		public string Delay { get { return (string)GetValue(DelayProperty); } set { SetValue(DelayProperty, value); } }
		#endregion

		#region Free Text To Suggestion Converter
		private bool FreeTextToSuggestionConverter(string filterInput, out object suggestion)
		{
			suggestion = null;
			if (!filterInput.IsNullOrWhiteSpace())
				suggestion = InvokeNew(filterInput);

			return suggestion != null;
		}
		#endregion 
	}
}
