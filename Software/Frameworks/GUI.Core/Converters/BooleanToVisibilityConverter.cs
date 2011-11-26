using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace KOControls.GUI.Core
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();

		private BooleanToVisibilityConverter() { }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var visible = false;
			if(value is bool) visible = (bool)value;
			else if(value is string) Boolean.TryParse((string)value, out visible);

			return visible ? Visibility.Visible : Visibility.Collapsed;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var visible = Visibility.Visible;
			if(value is Visibility) visible = (Visibility)value;
			else if(value is string) Visibility.TryParse((string)value, out visible);

			return visible == Visibility.Visible;
		}
	}
}
