namespace TinyDB.Core.Definitions
{
    public class Column
    {
        public string Name { get; }
        public ColumnType Type { get; }
        public bool IsPrimaryKey { get; }

        public Column(string name, ColumnType type, bool isPrimaryKey = false)
        {
            Name = name;
            Type = type;
            IsPrimaryKey = isPrimaryKey;
        }
    }
}