using System.Collections.Generic;

namespace TinyDB.Core.Execution
{
    public class ExecutionResult
    {
        public string Message { get; }
        public List<string> Columns { get; }
        public List<object[]> Rows { get; }
        public bool IsQuery { get; } // True for SELECT, False for INSERT/CREATE

        // Constructor for commands (CREATE, INSERT)
        public ExecutionResult(string message)
        {
            Message = message;
            Columns = new List<string>();
            Rows = new List<object[]>();
            IsQuery = false;
        }

        // Constructor for queries (SELECT)
        public ExecutionResult(List<string> columns, List<object[]> rows)
        {
            Message = $"Returned {rows.Count} rows.";
            Columns = columns;
            Rows = rows;
            IsQuery = true;
        }
    }
}