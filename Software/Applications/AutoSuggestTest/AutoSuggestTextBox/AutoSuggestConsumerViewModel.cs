using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KOControls.GUI;
using KOControls.Core;
using System.Windows.Data;
using KOControls.Samples.Core.Model;
using System.Collections;
using System.Globalization;

namespace ControlTestApp
{
	public class AutoSuggestConsumerViewModel : AutoSuggestConsumerViewModelBase
	{
		public AutoSuggestConsumerViewModel()
		{
			AutoSuggestVM.Suggestion = AllCities[1];
		}

		#region IsFilterByCountryCity
		public static readonly DependencyProperty IsFilterByCountryCityProperty = ViewModel.RegisterProperty<bool, AutoSuggestConsumerViewModel>("IsFilterByCountryCity", false,
			(d, e) =>
			{
				var autoSuggestConsumerVM = d as AutoSuggestConsumerViewModelBase;
				if (autoSuggestConsumerVM != null)
				{
					if (true.Equals(e.NewValue))
						autoSuggestConsumerVM.AutoSuggestVM.Selector = SelectorFactory.GetCitynameCountrySelector(CitySuggestionToStringValueConverter, autoSuggestConsumerVM.AllCities);
					else
						autoSuggestConsumerVM.AutoSuggestVM.Selector = SelectorFactory.GetDefaultSelector(CitySuggestionToStringValueConverter, autoSuggestConsumerVM.AllCities);
				}
			});
		public bool IsFilterByCountryCity { get { return (bool)GetValue(IsFilterByCountryCityProperty); } set { SetValue(IsFilterByCountryCityProperty, value); } }
		#endregion
	}
}
