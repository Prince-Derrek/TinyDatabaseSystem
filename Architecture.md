# System Architecture & Internals

TinySQL follows a strict **Layered Architecture** to decouple the user interface from the database logic. This separation allows the same Core engine to power both a Console REPL and a Web API.

## 1. High-Level Design

```text
[ USER INPUT ]
      |
      v
+-----------------------+
|   INTERFACE LAYER     |  (TinyDB.Repl / TinyDB.Web)
+-----------------------+
      |
      v
+-----------------------+
|    QUERY ENGINE       |  (TinyDB.Core)
| --------------------- |
|  1. Tokenizer         |  -> Converts string to Tokens
|  2. Parser            |  -> Validates grammar & Dispatch
|  3. Executor          |  -> Orchestrates Storage operations
+-----------------------+
      |
      v
+-----------------------+
|   STORAGE ENGINE      |  (TinyDB.Core)
| --------------------- |
|  1. Table Definitions |  -> Schema enforcement
|  2. Indices           |  -> Hash Maps for Primary Keys
|  3. Row Storage       |  -> In-Memory Lists
+-----------------------+

```

## 2. Component Details

### The Query Pipeline

1. **Tokenizer (Lexer):**
* Reads raw text character-by-character.
* Identifies keywords (`SELECT`, `INSERT`), Literals (`'Hello'`, `123`), and Symbols (`(`, `,`, `*`).
* Output: A stream of `Token` objects.


2. **Recursive Descent Parser:**
* Consumes tokens to understand intent.
* Translates SQL grammar directly into Engine method calls.
* Example: `PARSE_SELECT` calls `engine.GetTable()` then iterates rows.



### Storage Model

* **Tables:** Stored as `List<object[]>`. This minimizes memory overhead compared to `List<Dictionary>`.
* **Indexing:** Primary Keys are enforced using a `Dictionary<object, object[]>`. This provides **O(1)** complexity for uniqueness checks, preventing full table scans during insertion.
* **Type System:** Schema validation occurs on **Write**. It is impossible to insert a `STRING` into an `INT` column.

### Join Algorithm

* **Strategy:** Nested Loop Join.
* **Complexity:** O(N * M).
* **Reasoning:** Chosen for implementation simplicity and clarity. While slower than Hash Joins for massive datasets, it is sufficient for an educational prototype.

## 3. Web Integration

The engine is registered as a **Singleton** in the ASP.NET Core Dependency Injection container. This ensures the database state persists across multiple HTTP requests, simulating a persistent server process.

