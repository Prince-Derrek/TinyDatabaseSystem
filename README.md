# TinySQL: A Relational Database Engine from Scratch

**TinySQL** is a lightweight, fully functional Relational Database Management System (RDBMS) implemented in C# (.NET 8). 

It was built as an engineering challenge to demonstrate systems programming concepts, including **language parsing**, **query execution engines**, and **data structure design**, without relying on external database libraries (like EF Core or SQLite).

## 🚀 Quick Start

### Prerequisites
* .NET 8.0 SDK
* Visual Studio or VS Code

### Running the REPL (Console)
Interact with the database using the command line.
```bash
cd src/TinyDB.Repl
dotnet run

```

### Running the Web Server (API)

Start a RESTful API to execute queries over HTTP.

```bash
cd src/TinyDB.Web
dotnet run

```

Swagger UI will be available at: `http://localhost:<port>/swagger`

## ✨ Key Features

* **SQL-Like Query Language:** Supports `CREATE`, `INSERT`, `SELECT`.
* **Relational Data:** Supports `JOIN` operations between tables.
* **Indexing:** Implements **Primary Key constraints** using Hash Indexing (O(1) lookups).
* **Strong Typing:** Enforces schema validation for `INT`, `STRING`, and `BOOL`.
* **Dual Interface:** Accessible via interactive Console REPL and REST API.

## 🏗 Project Structure

* **`TinyDB.Core`**: The brain. Contains the Storage Engine, Tokenizer, Parser, and Executor.
* **`TinyDB.Repl`**: The face. A console application for interactive usage.
* **`TinyDB.Web`**: The demo. An ASP.NET Core API demonstrating embedded usage.
* **`TinyDB.Tests`**: The verifier. xUnit test suite ensuring correctness.

## 📝 Example Usage

```sql
-- Define a schema with a Primary Key
CREATE TABLE Users (Id INT PRIMARY KEY, Name STRING, IsActive BOOL);

-- Insert data
INSERT INTO Users VALUES (1, 'Derrek', true);
INSERT INTO Users VALUES (2, 'Kimani', false);

-- Query with a Join
SELECT * FROM Users JOIN Orders ON Users.Id = Orders.UserId;

```

## 🤖 AI Usage Disclosure

This project was designed and implemented with the guidance of an AI System Architect (Gemini).

* **Role of AI:** Provided architectural phases, explained RDBMS concepts, and assisted with C# syntax for parsing logic.
* **Role of Human:** Executed implementation, verified logic through testing, and made design decisions regarding scope (e.g., choosing In-Memory storage).

