using System;
using System.Collections.Generic;
using System.Linq;
using TinyDB.Core.Definitions;
using TinyDB.Core.Execution;
using TinyDB.Core.Storage;

namespace TinyDB.Core.Parsing
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private readonly Engine _engine;
        private int _position;

        public Parser(List<Token> tokens, Engine engine)
        {
            _tokens = tokens;
            _engine = engine;
            _position = 0;
        }

        public ExecutionResult Parse()
        {
            if (Match(TokenType.EOF)) return new ExecutionResult("Empty query.");

            if (Match(TokenType.CREATE)) return ParseCreateTable();
            if (Match(TokenType.INSERT)) return ParseInsert();
            if (Match(TokenType.SELECT)) return ParseSelect();

            throw new Exception($"Unexpected token: {Peek().Type}");
        }

        // --------------------------------------------------------
        // HANDLER: CREATE TABLE
        // Syntax: CREATE TABLE <Name> ( <Col> <Type>, ... )
        // --------------------------------------------------------
        private ExecutionResult ParseCreateTable()
        {
            Consume(TokenType.TABLE, "Expected 'TABLE' after CREATE");

            var tableName = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            var table = _engine.CreateTable(tableName);

            Consume(TokenType.OPEN_PAREN, "Expected '(' after table name");

            // Loop for columns
            do
            {
                var colName = Consume(TokenType.IDENTIFIER, "Expected column name").Value;
                var typeToken = Advance();

                ColumnType colType = typeToken.Type switch
                {
                    TokenType.INT_TYPE => ColumnType.Integer,
                    TokenType.STRING_TYPE => ColumnType.String,
                    TokenType.BOOL_TYPE => ColumnType.Boolean,
                    _ => throw new Exception($"Unknown column type: {typeToken.Value}")
                };

                table.AddColumn(colName, colType);

            } while (Match(TokenType.COMMA)); // Continue if comma is found

            Consume(TokenType.CLOSE_PAREN, "Expected ')' after column definitions");

            return new ExecutionResult($"Table '{tableName}' created successfully.");
        }

        // --------------------------------------------------------
        // HANDLER: INSERT
        // Syntax: INSERT INTO <Name> VALUES ( <Val>, ... )
        // --------------------------------------------------------
        private ExecutionResult ParseInsert()
        {
            Consume(TokenType.INTO, "Expected 'INTO' after INSERT");
            var tableName = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            var table = _engine.GetTable(tableName);

            Consume(TokenType.VALUES, "Expected 'VALUES'");
            Consume(TokenType.OPEN_PAREN, "Expected '('");

            var values = new List<object>();

            do
            {
                var token = Advance();
                object val = token.Type switch
                {
                    TokenType.INTEGER_LITERAL => int.Parse(token.Value),
                    TokenType.STRING_LITERAL => token.Value,
                    TokenType.BOOLEAN_LITERAL => bool.Parse(token.Value),
                    _ => throw new Exception($"Unexpected value: {token.Value}")
                };
                values.Add(val);

            } while (Match(TokenType.COMMA));

            Consume(TokenType.CLOSE_PAREN, "Expected ')'");

            table.InsertRow(values.ToArray());

            return new ExecutionResult("1 row inserted.");
        }

        // --------------------------------------------------------
        // HANDLER: SELECT
        // Syntax: SELECT <Cols> FROM <Table>
        // --------------------------------------------------------
        private ExecutionResult ParseSelect()
        {
            var requestedColumns = new List<string>();
            bool selectAll = false;

            // 1. Parse Column List
            if (Match(TokenType.STAR))
            {
                selectAll = true;
            }
            else
            {
                do
                {
                    requestedColumns.Add(Consume(TokenType.IDENTIFIER, "Expected column name").Value);
                } while (Match(TokenType.COMMA));
            }

            // 2. Parse FROM
            Consume(TokenType.FROM, "Expected 'FROM'");
            var tableName = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            var table = _engine.GetTable(tableName);

            // 3. Execute
            // If SELECT *, get all column names from table
            if (selectAll)
            {
                requestedColumns = table.Columns.Select(c => c.Name).ToList();
            }

            // Map the rows to the requested columns
            // We need to find the index of each requested column in the table definition
            var indices = new List<int>();
            foreach (var colName in requestedColumns)
            {
                int index = table.Columns.FindIndex(c => c.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));
                if (index == -1) throw new Exception($"Column '{colName}' does not exist in table '{tableName}'");
                indices.Add(index);
            }

            // Build result set
            var resultRows = new List<object[]>();
            foreach (var row in table.Rows)
            {
                var resultRow = new object[indices.Count];
                for (int i = 0; i < indices.Count; i++)
                {
                    resultRow[i] = row[indices[i]];
                }
                resultRows.Add(resultRow);
            }

            return new ExecutionResult(requestedColumns, resultRows);
        }

        // --------------------------------------------------------
        // HELPERS
        // --------------------------------------------------------

        private Token Peek() => _position < _tokens.Count ? _tokens[_position] : _tokens.Last();

        private Token Advance()
        {
            var token = Peek();
            if (_position < _tokens.Count) _position++;
            return token;
        }

        private bool Match(TokenType type)
        {
            if (Peek().Type == type)
            {
                Advance();
                return true;
            }
            return false;
        }

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Peek().Type == type) return Advance();
            throw new Exception(errorMessage);
        }
    }
}