using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using KOControls.Core;
using System.Globalization;

namespace KOControls.GUI
{
	using __type = AutoSuggestComboBox;
	public class AutoSuggestComboBox : Control
	{
		protected static readonly ResourceDictionary ResourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/KOControls.GUI;component/AutoSuggestComboBox.xaml") };
		private static readonly ControlTemplate AutoSuggestComboBox_Template;
		private static readonly Style AutoSuggestControl_DisplayMemberPathStyle;

		static AutoSuggestComboBox()
		{
			AutoSuggestComboBox_Template = (ControlTemplate)ResourceDictionary["AutoSuggestComboBox_Template"];
			AutoSuggestControl_DisplayMemberPathStyle = (Style)ResourceDictionary["AutoSuggestControl_DisplayMemberPathStyle"];
		}
		public AutoSuggestComboBox()
		{
			VM = new AutoSuggestViewModel();
			VM.Selector = CreateSelector();
			
			SetCurrentValue(TemplateProperty, AutoSuggestComboBox_Template);

			ApplyTemplate();

			Focusable = false;
		}
		private bool _initialized;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_popup = (Popup)Template.FindName("_popup", this);
			_textBox = (TextBox)Template.FindName("_textBox", this);
			
			_toggleButton = (ToggleButton)Template.FindName("_toggleButton", this);
			_toggleButton.Click += ToggleButtonClick;

			_autoSuggestControl = (AutoSuggestControl)Template.FindName("_autoSuggestControl", this);
			_autoSuggestControl.ApplyTemplate();
		}
		private TextBox _textBox;
		private Popup _popup;
		private AutoSuggestControl _autoSuggestControl;
		private ToggleButton _toggleButton;

		#region VM
		private static readonly DependencyPropertyKey VMPropertyKey = ViewModel.RegisterReadOnlyProperty<AutoSuggestViewModel, __type>("VM");
		public static DependencyProperty VMProperty { get { return VMPropertyKey.DependencyProperty; } }
		public AutoSuggestViewModel VM { get { return (AutoSuggestViewModel)GetValue(VMProperty); } private set { SetValue(VMPropertyKey, value); } }
		#endregion

		#region ItemsSource
		public static readonly DependencyProperty ItemsSourceProperty = ViewModel.RegisterProperty<IEnumerable, AutoSuggestComboBox>("ItemsSource", null, (d, a) =>
		{
			var autoSuggestComboBox = (AutoSuggestComboBox)d;
			if (autoSuggestComboBox._initialized)
				autoSuggestComboBox.VM.Selector = autoSuggestComboBox.CreateSelector();
			else
				Dependents_Changed(d, a);
		});
		public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
		#endregion


		#region SelectedValue
		public static readonly DependencyProperty SelectedValueProperty = ViewModel.RegisterProperty<object, __type>("SelectedValue");
		public object SelectedValue { get { return (object)GetValue(SelectedValueProperty); } set { SetValue(SelectedValueProperty, value); } }
		#endregion

		#region SelectedValuePath
		private IValueConverter _suggestionToSelectedValueConverter = new AutoSuggestViewModel.PropertyPathSuggestionToStringConverter("");
		public static readonly DependencyProperty SelectedValuePathProperty = ViewModel.RegisterProperty<string, __type>("SelectedValuePath", null, (d, a) =>
		{
			var autoSuggestComboBox = (AutoSuggestComboBox)d;
			autoSuggestComboBox._suggestionToSelectedValueConverter = new AutoSuggestViewModel.PropertyPathSuggestionToStringConverter(a.NewValue+"");
			if(autoSuggestComboBox.IsInitialized)
				autoSuggestComboBox.SetCurrentValue(SelectedValueProperty, autoSuggestComboBox._suggestionToSelectedValueConverter.Convert(autoSuggestComboBox.VM.Suggestion, null, null, CultureInfo.CurrentCulture));
		});
		public string SelectedValuePath { get { return (string)GetValue(SelectedValuePathProperty); } set { SetValue(SelectedValuePathProperty, value); } }
		#endregion

