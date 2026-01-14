using Xunit;
using TinyDB.Core.Storage;
using TinyDB.Core.Parsing;
using TinyDB.Core.Execution;

namespace TinyDB.Tests
{
    public class CrudTests
    {
        [Fact]
        public void Can_Update_Row()
        {
            var engine = new Engine();
            Run(engine, "CREATE TABLE Users (Id INT PRIMARY KEY, Name STRING)");
            Run(engine, "INSERT INTO Users VALUES (1, 'OldName')");

            // Update
            var res = Run(engine, "UPDATE Users SET Name = 'NewName' WHERE Id = 1");
            Assert.Equal("1 rows updated.", res.Message);

            // Verify
            var select = Run(engine, "SELECT Name FROM Users");
            Assert.Equal("NewName", select.Rows[0][0]);
        }

        [Fact]
        public void Can_Delete_Row()
        {
            var engine = new Engine();
            Run(engine, "CREATE TABLE Users (Id INT PRIMARY KEY, Name STRING)");
            Run(engine, "INSERT INTO Users VALUES (1, 'ToDie')");
            Run(engine, "INSERT INTO Users VALUES (2, 'ToLive')");

            // Delete
            var res = Run(engine, "DELETE FROM Users WHERE Id = 1");
            Assert.Equal("1 rows deleted.", res.Message);

            // Verify
            var select = Run(engine, "SELECT * FROM Users");
            Assert.Equal(1, select.Rows.Count);
            Assert.Equal(2, select.Rows[0][0]); // Only ID 2 remains
        }

        private ExecutionResult Run(Engine engine, string sql)
        {
            return new Parser(new Tokenizer(sql).Tokenize(), engine).Parse();
        }
    }
}