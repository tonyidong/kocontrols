using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace KO.Controls.Common.Command
{
	public class CommandViewModel
	{
		public CommandViewModel(string displayName, ICommand command)
		{
			this.DisplayName = displayName;
			this.Command = command;
		}

		public string DisplayName { get; private set; }
		public ICommand Command { get; private set; }
	}
}
