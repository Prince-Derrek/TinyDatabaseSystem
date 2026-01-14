using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyDB.Core.Definitions
{
    public class Table
    {
        public string Name { get; }
        public List<Column> Columns { get; }
        public List<object[]> Rows { get; }

        private Dictionary<object, object[]> _primaryKeyIndex;
        private int? _primaryKeyColumnIndex; // Which column number is the PK?

        public Table(string name)
        {
            Name = name;
            Columns = new List<Column>();
            Rows = new List<object[]>();
            _primaryKeyIndex = new Dictionary<object, object[]>();
            _primaryKeyColumnIndex = null;
        }

        public void AddColumn(string name, ColumnType type, bool isPrimaryKey = false)
        {
            if (Columns.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Column '{name}' already exists.");

            if (isPrimaryKey)
            {
                if (_primaryKeyColumnIndex.HasValue)
                    throw new ArgumentException("Table can only have one Primary Key.");

                _primaryKeyColumnIndex = Columns.Count; // The current index
            }

            Columns.Add(new Column(name, type, isPrimaryKey));
        }

        public void InsertRow(object[] values)
        {
            // 1. Validation Checks (Length & Type) - Keep existing logic
            if (values.Length != Columns.Count)
                throw new ArgumentException($"Expected {Columns.Count} values, got {values.Length}.");

            for (int i = 0; i < Columns.Count; i++)
            {
                if (!IsValidType(values[i], Columns[i].Type))
                    throw new ArgumentException($"Column '{Columns[i].Name}' mismatch.");
            }

            // 2. NEW: Primary Key Constraint Check
            if (_primaryKeyColumnIndex.HasValue)
            {
                object pkValue = values[_primaryKeyColumnIndex.Value];

                if (_primaryKeyIndex.ContainsKey(pkValue))
                {
                    throw new ArgumentException($"Violation of Unique Constraint: Key '{pkValue}' already exists in '{Name}'.");
                }

                // Add to Index
                _primaryKeyIndex.Add(pkValue, values);
            }

            // 3. Add to Storage
            Rows.Add(values);
        }

        private bool IsValidType(object value, ColumnType type)
        {
            if (value == null) return false;
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