		#region SelectedItem
		private bool _selectedItemChangingSuggestion;
		private bool _suggestionChangingSelectedItem;
		public static readonly DependencyProperty SelectedItemProperty = ViewModel.RegisterProperty<object, __type>("SelectedItem", null, (d, a) =>
			{
				var autoSuggestComboBox = (AutoSuggestComboBox)d;
				if (autoSuggestComboBox._suggestionChangingSelectedItem) return;

				autoSuggestComboBox._selectedItemChangingSuggestion = true;
				try { autoSuggestComboBox.VM.Suggestion = a.NewValue; }
				finally { autoSuggestComboBox._selectedItemChangingSuggestion = false; }
			});
		public object SelectedItem { get { return (object)GetValue(SelectedItemProperty); } set { SetValue(SelectedItemProperty, value); } }
		#endregion

		#region DisplayMemberPath
		public static readonly DependencyProperty DisplayMemberPathProperty = ViewModel.RegisterProperty<string, __type>("DisplayMemberPath", null, (d, a) =>
		{
			var autoSuggestComboBox = (AutoSuggestComboBox)d;
			if (autoSuggestComboBox._initialized)
			{
				var suggestionToStringConverter = autoSuggestComboBox.CreateSuggestionToStringConverter();
				autoSuggestComboBox.VM.SuggestionToStringConverter = suggestionToStringConverter;
				autoSuggestComboBox.VM.SuggestionPreviewToStringConverter = suggestionToStringConverter;
				autoSuggestComboBox.VM.Selector = autoSuggestComboBox.CreateSelector();
			}
			else
				Dependents_Changed(d, a);

			autoSuggestComboBox.ResolveAutoSuggestControlStyle();
		});
		public string DisplayMemberPath { get { return (string)GetValue(DisplayMemberPathProperty); } set { SetValue(DisplayMemberPathProperty, value); } }
		private IValueConverter CreateSuggestionToStringConverter()
		{
			return string.IsNullOrEmpty(DisplayMemberPath) ?
				(IValueConverter)AutoSuggestViewModel.DefaultSuggestionToStringConverter.Instance :
				(IValueConverter)new AutoSuggestViewModel.PropertyPathSuggestionToStringConverter(DisplayMemberPath);
		}
		#endregion

		#region TextBoxStyle
		public static readonly DependencyProperty TextBoxStyleProperty = ViewModel.RegisterProperty<Style, __type>("TextBoxStyle");
		public Style TextBoxStyle { get { return (Style)GetValue(TextBoxStyleProperty); } set { SetValue(TextBoxStyleProperty, value); } }
		#endregion

		#region ToggleButtonStyle
		public static readonly DependencyProperty ToggleButtonStyleProperty = ViewModel.RegisterProperty<Style, __type>("ToggleButtonStyle");
		public Style ToggleButtonStyle { get { return (Style)GetValue(ToggleButtonStyleProperty); } set { SetValue(ToggleButtonStyleProperty, value); } }
		#endregion

		#region AutoSuggestControlStyle
		public static readonly DependencyProperty AutoSuggestControlStyleProperty = ViewModel.RegisterProperty<Style, __type>("AutoSuggestControlStyle", null, (d, a)=>((__type)d).ResolveAutoSuggestControlStyle());
		public Style AutoSuggestControlStyle { get { return (Style)GetValue(AutoSuggestControlStyleProperty); } set { SetValue(AutoSuggestControlStyleProperty, value); } }
		#endregion

		#region SuggestionsFilter
		public static readonly DependencyProperty SuggestionsFilterProperty = ViewModel.RegisterProperty<Func<object, string, bool>, AutoSuggestComboBox>("SuggestionsFilter", null, (d,a) =>
		{
			var autoSuggestComboBox = (AutoSuggestComboBox)d;
			if (autoSuggestComboBox._initialized)
				autoSuggestComboBox.VM.Selector = autoSuggestComboBox.CreateSelector();                                                                                  		
		});
		public Func<object, string, bool> SuggestionsFilter { get { return (Func<object, string, bool>)GetValue(SuggestionsFilterProperty); } set { SetValue(SuggestionsFilterProperty, value); } }

