using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace KOControls.Core
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Assigns the Binding to the desired property on the target object.
		/// </summary>
		public static void ApplyBinding(this DependencyObject target, DependencyProperty property, BindingBase binding=null)
		{
			if(binding == null) BindingOperations.ClearBinding(target, property);
			else BindingOperations.SetBinding(target, property, binding);
		}

		public static void SetBinding(this DependencyObject target, DependencyProperty targetProperty, DependencyObject source, DependencyProperty sourceProperty = null, BindingMode mode = BindingMode.TwoWay)
		{
			BindingOperations.SetBinding(target, targetProperty, new Binding() { Path=new PropertyPath(sourceProperty ?? targetProperty), Source = source, Mode = mode });
		}

		public static void AddValueChanged(this DependencyObject target, DependencyProperty property, EventHandler handler)
		{
			var descriptor = DependencyPropertyDescriptor.FromProperty(property, target.GetType());
			descriptor.AddValueChanged(target, handler);
		}
		public static void AddValueChanged(this DependencyObject target, DependencyPropertyKey property, EventHandler handler)
		{
			var descriptor = DependencyPropertyDescriptor.FromProperty(property.DependencyProperty, target.GetType());
			descriptor.AddValueChanged(target, handler);
		}
		public static void RemoveValueChanged(this DependencyObject target, DependencyProperty property, EventHandler handler)
		{
			var descriptor = DependencyPropertyDescriptor.FromProperty(property, target.GetType());
			descriptor.RemoveValueChanged(target, handler);
		}
		public static void RemoveValueChanged(this DependencyObject target, DependencyPropertyKey property, EventHandler handler)
		{
			var descriptor = DependencyPropertyDescriptor.FromProperty(property.DependencyProperty, target.GetType());
			descriptor.RemoveValueChanged(target, handler);
		}

		public static object FirstOrNull(this IEnumerable target) { return target == null ? null : target.Cast<object>().FirstOrDefault(); }
		public static int Count(this IEnumerable target) { return target == null ? 0 : Enumerable.Count(target.Cast<object>()); }

		public static bool IsNullOrWhiteSpace(this string str)
		{
			return String.IsNullOrEmpty(str) || String.IsNullOrEmpty(str.Trim());
		}
	}
}
