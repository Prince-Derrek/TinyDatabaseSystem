namespace TinyDB.Core.Definitions
{
    public class Column
    {
        public string Name { get; }
        public ColumnType Type { get; }

        public Column(string name, ColumnType type)
        {
            Name = name;
            Type = type;
        }
    }
}