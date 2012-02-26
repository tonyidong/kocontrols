using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using KOControls.Core;
using System.Collections;
using KOControls.GUI.Core;
using KOControls.Samples.Core.Model;
using KOControls.Samples.Core.Services;

namespace KOControls.GUI.Tests.AutoSuggest
{
	internal class AutoSuggestConsumerViewModel : AutoSuggestViewModel
	{
		public AutoSuggestConsumerViewModel()
			: base(new AutoSuggestViewModel.DefaultSelector(new ValueConverter(x => x == null ? "" : ((City)x).Name), TestDataService.AllCities), new ValueConverter(x => x == null ? "" : ((City)x).Name))
		{

		}

		public int SuggestionsCount
		{
			get
			{
				ICollection<City> c = Suggestions as ICollection<City>;
				if (c != null)
					return c.Count;

				int result = 0;
				if (Suggestions != null)
				{
					foreach (var next in Suggestions)
							result++;
				}
				return result;
			}
		}
	}
}
