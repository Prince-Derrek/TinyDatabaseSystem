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
        public int DeleteRows(string columnName, object value)
        {
            // Find the column index
            var colIndex = Columns.FindIndex(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            if (colIndex == -1) throw new ArgumentException($"Column '{columnName}' not found.");

            // Check if we are deleting by Primary Key (Fast Path O(1))
            if (_primaryKeyColumnIndex.HasValue && colIndex == _primaryKeyColumnIndex.Value)
            {
                if (_primaryKeyIndex.ContainsKey(value))
                {
                    var row = _primaryKeyIndex[value];
                    Rows.Remove(row); // O(N) in List, but O(1) in Index lookup
                    _primaryKeyIndex.Remove(value);
                    return 1;
                }
                return 0;
            }

            // Slow Path: Scan all rows (O(N))
            var rowsToDelete = new List<object[]>();
            foreach (var row in Rows)
            {
                if (row[colIndex].Equals(value))
                {
                    rowsToDelete.Add(row);
                }
            }

            foreach (var row in rowsToDelete)
            {
                Rows.Remove(row);
                // Also remove from index if we have one
                if (_primaryKeyColumnIndex.HasValue)
                {
                    var pkVal = row[_primaryKeyColumnIndex.Value];
                    _primaryKeyIndex.Remove(pkVal);
                }
            }

            return rowsToDelete.Count;
        }

        public int UpdateRows(Dictionary<string, object> updates, string whereCol, object whereVal)
        {
            // 1. Identify rows to update
            var colIndex = Columns.FindIndex(c => c.Name.Equals(whereCol, StringComparison.OrdinalIgnoreCase));
            if (colIndex == -1) throw new ArgumentException($"WHERE Column '{whereCol}' not found.");

            List<object[]> targets = new List<object[]>();

            // Optimization: PK Lookup
            if (_primaryKeyColumnIndex.HasValue && colIndex == _primaryKeyColumnIndex.Value)
            {
                if (_primaryKeyIndex.TryGetValue(whereVal, out var row)) targets.Add(row);
            }
            else
            {
                // Scan
                targets.AddRange(Rows.Where(r => r[colIndex].Equals(whereVal)));
            }

            // 2. Map update columns to indices
            var updateIndices = new Dictionary<int, object>();
            foreach (var kvp in updates)
            {
                int idx = Columns.FindIndex(c => c.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (idx == -1) throw new ArgumentException($"Target Column '{kvp.Key}' not found.");

                // Safety: Prevent PK updates
                if (_primaryKeyColumnIndex.HasValue && idx == _primaryKeyColumnIndex.Value)
                    throw new ArgumentException("Updating Primary Key is not allowed.");

                // Type Check
                if (!IsValidType(kvp.Value, Columns[idx].Type))
                    throw new ArgumentException($"Type mismatch for column '{kvp.Key}'.");

                updateIndices[idx] = kvp.Value;
            }

            // 3. Apply updates
            foreach (var row in targets)
            {
                foreach (var kvp in updateIndices)
                {
                    row[kvp.Key] = kvp.Value;
                }
            }

            return targets.Count;
        }
    }
}