using System;
using System.ComponentModel;
using System.Windows;

namespace KOControls.Core
{
	public class ViewModel : DependencyObject
	{
		public static bool IsInDesignMode
		{
			get
			{
				if(!_isInDesignMode.HasValue)
				{
#if SILVERLIGHT
					_isInDesignMode = DesignerProperties.IsInDesignTool;
#else
					var prop = DesignerProperties.IsInDesignModeProperty;
					_isInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
#endif
				}
				return _isInDesignMode.Value;
			}
		}
		private static bool? _isInDesignMode;

		public static DependencyProperty RegisterProperty<TYPE, OWNER>(string name, TYPE defaultValue=default(TYPE), PropertyChangedCallback valueChanged=null, CoerceValueCallback coerceValue=null)
			where OWNER : DependencyObject
		{
			return DependencyProperty.Register(name, typeof(TYPE), typeof(OWNER), new FrameworkPropertyMetadata(defaultValue, valueChanged, coerceValue));
		}

		public static DependencyPropertyKey RegisterReadOnlyProperty<TYPE, OWNER>(string name, TYPE defaultValue=default(TYPE), PropertyChangedCallback valueChanged=null, CoerceValueCallback coerceValue=null)
			where OWNER : DependencyObject
		{
			return DependencyProperty.RegisterReadOnly(name, typeof(TYPE), typeof(OWNER), new FrameworkPropertyMetadata(defaultValue, valueChanged, coerceValue));
		}

		public static void OverrideProperty<OWNER>(DependencyProperty property, object defaultValue=null, PropertyChangedCallback valueChanged=null, CoerceValueCallback coerceValue=null)
			where OWNER : DependencyObject
		{
			property.OverrideMetadata(typeof(OWNER), new FrameworkPropertyMetadata(defaultValue ?? property.DefaultMetadata.DefaultValue, valueChanged, coerceValue));
		}

		public static void OverrideReadOnlyProperty<OWNER>(DependencyPropertyKey property, object defaultValue=null, PropertyChangedCallback valueChanged=null, CoerceValueCallback coerceValue=null)
			where OWNER : DependencyObject
		{
			property.OverrideMetadata(typeof(OWNER), new FrameworkPropertyMetadata(defaultValue ?? property.DependencyProperty.DefaultMetadata.DefaultValue, valueChanged, coerceValue));
		}

		public static void AddPropertyOwner<OWNER>(DependencyProperty property, object defaultValue=null, PropertyChangedCallback valueChanged=null, CoerceValueCallback coerceValue=null)
			where OWNER : DependencyObject
		{
			property.AddOwner(typeof(OWNER), new FrameworkPropertyMetadata(defaultValue ?? property.DefaultMetadata.DefaultValue, valueChanged, coerceValue));
		}

		public object GetValue(DependencyPropertyKey property) { return GetValue(property.DependencyProperty); }

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
		{
			base.OnPropertyChanged(args);

			if(PropertyChanged != null)
				PropertyChanged(args);
		}
		public event Action<DependencyPropertyChangedEventArgs> PropertyChanged;
	}
}	
