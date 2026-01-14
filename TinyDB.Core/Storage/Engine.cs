using System;
using System.Collections.Generic;
using System.Linq;
using TinyDB.Core.Definitions;

namespace TinyDB.Core.Storage
{
    public class Engine
    {
        private readonly Dictionary<string, Table> _tables;

        public Engine()
        {
            _tables = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);
        }

        public Table CreateTable(string tableName)
        {
            if (_tables.ContainsKey(tableName))
                throw new ArgumentException($"Table '{tableName}' already exists.");

            var table = new Table(tableName);
            _tables[tableName] = table;
            return table;
        }

        public Table GetTable(string tableName)
        {
            if (!_tables.TryGetValue(tableName, out var table))
                throw new ArgumentException($"Table '{tableName}' not found.");

            return table;
        }

        // Just for debug/demo purposes
        public List<string> ListTables()
        {
            return _tables.Keys.ToList();
        }
    }
}