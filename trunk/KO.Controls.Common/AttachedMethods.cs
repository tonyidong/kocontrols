using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace KO.Controls.Common
{
	public static class AttachedMethods
	{
		public static void AddValueChanged(this DependencyObject target, DependencyProperty property, EventHandler handler)
		{
			DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(property, target.GetType());
			descriptor.AddValueChanged(target, handler);
		}
		public static void AddValueChanged(this DependencyObject target, DependencyPropertyKey property, EventHandler handler)
		{
			DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(property.DependencyProperty, target.GetType());
			descriptor.AddValueChanged(target, handler);
		}
		public static void RemoveValueChanged(this DependencyObject target, DependencyProperty property, EventHandler handler)
		{
			DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(property, target.GetType());
			descriptor.RemoveValueChanged(target, handler);
		}
		public static void RemoveValueChanged(this DependencyObject target, DependencyPropertyKey property, EventHandler handler)
		{
			DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(property.DependencyProperty, target.GetType());
			descriptor.RemoveValueChanged(target, handler);
		}
	}
}
