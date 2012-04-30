using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace KOControls.GUI.Core
{
	public static class ExtensionMethods
	{
		public static DependencyObject GetVisualParent(this DependencyObject elem)
		{
			if(elem == null) throw new ArgumentNullException();

			if(elem is Visual || elem is Visual3D)
			{
				var prnt = VisualTreeHelper.GetParent(elem);
				if(prnt != null) return prnt;

				if(elem is FrameworkElement)
				{
					prnt = ((FrameworkElement)elem).TemplatedParent;
					if(prnt != null) return prnt;
				}
			}
			else if(elem is FrameworkContentElement)
			{
				var prnt = ((FrameworkContentElement)elem).TemplatedParent;
				if(prnt != null) return prnt;
			}
			else
			{
				var prnt = LogicalTreeHelper.GetParent(elem);
				if(prnt != null)
				{
					if(prnt is Visual || prnt is Visual3D) return prnt;
					return GetVisualParent(prnt);//this is logical prnt, keep looking for visual prnt.
				}
			}

			var prnt2 = ItemsControl.ItemsControlFromItemContainer(elem);
			if(prnt2 != null) return prnt2;

			return null;
		}

		public static T GetVisualParent<T>(this DependencyObject elem) where T : DependencyObject
		{
			if(elem == null) throw new ArgumentNullException();

			if(elem is Visual || elem is Visual3D)
			{
				var prnt = VisualTreeHelper.GetParent(elem);
				if(prnt is T) return (T)prnt;
				if(prnt is Visual || prnt is Visual3D)
					return GetVisualParent<T>(prnt);

				if(elem is FrameworkElement)
				{
					prnt = ((FrameworkElement)elem).Parent ?? ((FrameworkElement)elem).TemplatedParent;
					if(prnt is T) return (T)prnt;
					if(prnt is Visual || prnt is Visual3D) return GetVisualParent<T>(prnt);
				}
			}
			else if(elem is FrameworkContentElement)
			{
				var prnt = ((FrameworkContentElement)elem).Parent ?? ((FrameworkContentElement)elem).TemplatedParent;
				if(prnt is T) return (T)prnt;
				if(prnt is Visual || prnt is Visual3D) return GetVisualParent<T>(prnt);
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

			var prnt2 = ItemsControl.ItemsControlFromItemContainer(elem);
			if(prnt2 is T) return (T)(object)prnt2;
			if(prnt2 != null) return GetVisualParent<T>(prnt2);

			return default(T);
		}

		public static bool VisualChildOf(this DependencyObject elem, DependencyObject parent)
		{
			if(elem == null) throw new ArgumentNullException();
			if(parent == null) return false;

			if(elem == parent) return true;

			if(elem is Visual || elem is Visual3D)
			{
				var prnt = VisualTreeHelper.GetParent(elem);
				if(prnt == parent)
					return true;
				if(prnt is Visual || prnt is Visual3D)
					return VisualChildOf(prnt, parent);

				if(elem is FrameworkElement)
				{
					prnt = ((FrameworkElement)elem).TemplatedParent;
					if(prnt == parent)
						return true;
					if(prnt is Visual || prnt is Visual3D)
						return VisualChildOf(prnt, parent);
				}
			}
			else if(elem is FrameworkContentElement)
			{
				var prnt = ((FrameworkContentElement)elem).TemplatedParent;
				if(prnt == parent)
					return true;
				if(prnt is Visual || prnt is Visual3D)
					return VisualChildOf(prnt, parent);
			}

			{
				var prnt = LogicalTreeHelper.GetParent(elem);
				if(prnt != null)
				{
					if(prnt == parent)
						return true;
					if(prnt is Visual || prnt is Visual3D)
						return VisualChildOf(prnt, parent);
					return VisualChildOf(prnt, parent);//this is logical prnt, keep looking for visual prnt.
				}
			}

			{
				var prnt = ItemsControl.ItemsControlFromItemContainer(elem);
				if(prnt == parent)
					return true;
				if(prnt != null)
					return VisualChildOf(prnt, parent);
			}

			return false;
		}

		public static bool MakeFocused(this DependencyObject elem)
		{
			if(elem == null) throw new ArgumentNullException();

			if(elem is IInputElement)
			{
				var inputElement = (IInputElement)elem;
				if(inputElement.IsKeyboardFocused || inputElement.IsKeyboardFocusWithin) return true;
			}

			if(FocusManager.GetIsFocusScope(elem))
			{
				var fe = FocusManager.GetFocusedElement(elem) as DependencyObject;
				if(fe != null && MakeFocused(fe)) return true;
			}
			if(elem is IInputElement)
			{
				var inputElement = (IInputElement)elem;

				Keyboard.Focus(inputElement);
				if(inputElement.IsKeyboardFocused || inputElement.IsKeyboardFocusWithin) return true;
			}

			if(elem is Selector)
			{
				var items = (Selector)elem;

				if(items.SelectedIndex >= 0)
				{
					var item = (FrameworkElement)items.ItemContainerGenerator.ContainerFromIndex(items.SelectedIndex);
					if(item != null)
					{
						item.BringIntoView();
						if(item.MakeFocused())
							return true;
					}
				}
				else
				{
					for(var i = 0; i < items.Items.Count; ++i)
					{
						var item = (FrameworkElement)items.ItemContainerGenerator.ContainerFromIndex(i);
						if(item != null)
						{
							item.BringIntoView();
							if(item.MakeFocused()) return true;
						}
					}
				}

				return false;
			}
			else
			{
				var children = new List<DependencyObject>();
				foreach(var next in LogicalTreeHelper.GetChildren(elem))
				{
					if(next is DependencyObject)
						children.Add((DependencyObject)next);
				}

				if(elem is Visual)
				{
					for(int i = 0; i < VisualTreeHelper.GetChildrenCount(elem); ++i)
					{
						DependencyObject next = VisualTreeHelper.GetChild(elem, i);
						if(!children.Contains(next))
							children.Add(next);
					}
				}

				if(children.Count > 0)
				{
					for(var i = 0; i < children.Count; ++i)
					{
						var nexti = children[i];
						var nextiti = KeyboardNavigation.GetTabIndex(nexti);
						for(var j = i - 1; j >= 0; --j)
						{
							var nextj = children[j];
							var nextjti = KeyboardNavigation.GetTabIndex(nextj);

							if(nextiti < nextjti)
							{
								children[j + 1] = nextj;
								children[j] = nexti;
							}
						}
					}

					for(var i = 0; i < children.Count; ++i)
					{
						var next = children[i];

						if(next.MakeFocused())
							return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Finds a Child of a given item in the visual tree. 
		/// </summary>
		/// <param name="parent">A direct parent of the queried item.</param>
		/// <typeparam name="T">The type of the queried item.</typeparam>
		/// <param name="childName">x:Name or Name of child. </param>
		/// <returns>The first parent item that matches the submitted type parameter. 
		/// If not matching item can be found, a null parent is being returned.</returns>
		public static T FindChild<T>(this DependencyObject parent, string childName)
		   where T : DependencyObject
		{
			// Confirm parent and childName are valid. 
			if(parent == null) return null;

			T foundChild = null;

			var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for(var i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				// If the child is not of the request child type child
				var childType = child as T;
				if(childType == null)
				{
					// recursively drill down the tree
					foundChild = FindChild<T>(child, childName);

					// If the child is found, break so we do not overwrite the found child. 
					if(foundChild != null) break;
				}
				else if(!string.IsNullOrEmpty(childName))
				{
					var frameworkElement = child as FrameworkElement;
					// If the child's name is set for search
					if(frameworkElement != null && frameworkElement.Name == childName)
					{
						// if the child's name is of the request name
						foundChild = (T)child;
						break;
					}
				}
				else
				{
					// child element found.
					foundChild = (T)child;
					break;
				}
			}

			return foundChild;
		}

		public static Window GetWindow(this DependencyObject elem)
		{
			return Window.GetWindow(elem) ?? GetVisualParent<Window>(elem) ?? Application.Current.MainWindow;
		}
	}
}
