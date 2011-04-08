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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlTestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			AutoSuggestTextBox.AutoSuggestTextBoxTest autoSuggestTest = new AutoSuggestTextBox.AutoSuggestTextBoxTest();
			AutoSuggestTextBox.AutoSuggestViewModel vm = new AutoSuggestTextBox.AutoSuggestViewModel();
			autoSuggestTest.DataContext = vm;
			ShowControl(autoSuggestTest);
		}

		private void ShowControl(Control ctrl)
		{
			contentPlaceholderPanel.Children.Clear();
			contentPlaceholderPanel.Children.Add(ctrl);
		}
	}
}
