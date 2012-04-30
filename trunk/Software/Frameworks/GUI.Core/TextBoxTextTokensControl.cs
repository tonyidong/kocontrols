using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using KOControls.Core;

namespace KOControls.GUI.Core
{
	public class TextBoxTextTokensControl : ITextTokensControl
	{
		public TextBoxTextTokensControl(TextBox textBox)
		{
			_textBox = textBox;

			_textBox.TextChanged += TextBox_TextChanged;

			_textBox.PreviewMouseDown += _textBox_PreviewMouseDown;
			_textBox.GotKeyboardFocus += _textBox_GotKeyboardFocus;

			var token = new TextToken(_textBox.Text + "", TextTokenType.Text);
			var currentToken = new TextBoxTextToken(token);

			SetCurrentToken(currentToken, CurrentTokenChangedArgs.ChangeTypes.Add);
		}
		private readonly TextBox _textBox;

		#region CurrentToken
		public TextToken CurrentToken { get { return _currentToken == null ? null : _currentToken.TextToken; } }
		private void SetCurrentToken(TextBoxTextToken newValue, CurrentTokenChangedArgs.ChangeTypes changeType)
		{
			var oldValue = _currentToken;
			_currentToken = newValue;

			var oldToken = oldValue == null ? null : oldValue.TextToken;
			var newToken = newValue == null ? null : newValue.TextToken;
			if (oldToken != newToken)
				OnCurrentTokenChanged(oldToken, newToken, changeType);
		}
		private TextBoxTextToken _currentToken;

		public event Action<CurrentTokenChangedArgs> CurrentTokenChanged;
		protected virtual void OnCurrentTokenChanged(TextToken oldValue, TextToken newValue, CurrentTokenChangedArgs.ChangeTypes changeType)
		{
			var currentTokenChanged = CurrentTokenChanged;
			if (currentTokenChanged == null) return;

			currentTokenChanged(new CurrentTokenChangedArgs(this, oldValue, newValue, changeType));
		}
		#endregion

		#region InsertAutoCompleteText
		public void InsertAutoCompleteText(string value)
		{
			if (_currentToken == null || _textBox.CaretIndex < _currentToken.EndIndex)
				return;

			try
			{
				_insertingAutoCompleteText = true;

				_textBox.Text = _currentToken.TextToken.Text + value;

				_textBox.Select(_currentToken.EndIndex, value.Length);
			}
			finally { _insertingAutoCompleteText = false; }

			_currentToken.SetTextToken(this, new TextToken(_currentToken.TextToken.Text + value, _currentToken.TextToken.TokenType), CurrentTokenChangedArgs.ChangeTypes.Add);
		}
		private bool _insertingAutoCompleteText;
		#endregion

		public void ReplaceTargetTokenText(string value)
		{
			if (_currentToken == null)
				return;

			try
			{
				_insertingAutoCompleteText = true;

				_textBox.Text = value;
			}
			finally { _insertingAutoCompleteText = false; }

			_currentToken.SetTextToken(this, new TextToken(value, _currentToken.TextToken.TokenType), CurrentTokenChangedArgs.ChangeTypes.Add);
			_textBox.CaretIndex = _currentToken.EndIndex;
		}

		#region GetFilterText
		public string GetFilterText()
		{
			if (_currentToken == null) return "";
			if (String.IsNullOrEmpty(_currentToken.TextToken.Text)) return "";

			if ((_textBox.SelectionStart + _textBox.SelectionLength) == _currentToken.EndIndex)
				return _currentToken.TextToken.Text.Substring(0, _textBox.SelectionStart - _currentToken.StartIndex);

			return _currentToken.TextToken.Text;
		}
		#endregion

		#region Focused
		public bool Focused
		{
			get { return _textBox.IsKeyboardFocused; }
			set
			{
				if (value == _textBox.IsKeyboardFocused) return;

				if (value) _textBox.Focus();
				else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
					_textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
				else
					_textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			}
		}
		#endregion

		public bool IsCursorAtTheEndOfText()
		{
			return _textBox.SelectionLength == 0 && _textBox.CaretIndex == _textBox.Text.Length;
		}
		public bool IsCurosorAtTheBegginingOfText()
		{
			return _textBox.CaretIndex == 0 && _textBox.SelectionLength == 0;
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (_insertingAutoCompleteText) return;

			var token = new TextToken(_textBox.Text, TextTokenType.Text);
			var currentToken = new TextBoxTextToken(token);

			CurrentTokenChangedArgs.ChangeTypes changeType;
			if (e.Changes.First().AddedLength > 0 && e.Changes.First().RemovedLength > 0)
				changeType = CurrentTokenChangedArgs.ChangeTypes.Replace;
			else if (e.Changes.First().AddedLength > 0)
				changeType = CurrentTokenChangedArgs.ChangeTypes.Add;
			else if (e.Changes.First().RemovedLength > 0)
				changeType = CurrentTokenChangedArgs.ChangeTypes.Remove;
			else
				changeType = CurrentTokenChangedArgs.ChangeTypes.None;

			SetCurrentToken(currentToken, changeType);
		}

		public event KeyEventHandler PreviewKeyDown
		{
			add { _textBox.PreviewKeyDown += value; }
			remove { _textBox.PreviewKeyDown -= value; }
		}
		public event KeyboardFocusChangedEventHandler GotKeyboardFocus
		{
			add { _textBox.GotKeyboardFocus += value; }
			remove { _textBox.GotKeyboardFocus -= value; }
		}
		public event KeyboardFocusChangedEventHandler LostKeyboardFocus
		{
			add { _textBox.LostKeyboardFocus += value; }
			remove { _textBox.LostKeyboardFocus -= value; }
		}
		public event MouseButtonEventHandler PreviewMouseDown
		{
			add { _textBox.PreviewMouseDown += value; }
			remove { _textBox.PreviewMouseDown -= value; }
		}

		private void _textBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Focused) return;

			_textBox.Dispatcher.BeginInvoke((Action)_textBox.SelectAll);
		}
		private void _textBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			_textBox.Dispatcher.BeginInvoke((Action)_textBox.SelectAll);
		}

		private class TextBoxTextToken
		{
			public TextBoxTextToken(TextToken token)
			{
				TextToken = token;
			}

			public TextToken TextToken { get; private set; }
			public void SetTextToken(TextBoxTextTokensControl owner, TextToken newValue, CurrentTokenChangedArgs.ChangeTypes changeType)
			{
				if (TextToken == newValue) return;

				var oldValue = TextToken;
				TextToken = newValue;
				if (owner._currentToken == this)
					owner.OnCurrentTokenChanged(oldValue, newValue, changeType);
			}

			public int StartIndex { get { return 0; } }
			public int EndIndex { get { return TextToken.Text.Length; } }
		}
	}
}
