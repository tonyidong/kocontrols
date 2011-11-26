using System;
using System.Globalization;
using System.Windows.Data;

namespace KOControls.GUI.Core
{
	public class ValueConverter : IValueConverter
	{
		public ValueConverter(Func<object, object> convert, Func<object, object> convertBack = null)
		{
			_convert = convert;
			_convertBack = convertBack;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { return _convert(value); }
		private readonly Func<object, object> _convert;
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(_convertBack == null) throw new NotImplementedException();

			return _convertBack(value);
		}
		private readonly Func<object, object> _convertBack;
	}
}
