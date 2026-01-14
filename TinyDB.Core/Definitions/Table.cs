using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyDB.Core.Definitions
{
    public class Table
    {
        public string Name { get; }
        public List<Column> Columns { get; }
        public List<object[]> Rows { get; } // A row is just an array of objects

        public Table(string name)
        {
            Name = name;
            Columns = new List<Column>();
            Rows = new List<object[]>();
        }

        public void AddColumn(string name, ColumnType type)
        {
            if (Columns.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Column '{name}' already exists.");

            Columns.Add(new Column(name, type));
        }

        public void InsertRow(object[] values)
        {
            // 1. Validation: Column count match
            if (values.Length != Columns.Count)
                throw new ArgumentException($"Expected {Columns.Count} values, got {values.Length}.");

            // 2. Validation: Type check
            for (int i = 0; i < Columns.Count; i++)
            {
                var column = Columns[i];
                var value = values[i];

                if (!IsValidType(value, column.Type))
                    throw new ArgumentException($"Column '{column.Name}' expects {column.Type}, got {value?.GetType().Name ?? "null"}.");
            }

            // 3. Insert
            Rows.Add(values);
        }

        private bool IsValidType(object value, ColumnType type)
        {
            if (value == null) return false; // We don't support NULLs yet for simplicity

            return type switch
            {
                ColumnType.Integer => value is int,
                ColumnType.String => value is string,
                ColumnType.Boolean => value is bool,
                _ => false
            };
        }
    }
}