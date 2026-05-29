## Summary
- Added `Perf_GetPriorities_baseline` timing test (100 k iterations, Stopwatch, `[Trait("Category","Perf")]`) to LanguagesConfigTests.cs — measure only, no timing assertion
- Used `ITestOutputHelper` for output so numbers are visible with `--logger "console;verbosity=detailed"`
- Recorded baseline and variance in ai-track-docs/perf-baseline.md: ~313–334 ns/op on arm64 macOS .NET 10
- Files touched: backend/tests/.../LanguagesConfigTests.cs, ai-track-docs/perf-baseline.md

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test ... --filter "Category=Perf" --logger "console;verbosity=detailed"
  Run 1: Per op : 334 ns
  Run 2: Per op : 322 ns
  Run 3: Per op : 313 ns
  Run 4: Per op : 317 ns

  dotnet test ... --filter "FullyQualifiedName~LanguagesConfigTests"
  → Passed! - Failed: 0, Passed: 22, Skipped: 0, Total: 22, Duration: 109 ms
  ```

## Risk & Rollback
- Risk: low
- Rollback: git revert 83f5f2bc5379791dcebeb0f0d2f508944fb7ec28

## Track
- Level: Crawl
- Exercise: Ex6
