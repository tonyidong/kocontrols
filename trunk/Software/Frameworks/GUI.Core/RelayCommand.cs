using System;
using System.Windows.Input;

namespace KOControls.GUI.Core
{
	public class RelayCommand : CommandBase
	{
		public RelayCommand(ICommand command)
		{
			if(command == null) throw new ArgumentNullException("command");

			_command = command;
		}
		private readonly ICommand _command;

		public override bool CanExecute(object parameter) { return _command.CanExecute(parameter); }
		public override void Execute(object parameter) { _command.Execute(parameter); }
	}
}
