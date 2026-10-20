# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Single `net10.0` console project (`EFCoreVsDapper/EFCoreVsDapper.csproj`) in `EFCoreVsDapper.slnx`, using BenchmarkDotNet 0.15, EF Core 10 (SQL Server), Dapper, Bogus and Microsoft.Extensions.Configuration (JSON + environment variables). Requires the .NET 10 SDK. There is no test project.

```bash
dotnet build -c Release
dotnet run -c Release --project EFCoreVsDapper                                   # runs every [Benchmark]; BenchmarkDotNet rejects non-optimized (Debug) builds
dotnet run -c Release --project EFCoreVsDapper -- --filter "*GetById*" --job dry  # subset / quick smoke run

# Create the schema (requires the dotnet-ef CLI; there is no local tool manifest)
dotnet ef database update --project EFCoreVsDapper
```

`Program.cs` passes `args` to `BenchmarkRunner`, so BenchmarkDotNet options such as `--filter` and `--job` work. Results go to `BenchmarkDotNet.Artifacts/` (git-ignored).

## Configuration and database setup

- `AppConfiguration.GetConnectionString()` reads `appsettings.json`, then `appsettings.{Environment}.json`, then environment variables. The environment comes from `DOTNET_ENVIRONMENT` and defaults to `Development`. `appsettings.json` has no connection string; `appsettings.Development.json` holds the LocalDB one. Override with the `ConnectionStrings__Default` environment variable (BenchmarkDotNet's child process inherits it). The files are copied to the output directory.
- The string is consumed in `Program.cs`, the `BenchmarkEFCoreDapper` field initializer (the benchmark process reads it itself), `DatabaseConnectionFactory`, and `ApplicationDbContext.OnConfiguring`, which only configures SQL Server when the options are not already configured (so the `dotnet ef` design-time path uses the same config).
- Default target: SQL Server LocalDB, database `TestEcomDB`, Windows auth (`Trusted_Connection`). Migrations are not applied at startup; run `dotnet ef database update` first. The only migration (`Initial`) creates the `Products` table.
- Seeding: the benchmarks expect about 1,000,000 rows (README). `ProductRepository.InsertAsync()` generates 1M Bogus products and saves them through EF in batches of 10,000, and `DeleteAllAsync()` truncates the table. Both calls are commented out in `Program.cs`; uncomment them for one run to (re)seed, then comment them out again. `GetById` benchmarks look up `Id = 2000`, so that row must exist (on an empty table they still run and return no row).

## How the benchmarks are structured

- `BenchmarkEFCoreDapper` (`[MemoryDiagnoser(false)]`) builds its **own** `ServiceProvider` in a field initializer. BenchmarkDotNet instantiates the class in a separate process, so the DI container in `Program.cs` is used only for seeding and the post-run cleanup.
- Each benchmark loops `numberOfIterations` times (currently `1`). The README's second results table used 50; change the field by hand to reproduce it.
- `IProductRepository`/`ProductRepository` holds both implementations side by side: `...WithDapperAsync`, `...WithEFCoreAsync`, and `...WithEFCoreAsNoTrackingAsync`. Each method opens its own resource: a new `ApplicationDbContext` from `IDatabaseConnectionFactory.CreateEFCoreContext()` or a new, already-opened `SqlConnection` from `CreateDapperConnection()`. The `ApplicationDbContext` registered in DI is never injected. Command timeout is 600 s for both.
- Scenarios: get by id; filter by price range + order + `OFFSET/FETCH` pagination plus a separate `COUNT` query (Dapper builds SQL with `StringBuilder`, EF composes `IQueryable`); single-row insert.
- The self-join `GROUP BY ... HAVING` variants exist in the repository but their benchmarks are commented out.

## Quirks

- Insert benchmarks add rows named `"New product"` throughout the run, which also grows the table the pagination `COUNT` queries scan. `CleanUpInsertedManually()` deletes them only after `BenchmarkRunner.Run` returns; if a run is aborted, delete them yourself.
- The Dapper get-by-id query does not select `Id`, while EF materializes the full entity, so the two are not exactly equivalent projections.
- The Dapper pagination query only adds `ORDER BY` for `created_on`, `price_desc` or `price_asc`. Any other `orderBy` produces `OFFSET` without `ORDER BY`, which SQL Server rejects. The EF version silently skips ordering instead.
