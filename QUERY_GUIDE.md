# TinySQL Syntax Guide

## Data Types
* `INT`: Integer numbers.
* `STRING`: Text (must be enclosed in single quotes `'value'`).
* `BOOL`: Boolean values (`true` / `false`).

## Commands

### 1. Create Table
Define a new table. Optionally set a Primary Key for uniqueness.
```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY, 
    Name STRING, 
    InStock BOOL
)

```

### 2. Insert Data

Add a row. Values must match the column order defined in the table.

```sql
INSERT INTO Products VALUES (101, 'Laptop', true)

```

### 3. Select Data

Retrieve all columns.

```sql
SELECT * FROM Products

```

Retrieve specific columns.

```sql
SELECT Name, InStock FROM Products

```

### 4. Join Tables

Combine data from two tables based on a matching column.

```sql
SELECT * FROM Users JOIN Orders ON Users.Id = Orders.UserId

```
