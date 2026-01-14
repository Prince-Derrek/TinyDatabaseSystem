using Xunit;
using TinyDB.Core.Storage;
using TinyDB.Core.Parsing;
using System.Linq;

namespace TinyDB.Tests
{
    public class JoinTests
    {
        [Fact]
        public void Can_Join_Two_Tables()
        {
            var engine = new Engine();

            // 1. Setup Data
            Run(engine, "CREATE TABLE Users (Id INT, Name STRING)");
            Run(engine, "CREATE TABLE Orders (OrderId INT, UserId INT, Amount INT)");

            Run(engine, "INSERT INTO Users VALUES (1, 'Derrek')");
            Run(engine, "INSERT INTO Users VALUES (2, 'Kimani')");

            Run(engine, "INSERT INTO Orders VALUES (100, 1, 500)"); // Derrek's order
            Run(engine, "INSERT INTO Orders VALUES (101, 1, 200)"); // Derrek's 2nd order
            Run(engine, "INSERT INTO Orders VALUES (102, 3, 999)"); // Unknown user (Should not appear in inner join)

            // 2. Perform Join
            // "Give me all orders with user names"
            var result = Run(engine, "SELECT * FROM Users JOIN Orders ON Users.Id = Orders.UserId");

            // 3. Assertions
            Assert.Equal(2, result.Rows.Count); // Should only match Derrek's 2 orders

            // Row 0 should be Derrek + Order 100
            // Users has 2 cols, Orders has 3 cols. Total 5.
            // Users.Name is index 1. Orders.Amount is index 2+2=4.
            Assert.Equal("Derrek", result.Rows[0][1]);
            Assert.Equal(500, result.Rows[0][4]);
        }

        private TinyDB.Core.Execution.ExecutionResult Run(Engine engine, string sql)
        {
            return new Parser(new Tokenizer(sql).Tokenize(), engine).Parse();
        }
    }
}