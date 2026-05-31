```

BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD


```
| Method               | Mean     | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------- |---------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| SetEquals_Original   | 4.731 μs | 0.0142 μs | 0.0111 μs |  1.00 | 0.3815 |      - |   2.38 KB |        1.00 |
| SetEquals_Optimized  | 4.493 μs | 0.0105 μs | 0.0088 μs |  0.95 | 0.3662 |      - |   2.28 KB |        0.96 |
| Duplicates_Original  | 6.336 μs | 0.0123 μs | 0.0109 μs |  1.34 | 1.6174 | 0.0534 |   9.95 KB |        4.19 |
| Duplicates_Optimized | 2.920 μs | 0.0092 μs | 0.0082 μs |  0.62 | 0.8011 | 0.0076 |   4.93 KB |        2.08 |
