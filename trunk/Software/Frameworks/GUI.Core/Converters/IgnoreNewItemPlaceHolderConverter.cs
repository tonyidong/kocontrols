using System;
using System.Windows.Data;

namespace KOControls.GUI.Core
{
	public class IgnoreNewItemPlaceHolderConverter : IValueConverter
	{
		public static readonly IgnoreNewItemPlaceHolderConverter Instance = new IgnoreNewItemPlaceHolderConverter();

		private const string NewItemPlaceholderName = "{NewItemPlaceholder}";

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value != null && value.ToString() == NewItemPlaceholderName)
				return null;// DependencyProperty.UnsetValue;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value != null && value.ToString() == NewItemPlaceholderName)
				return null;// DependencyProperty.UnsetValue;
			return value;
		}
	}
}
