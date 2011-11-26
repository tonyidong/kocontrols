using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KOControls.GUI;
using KOControls.GUI.Core;
using KOControls.Core;
using System.Windows.Data;
using KOControls.Samples.Core.Model;
using KOControls.Samples.Core.Services;
using KOControls.Samples.Core;
using ControlTestApp.AutoSuggestTextBox;

namespace ControlTestApp
{
	public class AutoSuggestConsumerViewModel : AutoSuggestConsumerViewModelBase
	{
		public AutoSuggestConsumerViewModel()
		{
			AutoSuggestVM.Suggestion = AllCities[1];
		}
	}
}
