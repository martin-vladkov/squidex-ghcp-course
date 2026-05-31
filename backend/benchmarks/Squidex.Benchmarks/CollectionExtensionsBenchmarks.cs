// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using BenchmarkDotNet.Attributes;

/// <summary>
/// Benchmarks for the two micro-optimizations in CollectionExtensions:
///
/// 1. SetEquals — replace Intersect().Count() with HashSet.IsSupersetOf()
/// 2. Duplicates — replace GroupBy().Where(Count > 1) with HashSet streaming
///
/// Each benchmark has an _Original and an _Optimized variant so a single run
/// produces the before/after comparison.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class CollectionExtensionsBenchmarks
{
    private List<string> _source = null!;
    private List<string> _other = null!;
    private List<string> _withDuplicates = null!;

    [GlobalSetup]
    public void Setup()
    {
        // 100-element string sets — realistic size for tag/permission collections
        _source = Enumerable.Range(0, 100).Select(i => $"tag-{i}").ToList();
        _other  = Enumerable.Range(0, 100).Select(i => $"tag-{i}").ToList();

        // List with ~20 % duplicate rate
        var unique = Enumerable.Range(0, 80).Select(i => $"field-{i}").ToList();
        var dupes  = Enumerable.Range(0, 20).Select(i => $"field-{i}").ToList();
        _withDuplicates = [.. unique, .. dupes];
    }

    // -------------------------------------------------------------------------
    // SetEquals
    // -------------------------------------------------------------------------

    [Benchmark(Baseline = true, Description = "SetEquals_Original")]
    public bool SetEquals_Original()
    {
        // Original implementation: Intersect() produces lazy IEnumerable<T>,
        // then Count() materialises it by iterating the entire result.
        return _source.Count == _other.Count && _source.Intersect(_other).Count() == _other.Count;
    }

    [Benchmark(Description = "SetEquals_Optimized")]
    public bool SetEquals_Optimized()
    {
        // Optimised: build one HashSet from source, then use IsSupersetOf which
        // short-circuits on the first miss. No intermediate iterator object.
        return _source.Count == _other.Count && new HashSet<string>(_source).IsSupersetOf(_other);
    }

    // -------------------------------------------------------------------------
    // Duplicates
    // -------------------------------------------------------------------------

    [Benchmark(Description = "Duplicates_Original")]
    public int Duplicates_Original()
    {
        // Original: GroupBy allocates a Dictionary<K,List<V>> plus one List<V>
        // per group. Count() on each group re-iterates the group's list.
        return _withDuplicates.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).Count();
    }

    [Benchmark(Description = "Duplicates_Optimized")]
    public int Duplicates_Optimized()
    {
        // Optimised: two HashSets, single pass, yields on first duplicate sighting.
        // Avoids GroupBy's Dictionary + per-group List allocations entirely.
        return Duplicates(_withDuplicates).Count();
    }

    private static IEnumerable<T> Duplicates<T>(IEnumerable<T> input)
    {
        var seen = new HashSet<T>();
        var reported = new HashSet<T>();

        foreach (var item in input)
        {
            if (!seen.Add(item) && reported.Add(item))
            {
                yield return item;
            }
        }
    }
}
