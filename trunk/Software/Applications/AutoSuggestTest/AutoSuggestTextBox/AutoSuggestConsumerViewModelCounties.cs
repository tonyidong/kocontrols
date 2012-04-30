using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using KOControls.GUI.Core;
using KOControls.Samples.Core.Model;
using KOControls.Samples.Core.Services;
using KOControls.GUI;

namespace ControlTestApp
{
	public class AutoSuggestConsumerViewModelCounties : DependencyObject
	{
		public AutoSuggestConsumerViewModelCounties()
		{
			var styleModel = AutoSuggestControlStyleViewModel.CreateDefaultInstance();
			styleModel.IsAutoCompleteOn = false;
			var countySuggestionToStringValueConverter = new ValueConverter(x => x == null ? "" : ((County)x).Name);

			var selector = SelectorFactory.GetCountySelector(countySuggestionToStringValueConverter, TestDataService.GetCounties());
			AutoSuggestVM = new AutoSuggestViewModel(selector, countySuggestionToStringValueConverter, countySuggestionToStringValueConverter, styleModel);
		}

		public AutoSuggestViewModel AutoSuggestVM { get; protected set; } //We can also make our class to derive from AutoSuggestViewModel
	}
}
