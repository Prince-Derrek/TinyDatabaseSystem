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
            if (Match(TokenType.DELETE)) return ParseDelete(); // NEW
            if (Match(TokenType.UPDATE)) return ParseUpdate(); // NEW

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

                bool isPk = false;
                if (Match(TokenType.PRIMARY))
                {
                    Consume(TokenType.KEY, "Expected 'KEY' after 'PRIMARY'");
                    isPk = true;
                }

                table.AddColumn(colName, colType, isPk);

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
            // 1. Parse Column List
            var requestedColumns = new List<string>();
            bool selectAll = false;

            if (Match(TokenType.STAR))
            {
                selectAll = true;
            }
            else
            {
                do { requestedColumns.Add(Consume(TokenType.IDENTIFIER, "Expected column").Value); }
                while (Match(TokenType.COMMA));
            }

            // 2. Parse FROM
            Consume(TokenType.FROM, "Expected 'FROM'");
            var tableAName = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            var tableA = _engine.GetTable(tableAName);

            // 3. CHECK FOR JOIN
            if (Match(TokenType.JOIN))
            {
                return ParseJoin(tableA, requestedColumns, selectAll);
            }

            // --- STANDARD SELECT (No Join) ---
            // (This logic remains largely the same as Phase 4, simplified for clarity)

            // Resolve columns
            if (selectAll) requestedColumns = tableA.Columns.Select(c => c.Name).ToList();

            var indices = GetColumnIndices(tableA, requestedColumns);
            var resultRows = new List<object[]>();

            foreach (var row in tableA.Rows)
            {
                var resultRow = new object[indices.Count];
                for (int i = 0; i < indices.Count; i++) resultRow[i] = row[indices[i]];
                resultRows.Add(resultRow);
            }

            return new ExecutionResult(requestedColumns, resultRows);
        }

        // 4. NEW: The Join Implementation
        private ExecutionResult ParseJoin(Definitions.Table tableA, List<string> requestedColumns, bool selectAll)
        {
            // Syntax: JOIN <TableB> ON <TableA.Col> = <TableB.Col>

            var tableBName = Consume(TokenType.IDENTIFIER, "Expected second table name").Value;
            var tableB = _engine.GetTable(tableBName);

            Consume(TokenType.ON, "Expected 'ON'");

            // Left side of condition (e.g., Users.Id)
            var leftTable = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            Consume(TokenType.DOT, "Expected '.'");
            var leftCol = Consume(TokenType.IDENTIFIER, "Expected column name").Value;

            Consume(TokenType.EQUALS, "Expected '='");

            // Right side of condition (e.g., Orders.UserId)
            var rightTable = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            Consume(TokenType.DOT, "Expected '.'");
            var rightCol = Consume(TokenType.IDENTIFIER, "Expected column name").Value;

            // Validate logic: We need to know which table is which to find the column indices
            // Simplification: We assume the user writes "ON TableA.Col = TableB.Col" (order matters for our simple parser)

            int indexA = tableA.Columns.FindIndex(c => c.Name.Equals(leftCol, StringComparison.OrdinalIgnoreCase));
            int indexB = tableB.Columns.FindIndex(c => c.Name.Equals(rightCol, StringComparison.OrdinalIgnoreCase));

            if (indexA == -1 || indexB == -1) throw new Exception("Join columns not found.");

            // Resolve Output Columns
            // If SELECT *, we combine columns from A and B
            List<string> finalColumns;
            if (selectAll)
            {
                finalColumns = new List<string>();
                finalColumns.AddRange(tableA.Columns.Select(c => $"{tableA.Name}.{c.Name}"));
                finalColumns.AddRange(tableB.Columns.Select(c => $"{tableB.Name}.{c.Name}"));
            }
            else
            {
                finalColumns = requestedColumns;
            }

            // Execute NESTED LOOP JOIN
            var resultRows = new List<object[]>();

            foreach (var rowA in tableA.Rows)
            {
                foreach (var rowB in tableB.Rows)
                {
                    var valA = rowA[indexA];
                    var valB = rowB[indexB];

                    // The Join Condition
                    if (valA.Equals(valB))
                    {
                        // Merge rows
                        // Note: This matches "SELECT *" behavior. 
                        // Handling specific columns in a join is complex, so we default to returning ALL columns for joined rows
                        // to satisfy the challenge constraints of simplicity.

                        var mergedRow = new object[rowA.Length + rowB.Length];
                        Array.Copy(rowA, 0, mergedRow, 0, rowA.Length);
                        Array.Copy(rowB, 0, mergedRow, rowA.Length, rowB.Length);

                        resultRows.Add(mergedRow);
                    }
                }
            }

            return new ExecutionResult(finalColumns, resultRows);
        }

        // --------------------------------------------------------
        // HANDLER: DELETE
        // Syntax: DELETE FROM <Table> WHERE <Col> = <Val>
        // --------------------------------------------------------
        private ExecutionResult ParseDelete()
        {
            Consume(TokenType.FROM, "Expected 'FROM'");
            var tableName = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            var table = _engine.GetTable(tableName);

            Consume(TokenType.WHERE, "Expected 'WHERE'");
            var colName = Consume(TokenType.IDENTIFIER, "Expected column name").Value;
            Consume(TokenType.EQUALS, "Expected '='");

            var valToken = Advance();
            object value = ParseValue(valToken);

            int count = table.DeleteRows(colName, value);
            return new ExecutionResult($"{count} rows deleted.");
        }

        // --------------------------------------------------------
        // HANDLER: UPDATE
        // Syntax: UPDATE <Table> SET <Col>=<Val>, ... WHERE <Col>=<Val>
        // --------------------------------------------------------
        private ExecutionResult ParseUpdate()
        {
            var tableName = Consume(TokenType.IDENTIFIER, "Expected table name").Value;
            var table = _engine.GetTable(tableName);

            Consume(TokenType.SET, "Expected 'SET'");

            var updates = new Dictionary<string, object>();
            do
            {
                var col = Consume(TokenType.IDENTIFIER, "Expected column").Value;
                Consume(TokenType.EQUALS, "Expected '='");
                var valToken = Advance();
                updates[col] = ParseValue(valToken);
            }
            while (Match(TokenType.COMMA));

            Consume(TokenType.WHERE, "Expected 'WHERE'");
            var whereCol = Consume(TokenType.IDENTIFIER, "Expected column").Value;
            Consume(TokenType.EQUALS, "Expected '='");
            var whereValToken = Advance();
            object whereVal = ParseValue(whereValToken);

            int count = table.UpdateRows(updates, whereCol, whereVal);
            return new ExecutionResult($"{count} rows updated.");
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
        private List<int> GetColumnIndices(Definitions.Table table, List<string> columns)
        {
            var indices = new List<int>();
            foreach (var col in columns)
            {
                int idx = table.Columns.FindIndex(c => c.Name.Equals(col, StringComparison.OrdinalIgnoreCase));
                if (idx == -1) throw new Exception($"Column '{col}' not found.");
                indices.Add(idx);
            }
            return indices;
        }
        private object ParseValue(Token token)
        {
            return token.Type switch
            {
                TokenType.INTEGER_LITERAL => int.Parse(token.Value),
                TokenType.STRING_LITERAL => token.Value,
                TokenType.BOOLEAN_LITERAL => bool.Parse(token.Value),
                _ => throw new Exception($"Unexpected value: {token.Value}")
            };
        }
    }
}