# Mini ORM

## 📌 Project Purpose
A simplified, scratch-built Object-Relational Mapper (ORM) using ADO.NET and Npgsql. Implemented a library that maps C# classes to PostgreSQL. This project demonstrates the internal workings of an ORM, including reflection-based metadata mapping, generic CRUD operations, and a command-line migration tool and included a step-by-step coding demo that proves the whole system works against a live database exactly as a real consumer would use it.

## 🎯 Learning Objectives
1. Understanding of how ORMs work internally by building one from the ground up.
2. Used ADO.NET with Npgsql (NpgsqlConnection, NpgsqlCommand, NpgsqlDataReader) for raw PostgreSQL access.
3. Applied C# reflection to map class properties to columns dynamically.
4. Mapped both value types and nullable instance types to their Postgres-native equivalents.
5. Designed a command-driven migration CLI that tracks and applied schema changes.
6. Demonstrated the system through a realistic step-by-step coding walkthrough.

## 🏗️ Project Structure

The solution consists of two console projects:

*   **MiniOrm/**: The core library containing the `DbContext`, generic `DbSet<T>`, and the `TypeMapper`. It also includes a step-by-step coding demo in `Program.cs`.
*   **MiniOrm.Migrations/**: A CLI tool for managing database schema changes (creating, applying, and rolling back migrations).

![Project Overview](assets/screenshot.png)

## ✅ Prerequisites

*   [.NET 9.0/10.0 SDK](https://dotnet.microsoft.com/download) 
*   [PostgreSQL](https://www.postgresql.org/download/) database instance.
*   Need Nugets packeges, run: dotnet add MiniOrm/MiniOrm.csproj package Npgsql

## ⚙️ Setup

### 1. PostgreSQL Setup
Ensure you have a PostgreSQL database created. You can create one using `psql` or a tool like pgAdmin:
```sql
CREATE DATABASE miniorm;
```

### 2. Configure Environment Variable
The application reads the database connection string from the `MINIORM_CONN` environment variable.

Set the variable in your terminal (replace with your actual credentials):
```bash
# Linux / macOS
export MINIORM_CONN="Host=localhost;Port=5432;Database=miniorm;Username=postgres;Password=your_password"

# Windows (Command Prompt)
set MINIORM_CONN=Host=localhost;Port=5432;Database=miniorm;Username=postgres;Password=your_password

# Windows (PowerShell)
$env:MINIORM_CONN="Host=localhost;Port=5432;Database=miniorm;Username=postgres;Password=your_password"
```

## 🗄️ Migrations CLI

The `MiniOrm.Migrations` project handles database schema management. Run MiniOrm/Migrations/Program.cs in the terminal and then write the command that mentions below:

### Generate a Migration
Scans your models (e.g., `Product`) and creates a timestamped `.sql` file with `up` and `down` sections.
```bash
dotnet run -- migrations add InitialMigration
```

### Apply Migrations
Runs all pending migrations in order and records them in a `migrations` table.
```bash
dotnet run -- migrations apply
```

### List Migrations
Displays all applied migrations.
```bash
dotnet run -- migrations list
```

### Rollback Migration
Reverts the last applied migration using its `down` script.
```bash
dotnet run -- migrations rollback
```

## 🚀 Running the Demo

Once migrations are applied, run the main project to see the ORM in action. Open MiniOrm/MiniOrm/Program.cs in terminal and run this using:
```bash
dotnet run
```
The demo performs the following steps:
1.  Instantiates the `AppDbContext`.
2.  Inserts a new `Product` (demonstrating `SERIAL PRIMARY KEY` and null handling). Product informations are mentions in Program.cs file.
3.  Finds a product by ID.
4.  Updates an existing product.
5.  Retrieves all products.
6.  Deletes the product and verifies the final state.


## 🛠️ Implementation Details: Mapping & Filtering

### Type Mapping
The ORM uses the `TypeMapper` class to translate between C# types and PostgreSQL data types. This is achieved through reflection:

1.  **Type Conversion**: The `MapToPostgresType` method inspects the `PropertyType` of each property and maps it to the corresponding PostgreSQL type:
    - `int` / `Int32` → `INTEGER`
    - `long` / `Int64` → `BIGINT`
    - `decimal` → `NUMERIC`
    - `string` → `TEXT`
    - `bool` → `BOOLEAN`
    - `DateTime` → `TIMESTAMP`
2.  **Primary Key Handling**: If a property is marked with `[PrimaryKey]`, it is automatically mapped to `SERIAL PRIMARY KEY` (for `int`) or `BIGSERIAL PRIMARY KEY` (for `long`), enabling auto-incrementing behavior in PostgreSQL.
3.  **Nullability Detection**: The ORM distinguishes between nullable and non-nullable types:
    - For **Value Types** (e.g., `int?`), it uses `Nullable.GetUnderlyingType` to check for nullability.
    - For **Reference Types** (e.g., `string?`), it uses `NullabilityInfoContext` to determine if the property is marked as nullable.
    - This ensures that database columns are created with the correct `NULL` or `NOT NULL` constraints.

### Attribute Filtering
The ORM uses a "Whitelist" approach for property mapping, ensuring that only intended properties are persisted to the database.

1.  **Table Mapping**: Every entity class must be decorated with the `[Table("name")]` attribute. If it's missing, the ORM will throw an exception during metadata generation.
2.  **Column Filtering**: In the `TypeMapper`, only properties explicitly decorated with `[Column("name")]` or `[PrimaryKey("name")]` are included in the `EntityMetadata`.
3.  **Ignoring Properties**: Any property within the class that *lacks* these attributes is completely ignored by the ORM's reflection engine. This allows developers to include:
    - Computed properties (e.g., `public string FullName => $"{FirstName} {LastName}";`).
    - Internal helper properties.
    - Navigation properties for business logic that aren't intended for direct database mapping.


## ⛔ Constraints
*   Uses **raw ADO.NET** and `Npgsql`.
*   Zero dependencies on Dapper, Entity Framework Core, or other ORMs.
*   Uses **parameterized SQL** to prevent SQL injection and handle null values (`DBNull.Value`) safely.