# EFCoreVsDapper

I am benchmarking EFCore and Dapper

## Summary

BenchmarkDotNet v0.14.0, 

Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)

12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores, RAM 16.0 GB

.NET SDK 8.0.400

  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2 [AttachedDebugger]

  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


| Method                                                     | Mean          | Error       | StdDev      | Median        | Allocated  |
|----------------------------------------------------------- |--------------:|------------:|------------:|--------------:|-----------:|
| GetByIdWithDapperAsync                                     |      5.086 ms |   0.1073 ms |   0.3061 ms |      4.993 ms |  366.99 KB |
| GetByIdWithEFCoreAsync                                     |     13.975 ms |   0.2430 ms |   0.3406 ms |     13.856 ms | 5206.96 KB |
| GetByIdWithEFCoreAsNoTrackingAsync                         |     13.578 ms |   0.1481 ms |   0.1312 ms |     13.598 ms | 5177.34 KB |
| InsertProductWithDapperAsync                               |      9.218 ms |   0.1760 ms |   0.1560 ms |      9.194 ms |  333.41 KB |
| InsertProductWithEFCoreAsync                               |     18.945 ms |   0.2834 ms |   0.2512 ms |     18.937 ms | 5530.56 KB |
| FindWithFilterOrderByPaginationWithDapperAsync             | 43,612.958 ms | 474.2633 ms | 443.6262 ms | 43,514.406 ms | 2048.23 KB |
| FindWithFilterOrderByPaginationWithEFCoreAsync             | 37,708.436 ms | 140.6169 ms | 109.7844 ms | 37,735.134 ms | 9988.21 KB |
| FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync | 37,770.107 ms | 214.9115 ms | 179.4608 ms | 37,692.805 ms | 8326.77 KB |

### Warnings
Environment
  Summary -> Benchmark was executed with attached debugger

### Hints
#### Outliers
 - BenchmarkEFCoreDapper.GetByIdWithDapperAsync: Default                                     -> 6 outliers were removed (5.88 ms..6.84 ms)
 - BenchmarkEFCoreDapper.GetByIdWithEFCoreAsync: Default                                     -> 1 outlier  was  removed (15.01 ms)
 - BenchmarkEFCoreDapper.GetByIdWithEFCoreAsNoTrackingAsync: Default                         -> 1 outlier  was  removed (14.00 ms)
 - BenchmarkEFCoreDapper.InsertProductWithDapperAsync: Default                               -> 1 outlier  was  removed (10.15 ms)
 - BenchmarkEFCoreDapper.InsertProductWithEFCoreAsync: Default                               -> 1 outlier  was  removed (20.09 ms)
 - BenchmarkEFCoreDapper.FindWithFilterOrderByPaginationWithEFCoreAsync: Default             -> 3 outliers were removed (39.71 s..40.98 s)
 - BenchmarkEFCoreDapper.FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync: Default -> 2 outliers were removed (38.43 s, 39.03 s)

### Legends 
 - Mean      : Arithmetic mean of all measurements
 - Error     : Half of 99.9% confidence interval
 - StdDev    : Standard deviation of all measurements
 - Median    : Value separating the higher half of all measurements (50th percentile)
 - Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
 - 1 ms      : 1 Millisecond (0.001 sec)

Diagnostic Output - MemoryDiagnoser 

BenchmarkRunner: End

Run time: 00:49:52 (2992.1 sec), executed benchmarks: 8

Global total time: 00:49:58 (2998.65 sec), executed benchmarks: 8