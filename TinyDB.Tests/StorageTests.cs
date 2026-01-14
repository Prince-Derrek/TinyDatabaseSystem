using Xunit;
using TinyDB.Core.Storage;
using TinyDB.Core.Definitions;

namespace TinyDB.Tests
{
    public class StorageTests
    {
        [Fact]
        public void Can_Create_Table_And_Insert_Data()
        {
            // Arrange
            var engine = new Engine();
            var table = engine.CreateTable("Users");
            table.AddColumn("Id", ColumnType.Integer);
            table.AddColumn("Name", ColumnType.String);

            // Act
            table.InsertRow(new object[] { 1, "Derrek" });
            table.InsertRow(new object[] { 2, "Kimani" });

            // Assert
            var storedTable = engine.GetTable("Users");
            Assert.Equal(2, storedTable.Rows.Count);
            Assert.Equal("Derrek", storedTable.Rows[0][1]);
        }
    }
}