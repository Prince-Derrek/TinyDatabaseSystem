# Limitations & Trade-offs

TinySQL is an **educational prototype** designed to demonstrate the internals of database construction. It is **NOT** intended for production use. 

Below are the explicit engineering trade-offs made during development:

## 1. Persistence (The "In-Memory" Constraint)
* **Limitation:** Data is stored strictly in RAM.
* **Consequence:** If the application (Repl or Web) stops, **all data is lost**.
* **Reasoning:** Implementing a B-Tree paging system for disk I/O would vastly exceed the scope of this project.

## 2. ACID Compliance
* **Atomicity:** Not supported. If an `INSERT` fails halfway, partial data is not rolled back.
* **Isolation:** No concurrency control. The system is not thread-safe.
* **Durability:** See Persistence above.

## 3. Query Optimization
* **Limitation:** No Cost-Based Optimizer.
* **Consequence:** The engine does not reorder joins or pick indexes dynamically. It executes exactly what is typed.
* **Algorithm:** `JOIN` operations use a Nested Loop (O(N*M)), which performs poorly on large datasets.

## 4. SQL Support
* **Supported:** * `CREATE TABLE`
  * `INSERT`
  * `SELECT` (including `JOIN`)
  * `UPDATE` (Simple equality `WHERE` only; Primary Keys cannot be updated)
  * `DELETE` (Simple equality `WHERE` only)
* **Not Supported:** * `GROUP BY`
  * `ORDER BY`
  * `NULL` values
  * Complex `WHERE` clauses (e.g., `>`, `<`, `OR`, `AND`)