# Mini ORM

A simplified, scratch-built Object-Relational Mapper (ORM) for .NET and PostgreSQL, built using ADO.NET and Npgsql. This project demonstrates the internal workings of an ORM, including reflection-based metadata mapping, generic CRUD operations, and a command-line migration tool.

## Project Structure

The solution consists of two console projects:

*   **MiniOrm/**: The core library containing the `DbContext`, generic `DbSet<T>`, and the `TypeMapper`. It also includes a step-by-step coding demo in `Program.cs`.
*   **MiniOrm.Migrations/**: A CLI tool for managing database schema changes (creating, applying, and rolling back migrations).

## Prerequisites

*   [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
*   [PostgreSQL](https://www.postgresql.org/download/) database instance.

## Setup

### 1. PostgreSQL Setup
Ensure you have a PostgreSQL database created. You can create one using `psql` or a tool like pgAdmin:
```sql
CREATE DATABASE miniorm;
```

### 2. Configure Environment Variable
The application reads the database connection string from the `MINIORM_CONN` environment variable. **Do not hardcode credentials.**

Set the variable in your terminal (replace with your actual credentials):
```bash
# Linux / macOS
export MINIORM_CONN="Host=localhost;Port=5432;Database=miniorm;Username=postgres;Password=your_password"

# Windows (Command Prompt)
set MINIORM_CONN=Host=localhost;Port=5432;Database=miniorm;Username=postgres;Password=your_password

# Windows (PowerShell)
$env:MINIORM_CONN="Host=localhost;Port=5432;Database=miniorm;Username=postgres;Password=your_password"
```

## Migrations CLI

The `MiniOrm.Migrations` project handles database schema management.

### Generate a Migration
Scans your models (e.g., `Product`) and creates a timestamped `.sql` file with `up` and `down` sections.
```bash
dotnet run --project MiniOrm.Migrations migrations add InitialMigration
```

### Apply Migrations
Runs all pending migrations in order and records them in a `migrations` table.
```bash
dotnet run --project MiniOrm.Migrations migrations apply
```

### List Migrations
Displays all applied migrations.
```bash
dotnet run --project MiniOrm.Migrations migrations list
```

### Rollback Migration
Reverts the last applied migration using its `down` script.
```bash
dotnet run --project MiniOrm.Migrations migrations rollback
```

## Running the Demo

Once migrations are applied, run the main project to see the ORM in action:
```bash
dotnet run --project MiniOrm
```
The demo performs the following steps:
1.  Instantiates the `AppDbContext`.
2.  Inserts a new `Product` (demonstrating `SERIAL PRIMARY KEY` and null handling).
3.  Finds a product by ID.
4.  Updates an existing product.
5.  Retrieves all products.
6.  Deletes the product and verifies the final state.

## Core Concepts

### Reflection-Based Type Mapping
The `TypeMapper` uses C# Reflection to dynamically build `EntityMetadata` at runtime. It maps C# types to their PostgreSQL equivalents:
*   `int` (PrimaryKey) -> `SERIAL PRIMARY KEY`
*   `int`/`long` -> `INTEGER`/`BIGINT`
*   `decimal` -> `NUMERIC`
*   `string` -> `TEXT`
*   `bool` -> `BOOLEAN`
*   `DateTime` -> `TIMESTAMP`

It also handles **nullable value types** (e.g., `decimal?`) and **nullable reference types** (e.g., `string?`), ensuring they are mapped to `NULL` or `NOT NULL` columns correctly.

### Attribute-Based Filtering
To ensure clean mapping and prevent unnecessary properties (like navigation properties) from being persisted, the ORM only maps properties decorated with:
*   `[Table("name")]`: Specifies the database table name.
*   `[Column("name")]`: Specifies the database column name.
*   `[PrimaryKey]`: Identifies the primary key (automatically treated as an auto-incrementing identity column).

Any property without a `[Column]` or `[PrimaryKey]` attribute is ignored by the ORM.

## Constraints
*   Uses **raw ADO.NET** and `Npgsql`.
*   Zero dependencies on Dapper, Entity Framework Core, or other ORMs.
*   Uses **parameterized SQL** to prevent SQL injection and handle null values (`DBNull.Value`) safely.
