using Xunit;
using TinyDB.Core.Storage;
using TinyDB.Core.Parsing;
using System;

namespace TinyDB.Tests
{
    public class ConstraintTests
    {
        [Fact]
        public void Primary_Key_Enforces_Uniqueness()
        {
            var engine = new Engine();

            // 1. Create table with PK
            Run(engine, "CREATE TABLE Users (Id INT PRIMARY KEY, Name STRING)");

            // 2. Insert first row (Should succeed)
            Run(engine, "INSERT INTO Users VALUES (1, 'Derrek')");

            // 3. Insert duplicate row (Should fail)
            var ex = Assert.Throws<ArgumentException>(() =>
                Run(engine, "INSERT INTO Users VALUES (1, 'Kimani')")
            );

            Assert.Contains("Violation of Unique Constraint", ex.Message);
        }

        private void Run(Engine engine, string sql)
        {
            var tokenizer = new Tokenizer(sql);
            var parser = new Parser(tokenizer.Tokenize(), engine);
            parser.Parse();
        }
    }
}