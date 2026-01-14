namespace TinyDB.Core.Parsing
{
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; } // Good for error reporting (e.g. "Error at char 5")

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public override string ToString()
        {
            return $"Token({Type}, '{Value}')";
        }
    }
}