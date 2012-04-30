using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using KOControls.Core;
using KOControls.GUI.Core;

namespace KOControls.GUI
{
	/// <summary>
	/// Popup class attempts to fix some microsoft bugs regarding window activation etc.
	/// </summary>
	public class Popup : System.Windows.Controls.Primitives.Popup
	{
		static Popup()
		{
			ViewModel.OverrideProperty<Popup>(AllowsTransparencyProperty, true);
			ViewModel.OverrideProperty<Popup>(PopupAnimationProperty, PopupAnimation.Fade);
		}
		protected override void OnOpened(EventArgs e)
		{
			base.OnOpened(e);

			WindowAttachEvents();
		}
		protected override void OnClosed(EventArgs e)
		{
			WindowDetachEvents();

			base.OnClosed(e);
		}

		private void WindowDetachEvents()
		{
			if (Window != null)
			{
				Window.PreviewKeyDown -= Window_PreviewKeyDown;
				Window.PreviewMouseDown -= Window_PreviewMouseDown;
				Window.Deactivated -= Window_Deactivated;
				Window.LocationChanged -= Window_LocationChanged;
			}
		}
		private void WindowAttachEvents()
		{
			if (Window != null)
			{
				WindowDetachEvents();

				Window.PreviewKeyDown += Window_PreviewKeyDown;
				Window.PreviewMouseDown -= Window_PreviewMouseDown;
				Window.Deactivated += Window_Deactivated;
				Window.LocationChanged += Window_LocationChanged;
			}
		}

		private void Window_LocationChanged(object sender, EventArgs e)
		{
			IsOpen = false;
		}
		private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(!((DependencyObject)e.OriginalSource).VisualChildOf(this))
				IsOpen = false;
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			IsOpen = false;
		}
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			Window.Activate();

			if(e.Handled) return;

			Dispatcher.BeginInvoke((Action)delegate
			{
				if(!_cancelWindowKeyDown)
				{
					if(e.Key == Key.Escape)
					{
						IsOpen = false;
						e.Handled = true;
					}
				}
				_cancelWindowKeyDown = true;
			});
		}
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if(e.Key == Key.Escape)
			{
				IsOpen = false;
				_cancelWindowKeyDown = true;
			}
		}
		public Window Window { get { return _window ?? (_window = this.GetWindow()); } } private Window _window;
		private bool _cancelWindowKeyDown;
	}
}
