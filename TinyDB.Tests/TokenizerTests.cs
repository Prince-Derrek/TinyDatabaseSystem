using Xunit;
using TinyDB.Core.Parsing;
using System.Linq;

namespace TinyDB.Tests
{
    public class TokenizerTests
    {
        [Fact]
        public void Tokenize_Simple_Select()
        {
            string input = "SELECT * FROM Users";
            var tokenizer = new Tokenizer(input);
            var tokens = tokenizer.Tokenize();

            Assert.Equal(TokenType.SELECT, tokens[0].Type);
            Assert.Equal(TokenType.STAR, tokens[1].Type);
            Assert.Equal(TokenType.FROM, tokens[2].Type);
            Assert.Equal(TokenType.IDENTIFIER, tokens[3].Type);
            Assert.Equal("Users", tokens[3].Value);
            Assert.Equal(TokenType.EOF, tokens.Last().Type);
        }

        [Fact]
        public void Tokenize_Insert_With_Literals()
        {
            string input = "INSERT INTO Users VALUES (1, 'Derrek')";
            var tokenizer = new Tokenizer(input);
            var tokens = tokenizer.Tokenize();

            Assert.Contains(tokens, t => t.Type == TokenType.INTEGER_LITERAL && t.Value == "1");
            Assert.Contains(tokens, t => t.Type == TokenType.STRING_LITERAL && t.Value == "Derrek");
        }
    }
}