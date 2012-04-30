namespace KOControls.Core
{
	#region TextTokenType
	public enum TextTokenType
	{
		Delimiter = 1,
		Text = 2
	}
	#endregion 

	public class TextToken
	{
		public string Text { get; private set; }
		public TextTokenType TokenType { get; private set; }

		public TextToken(string text, TextTokenType tokenType)
		{
			Text = text;
			TokenType = tokenType;
		}
	}
}