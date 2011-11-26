using System;
using System.Windows;

namespace KOControls.Samples.Core
{
	public partial class CityEditWindow : Window
	{
		public new CityEditVM DataContext { get { return (CityEditVM)base.DataContext; } set { base.DataContext = value; } }

		public CityEditWindow()
		{
			InitializeComponent();
		}

		public CityEditWindow(CityEditVM vm)
			: this()
		{
			this.DataContext = vm;
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void saveButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
