using System;
using System.Collections.Generic;
using System.Text;

namespace TinyDB.Core.Parsing
{
    public class Tokenizer
    {
        private readonly string _text;
        private int _position;

        private static readonly Dictionary<string, TokenType> _keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            { "CREATE", TokenType.CREATE },
            { "TABLE", TokenType.TABLE },
            { "INSERT", TokenType.INSERT },
            { "INTO", TokenType.INTO },
            { "VALUES", TokenType.VALUES },
            { "SELECT", TokenType.SELECT },
            { "FROM", TokenType.FROM },
            { "WHERE", TokenType.WHERE },
            { "INT", TokenType.INT_TYPE },
            { "STRING", TokenType.STRING_TYPE },
            { "BOOL", TokenType.BOOL_TYPE },
            { "TRUE", TokenType.BOOLEAN_LITERAL },
            { "FALSE", TokenType.BOOLEAN_LITERAL },
            { "PRIMARY", TokenType.PRIMARY },
            { "KEY", TokenType.KEY },
            { "JOIN", TokenType.JOIN },
            { "ON", TokenType.ON },
            { "UPDATE", TokenType.UPDATE },
            { "SET", TokenType.SET },
            { "DELETE", TokenType.DELETE }
        };

        public Tokenizer(string text)
        {
            _text = text;
            _position = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _text.Length)
            {
                char current = _text[_position];

                // 1. Skip Whitespace
                if (char.IsWhiteSpace(current))
                {
                    _position++;
                    continue;
                }

                // 2. Symbols
                if (current == '(') { tokens.Add(new Token(TokenType.OPEN_PAREN, "(", _position++)); continue; }
                if (current == ')') { tokens.Add(new Token(TokenType.CLOSE_PAREN, ")", _position++)); continue; }
                if (current == ',') { tokens.Add(new Token(TokenType.COMMA, ",", _position++)); continue; }
                if (current == '*') { tokens.Add(new Token(TokenType.STAR, "*", _position++)); continue; }
                if (current == ';') { tokens.Add(new Token(TokenType.SEMICOLON, ";", _position++)); continue; }
                if (current == '.') { tokens.Add(new Token(TokenType.DOT, ".", _position++)); continue; }
                if (current == '=') { tokens.Add(new Token(TokenType.EQUALS, "=", _position++)); continue; }

                // 3. String Literals (starts with single quote ')
                if (current == '\'')
                {
                    tokens.Add(ReadStringLiteral());
                    continue;
                }

                // 4. Numbers (Integers)
                if (char.IsDigit(current))
                {
                    tokens.Add(ReadInteger());
                    continue;
                }

                // 5. Identifiers or Keywords (starts with letter)
                if (char.IsLetter(current) || current == '_')
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                    continue;
                }

                throw new Exception($"Unexpected character '{current}' at position {_position}");
            }

            tokens.Add(new Token(TokenType.EOF, null, _position));
            return tokens;
        }

        private Token ReadStringLiteral()
        {
            int start = _position;
            _position++; // Skip opening quote

            var sb = new StringBuilder();
            while (_position < _text.Length && _text[_position] != '\'')
            {
                sb.Append(_text[_position]);
                _position++;
            }

            if (_position >= _text.Length)
                throw new Exception("Unterminated string literal");

            _position++; // Skip closing quote
            return new Token(TokenType.STRING_LITERAL, sb.ToString(), start);
        }

        private Token ReadInteger()
        {
            int start = _position;
            while (_position < _text.Length && char.IsDigit(_text[_position]))
            {
                _position++;
            }

            string value = _text.Substring(start, _position - start);
            return new Token(TokenType.INTEGER_LITERAL, value, start);
        }

        private Token ReadIdentifierOrKeyword()
        {
            int start = _position;
            while (_position < _text.Length && (char.IsLetterOrDigit(_text[_position]) || _text[_position] == '_'))
            {
                _position++;
            }

            string text = _text.Substring(start, _position - start);

            if (_keywords.TryGetValue(text, out var type))
            {
                // Special handling: True/False are literals, not just keywords
                if (type == TokenType.BOOLEAN_LITERAL)
                {
                    return new Token(TokenType.BOOLEAN_LITERAL, text.ToLower(), start);
                }
                return new Token(type, text.ToUpper(), start);
            }

            return new Token(TokenType.IDENTIFIER, text, start);
        }
    }
}