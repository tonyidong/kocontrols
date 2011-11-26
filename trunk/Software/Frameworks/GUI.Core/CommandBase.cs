using System;
using System.Windows;
using System.Windows.Input;
using KOControls.Core;

namespace KOControls.GUI.Core
{
	public abstract class CommandBase : ViewModel, ICommand
	{
		#region Header
		public static readonly DependencyProperty HeaderProperty = RegisterProperty<object, CommandBase>("Header");
		public object Header { get { return GetValue(HeaderProperty); } set { SetValue(HeaderProperty, value); } }
		#endregion


		#region Visible
		public static readonly DependencyProperty VisibleProperty = RegisterProperty<bool, CommandBase>("Visible");
		public bool Visible { get { return (bool)GetValue(VisibleProperty); } set { SetValue(VisibleProperty, value); } }
		#endregion

		#region Enabled
		public static readonly DependencyProperty EnabledProperty = RegisterProperty<bool, CommandBase>("Enabled");
		public bool Enabled { get { return (bool)GetValue(EnabledProperty); } set { SetValue(EnabledProperty, value); } }
		#endregion


		protected CommandBase(object header = null) { Header = header; }

		public abstract void Execute(object parameter=null);
		public abstract bool CanExecute(object parameter=null);
		public virtual event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
	}
}
