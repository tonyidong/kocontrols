using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using KOControls.Core;

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
			if(_window != null)
			{
				_window.PreviewKeyDown -= Window_PreviewKeyDown;
				_window.Deactivated -= Window_Deactivated;
				_window.LocationChanged -= Window_LocationChanged;
			}
		}
		private void WindowAttachEvents()
		{
			WindowDetachEvents();
		
			_window = Window.GetWindow(this);
			if(_window != null)
			{
				_window.PreviewKeyDown += Window_PreviewKeyDown;
				_window.Deactivated += Window_Deactivated;
				_window.LocationChanged += Window_LocationChanged;
			}
		}

		private void Window_LocationChanged(object sender, EventArgs e)
		{
			HorizontalOffset += 0.01;
			HorizontalOffset -= 0.01;
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			IsOpen = false;
		}
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(_window != null) _window.Activate();

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
		private Window _window;
		private bool _cancelWindowKeyDown;
	}
}
