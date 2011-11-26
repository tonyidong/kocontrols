using System;

namespace KOControls.GUI.Core
{
	public class Command : CommandBase
	{
		public Command(Action<object> execute, Predicate<object> canExecute=null, object header=null)
			: base(header)
		{
			if(execute == null) throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}
		private readonly Action<object> _execute;
		private readonly Predicate<object> _canExecute;

		public override bool CanExecute(object parameter=null) { return _canExecute == null || _canExecute(parameter); }
		public override void Execute(object parameter=null) { _execute(parameter); }
	}
}
