using Xunit;
using TinyDB.Core.Storage;
using TinyDB.Core.Parsing;
using System.Linq;

namespace TinyDB.Tests
{
    public class ParserTests
    {
        [Fact]
        public void Full_Cycle_Test()
        {
            var engine = new Engine();

            // 1. Create Table
            Run(engine, "CREATE TABLE Users (Id INT, Name STRING, IsAdmin BOOL)");
            Assert.NotNull(engine.GetTable("Users"));

            // 2. Insert Data
            Run(engine, "INSERT INTO Users VALUES (1, 'Derrek', true)");
            Run(engine, "INSERT INTO Users VALUES (2, 'Guest', false)");

            var table = engine.GetTable("Users");
            Assert.Equal(2, table.Rows.Count);

            // 3. Select All
            var resultAll = Run(engine, "SELECT * FROM Users");
            Assert.Equal(2, resultAll.Rows.Count);
            Assert.Equal(3, resultAll.Columns.Count); // Id, Name, IsAdmin

            // 4. Select Specific Columns
            var resultPartial = Run(engine, "SELECT Name FROM Users");
            Assert.Equal(2, resultPartial.Rows.Count);
            Assert.Equal(1, resultPartial.Columns.Count);
            Assert.Equal("Derrek", resultPartial.Rows[0][0]);
        }

        // Helper to run tokenizer + parser
        private TinyDB.Core.Execution.ExecutionResult Run(Engine engine, string sql)
        {
            var tokenizer = new Tokenizer(sql);
            var tokens = tokenizer.Tokenize();
            var parser = new Parser(tokens, engine);
            return parser.Parse();
        }
    }
}