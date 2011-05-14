using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes; 

namespace ControlTestApp.AutoSuggestTextBox
{
	/// <summary>
	/// Interaction logic for CityEditWindow.xaml
	/// </summary>
	public partial class CityEditWindow : Window
	{
		CityEditVM vm = null;
		public CityEditWindow()
		{
			InitializeComponent();
		}

		public CityEditWindow(CityEditVM vm)
			: this()
		{
			this.vm = vm;
			this.DataContext = vm;
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void saveButton_Click(object sender, RoutedEventArgs e)
		{
			vm.SaveCity();
		}
	}
}
