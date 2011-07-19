using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace KO.Controls.Common
{
	public static class ExtensionMethods
	{
		public static DependencyObject GetVisualParent(this DependencyObject elem)
		{
			if(elem == null) throw new ArgumentNullException();

			if(elem is Visual || elem is Visual3D)
			{
				DependencyObject prnt = VisualTreeHelper.GetParent(elem);
				if(prnt != null) return prnt;

				if(elem is FrameworkElement)
				{
					prnt = ((FrameworkElement)elem).TemplatedParent;
					if(prnt != null) return prnt;
				}
				else if(elem is FrameworkContentElement)
				{
					prnt = ((FrameworkContentElement)elem).TemplatedParent;
					if(prnt != null) return prnt;
				}
				}
				else
				{
				DependencyObject prnt = LogicalTreeHelper.GetParent(elem);
				if(prnt != null)
				{
					if(prnt is Visual || prnt is Visual3D) return prnt;
					return GetVisualParent(prnt);//this is logical prnt, keep looking for visual prnt.
				}
				}
          
			DependencyObject prnt2 = ItemsControl.ItemsControlFromItemContainer(elem);
			if(prnt2 != null) return prnt2;

			return null;
		}

		public static T GetVisualParent<T>(this DependencyObject elem) where T : DependencyObject
		{
			if(elem == null) throw new ArgumentNullException();

			if(elem is Visual || elem is Visual3D)
			{
				DependencyObject prnt = VisualTreeHelper.GetParent(elem);
				if(prnt is T) return (T)prnt;
				if(prnt is Visual || prnt is Visual3D)
					return GetVisualParent<T>(prnt);

				if(elem is FrameworkElement)
				{
					prnt = ((FrameworkElement)elem).TemplatedParent;
					if(prnt is T) return (T)prnt;
					if(prnt is Visual || prnt is Visual3D) return GetVisualParent<T>(prnt);
				}
				else if(elem is FrameworkContentElement)
				{
					prnt = ((FrameworkContentElement)elem).TemplatedParent;
					if(prnt is T) return (T)prnt;
					if(prnt is Visual || prnt is Visual3D) return GetVisualParent<T>(prnt);
				}
			 }
			 else
			 {
				var prnt = LogicalTreeHelper.GetParent(elem);
				if(prnt != null)
				{
					if(prnt is T) return (T)prnt;
					if(prnt is Visual || prnt is Visual3D) return GetVisualParent<T>(prnt);
					return GetVisualParent<T>(prnt);//this is logical prnt, keep looking for visual prnt.
				}
			 }

			DependencyObject prnt2 = ItemsControl.ItemsControlFromItemContainer(elem);
			if(prnt2 is T) return (T)(object)prnt2;
			if(prnt2 != null) return GetVisualParent<T>(prnt2);
    
			return null;
		}

		public static bool VisualChildOf(this DependencyObject elem, DependencyObject parent)
		{
			if (elem == null) throw new ArgumentNullException();
			if (parent == null) return false;

			if (elem == parent) return true;

			if (elem is Visual || elem is Visual3D)
			{
				DependencyObject prnt = VisualTreeHelper.GetParent(elem);
				if (prnt == parent)
					return true;
				if (prnt is Visual || prnt is Visual3D)
					return VisualChildOf(prnt, parent);

				if (elem is FrameworkElement)
				{
					prnt = ((FrameworkElement)elem).TemplatedParent;
					if (prnt == parent)
						return true;
					if (prnt is Visual || prnt is Visual3D)
						return VisualChildOf(prnt, parent);
				}
				else if (elem is FrameworkContentElement)
				{
					prnt = ((FrameworkContentElement)elem).TemplatedParent;
					if (prnt == parent)
						return true;
					if (prnt is Visual || prnt is Visual3D)
						return VisualChildOf(prnt, parent);
				}
			}

			{
				DependencyObject prnt = LogicalTreeHelper.GetParent(elem);
				if (prnt != null)
				{
					if (prnt == parent) 
						return true;
					if (prnt is Visual || prnt is Visual3D) 
						return VisualChildOf(prnt, parent);
					return VisualChildOf(prnt, parent);//this is logical prnt, keep looking for visual prnt.
				}
			}

			{
				DependencyObject prnt = ItemsControl.ItemsControlFromItemContainer(elem);
				if (prnt == parent) 
					return true;
				if (prnt != null) 
					return VisualChildOf(prnt, parent);
			}

			return false;
		}

		public static bool MakeFocused(this DependencyObject elem)
		{
			if (elem == null) throw new ArgumentNullException();

			if (elem is IInputElement)
			{
				var inputElement = (IInputElement)elem;
				if (inputElement.IsKeyboardFocused || inputElement.IsKeyboardFocusWithin) return true;
			}

			if (FocusManager.GetIsFocusScope(elem))
			{
				DependencyObject fe = FocusManager.GetFocusedElement(elem) as DependencyObject;
				if (fe != null && MakeFocused(fe)) return true;
			}
			if (elem is IInputElement)
			{
				IInputElement inputElement = (IInputElement)elem;

				Keyboard.Focus(inputElement);
				if (inputElement.IsKeyboardFocused || inputElement.IsKeyboardFocusWithin) return true;
			}

			if (elem is Selector)
			{
				Selector items = (Selector)elem;

				if (items.SelectedIndex >= 0)
				{
					FrameworkElement item = (FrameworkElement)items.ItemContainerGenerator.ContainerFromIndex(items.SelectedIndex);
					if (item != null)
					{
						item.BringIntoView();
						if (item.MakeFocused())
							return true;
					}
				}
				else
				{
					for (var i = 0; i < items.Items.Count; ++i)
					{
						FrameworkElement item = (FrameworkElement)items.ItemContainerGenerator.ContainerFromIndex(i);
						if (item != null)
						{
							item.BringIntoView();
							if (item.MakeFocused()) return true;
						}
					}
				}

				return false;
			}
			else
			{
				List<DependencyObject> children = new List<DependencyObject>();
				foreach (var next in LogicalTreeHelper.GetChildren(elem))
				{
					if (next is DependencyObject)
						children.Add((DependencyObject)next);
				}

				if (elem is Visual)
				{
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount(elem); ++i)
					{
						DependencyObject next = VisualTreeHelper.GetChild(elem, i);
						if (!children.Contains(next))
							children.Add(next);
					}
				}

				if (children.Count > 0)
				{
					for (int i = 0; i < children.Count; ++i)
					{
						DependencyObject nexti = children[i];
						int nextiti = KeyboardNavigation.GetTabIndex(nexti);
						for (int j = i - 1; j >= 0; --j)
						{
							DependencyObject nextj = children[j];
							int nextjti = KeyboardNavigation.GetTabIndex(nextj);

							if (nextiti < nextjti)
							{
								children[j + 1] = nextj;
								children[j] = nexti;
							}
						}
					}

					for (int i = 0; i < children.Count; ++i)
					{
						DependencyObject next = children[i];

						if (next.MakeFocused())
							return true;
					}
				}
				return false;
			}
		}
	}
}
