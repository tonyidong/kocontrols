using System;
using System.Collections.Generic;
using System.Windows.Input;
using KOControls.Core;

namespace KOControls.GUI.Core
{
	/// <summary>
	/// Interface for a control which provides a list of text tokens
	/// </summary>
	public interface ITextTokensControl
	{
		TextToken CurrentToken { get; }
		event Action<CurrentTokenChangedArgs> CurrentTokenChanged;

		string GetFilterText();

		void InsertAutoCompleteText(string value);
		void ReplaceTargetTokenText(string value);

		bool Focused { get; set; }

		bool IsCursorAtTheEndOfText();
		bool IsCurosorAtTheBegginingOfText();
		event KeyEventHandler PreviewKeyDown;
		event KeyboardFocusChangedEventHandler GotKeyboardFocus;
		event KeyboardFocusChangedEventHandler LostKeyboardFocus;
		event MouseButtonEventHandler PreviewMouseDown;
	}

	#region EventArgs
	public class TokensChangedArgs : EventArgs
	{
		public ITextTokensControl Target { get; private set; }

		public IList<TextToken> RemovedTokens { get; private set; }
		public IList<TextToken> AddedTokens { get; private set; }

		public TokensChangedArgs(ITextTokensControl target, IList<TextToken> removedTokens, IList<TextToken> addedTokens)
		{
			Target = target;

			RemovedTokens = removedTokens;
			AddedTokens = addedTokens;
		}
	}
	public class CurrentTokenChangedArgs : EventArgs
	{
		public ITextTokensControl Target { get; private set; }

		[Flags]
		public enum ChangeTypes
		{
			None = 0,
			Add = 1,
			Remove = 2,
			Replace = Add | Remove,
			CarretChange = 4
		}
		public ChangeTypes ChangeType { get; private set; }

		public TextToken OldValue { get; private set; }
		public TextToken NewValue { get; private set; }

		public CurrentTokenChangedArgs(ITextTokensControl target, TextToken oldValue, TextToken newValue, ChangeTypes changeType)
		{
			Target = target;
			OldValue = oldValue;
			NewValue = newValue;
			ChangeType = changeType;
		}
	}
	#endregion
}
