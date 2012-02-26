using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using KOControls.Samples.Core.Services;
using KOControls.Samples.Core.Model;

namespace KOControls.GUI.Tests.AutoSuggest
{
	[TestFixture]
	public class AutoSuggestViewModelTests : BaseTests
	{
		private AutoSuggestConsumerViewModel CreateConsumerAutoSuggestViewModel()
		{
			return new AutoSuggestConsumerViewModel();
		}

		[Test]
		public void Can_FilterSuggestionsFilter()
		{
			AutoSuggestConsumerViewModel asVM = CreateConsumerAutoSuggestViewModel();
	
			asVM.ApplyFilter("");
			Assert.AreEqual(asVM.SuggestionsCount, TestDataService.AllCities.Count);

			asVM.ApplyFilter(TestDataService.AllCities[0].Name);
			Assert.AreEqual(asVM.SuggestionsCount, 1);

			asVM.ApplyFilter(TestDataService.AllCities[0].Name.Substring(0, 2));
			Assert.GreaterOrEqual(asVM.SuggestionsCount, 1);

			asVM.ApplyFilter("Invalid Suggestion");
			Assert.AreEqual(asVM.SuggestionsCount, 0);
		}

		[Test]
		public void Can_SetSuggestionsPreview()
		{
			AutoSuggestConsumerViewModel asVM = CreateConsumerAutoSuggestViewModel();
			City firstCity = TestDataService.AllCities[0];

			asVM.ApplyFilter(firstCity.Name.Substring(0, 1));
			asVM.SuggestionPreview = firstCity;
			Assert.AreEqual(firstCity, asVM.SuggestionPreview);

			asVM.ConfirmCommand.Execute();
			Assert.AreEqual(firstCity, asVM.SuggestionPreview);
			Assert.AreEqual(firstCity, asVM.Suggestion);
		}

		#region IsEmptyValueAllowed
		[Test]
		public void IsEmptyValueAllowed_Test()
		{
			AutoSuggestConsumerViewModel asVM = CreateConsumerAutoSuggestViewModel();

			asVM.IsEmptyValueAllowed = true;

			asVM.IsFreeTextAllowed = false;
			ExecuteIsEmptyValueAllowed_Test(asVM);
			asVM.IsFreeTextAllowed = true;
			ExecuteIsEmptyValueAllowed_Test(asVM);

			asVM.IsEmptyValueAllowed = false;

			asVM.IsFreeTextAllowed = true;
			ExecuteIsEmptyValueNotAllowed_Test(asVM);
			asVM.IsFreeTextAllowed = false;
			ExecuteIsEmptyValueNotAllowed_Test(asVM);
		}

		private void ExecuteIsEmptyValueAllowed_Test(AutoSuggestConsumerViewModel asVM)
		{
			City newEmptyValue = new City();
			newEmptyValue.Name = "Empty Value";
			newEmptyValue.Key = City.GetNextKey();
			City city = new City();
			city.Key = City.GetNextKey();

			asVM.SuggestionPreview = asVM.EmptyValue;
			Assert.AreEqual(asVM.EmptyValue, asVM.SuggestionPreview);
			asVM.ConfirmCommand.Execute();
			Assert.AreEqual(asVM.EmptyValue, asVM.Suggestion);

			asVM.EmptyValue = newEmptyValue;
			asVM.SuggestionPreview = asVM.EmptyValue;
			Assert.AreEqual(asVM.EmptyValue, asVM.SuggestionPreview);
			asVM.ConfirmCommand.Execute();
			Assert.AreEqual(asVM.EmptyValue, asVM.Suggestion);

			asVM.SuggestionPreview = city;
			asVM.ConfirmCommand.Execute();
			Assert.AreEqual(asVM.EmptyValue, asVM.Suggestion);
		}

		private void ExecuteIsEmptyValueNotAllowed_Test(AutoSuggestConsumerViewModel asVM)
		{
			City newEmptyValue = new City();
			newEmptyValue.Name = "Empty Value";
			newEmptyValue.Key = City.GetNextKey();
			
			asVM.Suggestion = TestDataService.AllCities[0];

			asVM.SuggestionPreview = asVM.EmptyValue;
			Assert.AreEqual(asVM.EmptyValue, asVM.SuggestionPreview);
			asVM.ConfirmCommand.Execute();
			Assert.AreEqual(TestDataService.AllCities[0], asVM.Suggestion);

			asVM.EmptyValue = newEmptyValue;
			asVM.SuggestionPreview = asVM.EmptyValue;
			Assert.AreEqual(asVM.EmptyValue, asVM.SuggestionPreview);
			asVM.ConfirmCommand.Execute();
			Assert.AreEqual(TestDataService.AllCities[0], asVM.Suggestion);
		}
		#endregion 

		#region IsFreeTextAllowed
		private const string InvalidFreeText = "Invalid FreeText";
		[Test]
		public void IsFreeTextAllowed_Test()
		{
			AutoSuggestConsumerViewModel asVM = CreateConsumerAutoSuggestViewModel();
			asVM.FreeTextToSuggestionConverter = null;

			asVM.IsFreeTextAllowed = true;
			//FreeText should not be allowed because there is not FreeTextToSuggestionConverter.
			ExecuteIsFreeTextNotAllowed_Test(asVM);
			asVM.IsEmptyValueAllowed = true;
			ExecuteIsFreeTextNotAllowed_Test(asVM);
			asVM.IsEmptyValueAllowed = false;
			ExecuteIsFreeTextNotAllowed_Test(asVM);

			asVM.FreeTextToSuggestionConverter = FreeTextToSuggestionConverter;

			asVM.IsEmptyValueAllowed = false;
			ExecuteIsFreeTextAllowed_Test(asVM);
			asVM.IsEmptyValueAllowed = true;
			ExecuteIsFreeTextAllowed_Test(asVM);

			asVM.IsFreeTextAllowed = false;

			asVM.IsEmptyValueAllowed = true;
			ExecuteIsFreeTextNotAllowed_Test(asVM);
			asVM.IsEmptyValueAllowed = false;
			ExecuteIsFreeTextNotAllowed_Test(asVM);
		}

		private void ExecuteIsFreeTextAllowed_Test(AutoSuggestConsumerViewModel asVM)
		{
			asVM.ApplyFilterCommand.Execute("Cape Town");
			asVM.SuggestionPreview = null;
			Assert.IsNull(asVM.SuggestionPreview);

			asVM.ConfirmCommand.Execute("Cape Town");
			Assert.IsNotNull(asVM.Suggestion);
			Assert.AreEqual(((City)asVM.Suggestion).Name, "Cape Town");
		}

		private void ExecuteIsFreeTextNotAllowed_Test(AutoSuggestConsumerViewModel asVM)
		{
			asVM.Suggestion = TestDataService.AllCities[0];
			asVM.SuggestionPreview = null;

			asVM.ApplyFilterCommand.Execute("Cape Town");
			Assert.IsNull(asVM.SuggestionPreview);
			Assert.AreEqual(0, asVM.SuggestionsCount);
			
			asVM.ConfirmCommand.Execute("Cape Town");
			Assert.AreEqual(TestDataService.AllCities[0], asVM.Suggestion);
		}

		private bool FreeTextToSuggestionConverter(string filterInput, out object suggestion)
		{
			City city = null;
			if(filterInput != InvalidFreeText)
			{
				city = new City();
				city.Key = City.GetNextKey();
				city.Name = filterInput;
			}

			suggestion = city;
			return city != null;
		}
		#endregion 
	}
}