		private ISelector CreateSelector()
		{
			return SuggestionsFilter == null ?
				new AutoSuggestViewModel.DefaultSelector(CreateSuggestionToStringConverter(), ItemsSource ?? new object[0]) :
				new AutoSuggestViewModel.DefaultSelector(SuggestionsFilter, ItemsSource ?? new object[0]);
		}
		#endregion

		//StyleModel options
		#region IsAutoCompleteOn
		public static readonly DependencyProperty IsAutoCompleteOnProperty = ViewModel.RegisterProperty<bool, AutoSuggestComboBox>("IsAutoCompleteOn", true,(d, a)=>
		{
			((AutoSuggestControlStyleViewModel)((AutoSuggestComboBox)d).VM.StyleModel).IsAutoCompleteOn = (bool)a.NewValue;
		});
		public bool IsAutoCompleteOn { get { return (bool)GetValue(IsAutoCompleteOnProperty); } set { SetValue(IsAutoCompleteOnProperty, value); } }
		#endregion
	
		#region ActualAutoSuggestControlStyle
		private static readonly DependencyPropertyKey ActualAutoSuggestControlStylePropertyKey = ViewModel.RegisterReadOnlyProperty<Style, __type>("ActualAutoSuggestControlStyle");
		private Style ActualAutoSuggestControlStyle { get { return (Style)GetValue(ActualAutoSuggestControlStylePropertyKey.DependencyProperty); } set { SetValue(ActualAutoSuggestControlStylePropertyKey, value); } }
		private void ResolveAutoSuggestControlStyle()
		{
			if(AutoSuggestControlStyle != null) ActualAutoSuggestControlStyle = AutoSuggestControlStyle;
			else if(!string.IsNullOrEmpty(DisplayMemberPath)) ActualAutoSuggestControlStyle = AutoSuggestControl_DisplayMemberPathStyle;
			else ActualAutoSuggestControlStyle = null;
		}
		#endregion

		protected static void Dependents_Changed(DependencyObject d, DependencyPropertyChangedEventArgs a)
		{
			var asc = (AutoSuggestComboBox)d;
			if(asc._initialized || asc.ItemsSource == null || String.IsNullOrEmpty(asc.DisplayMemberPath))
				return;

			var suggestionToStringConverter = asc.CreateSuggestionToStringConverter();
			asc.VM.SuggestionToStringConverter = suggestionToStringConverter;
			asc.VM.SuggestionPreviewToStringConverter = suggestionToStringConverter;
			asc.VM.Selector = asc.CreateSelector();

			asc.VM.StyleModel = AutoSuggestControlStyleViewModel.CreateDefaultInstance();
			//((AutoSuggestControlStyleViewModel)asc.VM.StyleModel).Separators = ";";

			if(asc.SelectedItem != null)
			{
				asc.VM.Suggestion = asc.SelectedItem;
				asc.SetCurrentValue(SelectedValueProperty, asc._suggestionToSelectedValueConverter.Convert(asc.VM.Suggestion, null, null, CultureInfo.CurrentCulture));
			}
			asc.VM.AddValueChanged(AutoSuggestViewModel.SuggestionProperty, delegate
			{
				if(!asc._selectedItemChangingSuggestion)
				{
					asc._suggestionChangingSelectedItem = true;
					try { asc.SetCurrentValue(SelectedItemProperty, asc.VM.Suggestion); }
					finally { asc._suggestionChangingSelectedItem = false; }
				}
				asc.SetCurrentValue(SelectedValueProperty, asc._suggestionToSelectedValueConverter.Convert(asc.VM.Suggestion, null, null, CultureInfo.CurrentCulture));
			});

			asc._initialized = true;
		}
		private void ToggleButtonClick(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke((Action)_textBox.SelectAll);
			_textBox.Focus();

			if(_toggleButton.IsChecked == false) return;

			VM.ApplyFilter("");
		}
	}
}
