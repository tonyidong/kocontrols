using KOControls.Core;
using System.Windows;

namespace KOControls.GUI
{
	public class AutoSuggestControlStyleViewModel : ViewModel
	{
		public static AutoSuggestControlStyleViewModel CreateDefaultInstance() { return new AutoSuggestControlStyleViewModel(TaboutTriggers.All, ConfirmTriggers.SpaceTabArrows); }

		#region Constructors
		public AutoSuggestControlStyleViewModel()
		{
		}
		public AutoSuggestControlStyleViewModel(TaboutTriggers taboutTrigger, ConfirmTriggers confirmTrigger)
		{
			TaboutTrigger = taboutTrigger;
			ConfirmTrigger = confirmTrigger;
		}
		#endregion 

		#region IsAutoCompleteOn
		public static readonly DependencyProperty IsAutoCompleteOnProperty = ViewModel.RegisterProperty<bool, AutoSuggestControlStyleViewModel>("IsAutoCompleteOn", true);
		public bool IsAutoCompleteOn { get { return (bool)GetValue(IsAutoCompleteOnProperty); } set { SetValue(IsAutoCompleteOnProperty, value); } }
		#endregion

		#region IsFilterTextDisplayed 
		public static readonly DependencyProperty IsFilterTextDisplayedProperty = ViewModel.RegisterProperty<bool, AutoSuggestControlStyleViewModel>("IsFilterTextDisplayed", false);
		public bool IsFilterTextDisplayed { get { return (bool)GetValue(IsFilterTextDisplayedProperty); } set { SetValue(IsFilterTextDisplayedProperty, value); } }
		#endregion

		#region ApplyFilterTrigger
		public static readonly DependencyProperty ApplyFilterTriggerProperty = ViewModel.RegisterProperty<ApplyFilterTriggers, AutoSuggestControlStyleViewModel>("ApplyFilterTrigger", ApplyFilterTriggers.FilterInputChanged);
		public ApplyFilterTriggers ApplyFilterTrigger { get { return (ApplyFilterTriggers)GetValue(ApplyFilterTriggerProperty); } set { SetValue(ApplyFilterTriggerProperty, value); } }
		#endregion

		#region ConfirmTrigger
		public static readonly DependencyProperty SelectionTriggerProperty = ViewModel.RegisterProperty<ConfirmTriggers, AutoSuggestControlStyleViewModel>("ConfirmTrigger", ConfirmTriggers.SpaceTabArrows);
		public ConfirmTriggers ConfirmTrigger { get { return (ConfirmTriggers)GetValue(SelectionTriggerProperty); } set { SetValue(SelectionTriggerProperty, value); } }
		#endregion

		#region TaboutTrigger
		public static readonly DependencyProperty TaboutCommandProperty = ViewModel.RegisterProperty<TaboutTriggers, AutoSuggestControlStyleViewModel>("TaboutTrigger", TaboutTriggers.Enter);
		public TaboutTriggers TaboutTrigger { get { return (TaboutTriggers)GetValue(TaboutCommandProperty); } set { SetValue(TaboutCommandProperty, value); } }
		#endregion
	}
}
