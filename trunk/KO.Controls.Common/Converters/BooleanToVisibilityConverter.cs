using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace KO.Controls.Common.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();
		private BooleanToVisibilityConverter() { }
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool visible = true;
			if (Boolean.TryParse(value.ToString(), out visible)
				&& !visible)
			{
				return System.Windows.Visibility.Collapsed;
			}

			return System.Windows.Visibility.Visible;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
