using System;
using System.Collections.Generic;
using System.Linq;
using TinyDB.Core.Storage;
using TinyDB.Core.Parsing;
using TinyDB.Core.Execution;

namespace TinyDB.Repl
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("   TinySQL Database Engine (v1.0)");
            Console.WriteLine("   Type 'EXIT' to quit.");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            // Initialize the Singletons
            var engine = new Engine();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("TinySQL> ");
                Console.ResetColor();

                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                try
                {
                    // 1. Tokenize
                    var tokenizer = new Tokenizer(input);
                    var tokens = tokenizer.Tokenize();

                    // 2. Parse & Execute
                    var parser = new Parser(tokens, engine);
                    var result = parser.Parse();

                    // 3. Print Result
                    PrintResult(result);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        static void PrintResult(ExecutionResult result)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" -> {result.Message}");
            Console.ResetColor();

            if (result.IsQuery && result.Rows.Count > 0)
            {
                PrintTable(result.Columns, result.Rows);
            }
            Console.WriteLine();
        }

        static void PrintTable(List<string> columns, List<object[]> rows)
        {
            // 1. Calculate column widths
            var widths = new int[columns.Count];
            for (int i = 0; i < columns.Count; i++)
            {
                widths[i] = columns[i].Length; // Start with header length
                foreach (var row in rows)
                {
                    string val = row[i]?.ToString() ?? "NULL";
                    if (val.Length > widths[i]) widths[i] = val.Length;
                }
                widths[i] += 2; // Add padding
            }

            // 2. Print Header
            PrintRow(columns.ToArray(), widths);
            PrintLine(widths);

            // 3. Print Data
            foreach (var row in rows)
            {
                PrintRow(row, widths);
            }
        }

        static void PrintLine(int[] widths)
        {
            Console.Write("+");
            foreach (var w in widths)
            {
                Console.Write(new string('-', w) + "+");
            }
            Console.WriteLine();
        }

        static void PrintRow(object[] row, int[] widths)
        {
            Console.Write("|");
            for (int i = 0; i < row.Length; i++)
            {
                string val = row[i]?.ToString() ?? "NULL";
                Console.Write(val.PadRight(widths[i]) + "|");
            }
            Console.WriteLine();
        }
    }
}