# EFCoreVsDapper

Benchmarking EFCore and Dapper with 1 Million records in a product table with SQL Server Database.

## Summary

BenchmarkDotNet v0.14.0, 

Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)

12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores, RAM 16.0 GB

.NET SDK 8.0.400

  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2 [AttachedDebugger]

  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

For 1 operation of each method:

| Method                                                     | Mean         | Error        | StdDev       | Allocated |
|----------------------------------------------------------- |-------------:|-------------:|-------------:|----------:|
| GetByIdWithDapperAsync                                     |     104.2 us |      2.06 us |      4.73 us |   7.55 KB |
| GetByIdWithEFCoreAsync                                     |     240.6 us |      4.76 us |      8.94 us | 104.44 KB |
| GetByIdWithEFCoreAsNoTrackingAsync                         |     250.2 us |      2.07 us |      1.93 us | 103.96 KB |
| FindWithFilterOrderByPaginationWithDapperAsync             | 620,303.9 us | 10,092.80 us |  9,440.81 us |  42.02 KB |
| FindWithFilterOrderByPaginationWithEFCoreAsync             | 636,583.7 us | 12,314.05 us | 11,518.57 us | 256.57 KB |
| FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync | 620,287.4 us |  7,573.23 us |  7,084.01 us | 223.68 KB |
| InsertProductWithDapperAsync                               |     167.3 us |      3.15 us |      4.10 us |   6.83 KB |
| InsertProductWithEFCoreAsync                               |     367.8 us |      7.24 us |     16.48 us |  110.8 KB |

For 50 consecutive operations of each method:

| Method                                                    | Mean          | Error       | StdDev      | Median        | Allocated  |
|-----------------------------------------------------------|--------------:|------------:|------------:|--------------:|-----------:|
| GetByIdWithDapperAsync                                    |      5.086 ms |   0.1073 ms |   0.3061 ms |      4.993 ms |  366.99 KB |
| GetByIdWithEFCoreAsync                                    |     13.975 ms |   0.2430 ms |   0.3406 ms |     13.856 ms | 5206.96 KB |
| GetByIdWithEFCoreAsNoTrackingAsync                        |     13.578 ms |   0.1481 ms |   0.1312 ms |     13.598 ms | 5177.34 KB |
| FindWithFilterOrderByPaginationWithDapperAsync            | 43,612.958 ms | 474.2633 ms | 443.6262 ms | 43,514.406 ms | 2048.23 KB |
| FindWithFilterOrderByPaginationWithEFCoreAsync            | 37,708.436 ms | 140.6169 ms | 109.7844 ms | 37,735.134 ms | 9988.21 KB |
| FindWithFilterOrderByPaginationWithEFCoreAsNoTrackingAsync| 37,770.107 ms | 214.9115 ms | 179.4608 ms | 37,692.805 ms | 8326.77 KB |
| InsertProductWithDapperAsync                              |      9.218 ms |   0.1760 ms |   0.1560 ms |      9.194 ms |  333.41 KB |
| InsertProductWithEFCoreAsync                              |     18.945 ms |   0.2834 ms |   0.2512 ms |     18.937 ms | 5530.56 KB |


### Legends 
 - Mean      : Arithmetic mean of all measurements
 - Error     : Half of 99.9% confidence interval
 - StdDev    : Standard deviation of all measurements
 - Median    : Value separating the higher half of all measurements (50th percentile)
 - Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
 - 1 ms      : 1 Millisecond (0.001 sec)