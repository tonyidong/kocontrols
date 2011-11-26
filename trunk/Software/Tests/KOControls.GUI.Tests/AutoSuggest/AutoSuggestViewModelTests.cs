using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using KOControls.Samples.Core.Services;

namespace KOControls.GUI.Tests.AutoSuggest
{
	[TestFixture]
	public class AutoSuggestViewModelTests : BaseTests
	{
		private AutoSuggestViewModel CreateConsumerAutoSuggestViewModel()
		{
			return new AutoSuggestConsumerViewModel();
		}

		[Test]
		public void Can_FilterSuggestions()
		{
			AutoSuggestConsumerViewModel asVM = (AutoSuggestConsumerViewModel)CreateConsumerAutoSuggestViewModel();
			asVM.ApplyFilter("");

			Assert.AreEqual(TestDataService.GetCities().Count, asVM.SuggestionsCount);
		}
	}
}
