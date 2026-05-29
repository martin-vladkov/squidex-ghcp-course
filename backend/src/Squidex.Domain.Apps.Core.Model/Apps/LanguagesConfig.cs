// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Apps;

/// <summary>
/// Immutable, value-equality collection of languages configured for an app.
/// Every mutating method (<see cref="Set"/>, <see cref="Remove"/>, <see cref="MakeMaster"/>)
/// returns a new instance; the current instance is never modified.
/// </summary>
public sealed class LanguagesConfig : IFieldPartitioning
{
    /// <summary>Singleton config pre-seeded with English (<c>en</c>) as the only and master language.</summary>
    public static readonly LanguagesConfig English = new (
        new Dictionary<string, LanguageConfig>
        {
            [Language.EN] = new LanguageConfig(),
        },
        Language.EN);

    private readonly Dictionary<string, LanguageConfig> values;
    private readonly string master;

    /// <summary>ISO 639-1 key of the master (default) language. Always present in <see cref="Values"/>.</summary>
    public string Master => master;

    /// <summary>All configured ISO 639-1 language keys, including the master.</summary>
    public IEnumerable<string> AllKeys => values.Keys;

    /// <summary>Full map of language key → <see cref="LanguageConfig"/> (optional flag, fallback list).</summary>
    public IReadOnlyDictionary<string, LanguageConfig> Values => values;

    public LanguagesConfig(Dictionary<string, LanguageConfig> values, string master)
    {
        Guard.NotNull(values);
        Guard.NotNullOrEmpty(master);

        Cleanup(values, ref master);

        this.values = values;
        this.master = master;
    }

    [Pure]
    public LanguagesConfig MakeMaster(Language language)
    {
        Guard.NotNull(language);

        return Build(values, language);
    }

    [Pure]
    public LanguagesConfig Set(Language language, bool isOptional = false, params Language[]? fallbacks)
    {
        Guard.NotNull(language);

        var newLanguages = new Dictionary<string, LanguageConfig>(values)
        {
            [language] = new LanguageConfig(isOptional, ReadonlyList.Create(fallbacks)),
        };

        return Build(newLanguages, master);
    }

    [Pure]
    public LanguagesConfig Remove(Language language)
    {
        Guard.NotNull(language);

        var newLanguages = new Dictionary<string, LanguageConfig>(values);

        newLanguages.Remove(language);

        return Build(newLanguages, master);
    }

    private LanguagesConfig Build(Dictionary<string, LanguageConfig> newLanguages, string newMaster)
    {
        if (newLanguages.Count == 0)
        {
            return this;
        }

        Cleanup(newLanguages, ref newMaster);

        if (EqualLanguages(newLanguages) && Equals(newMaster, master))
        {
            return this;
        }

        return new LanguagesConfig(newLanguages, newMaster);
    }

    private bool EqualLanguages(Dictionary<string, LanguageConfig> newLanguages)
    {
        return newLanguages.EqualsDictionary(values);
    }

    private void Cleanup(Dictionary<string, LanguageConfig> newLanguages, ref string newMaster)
    {
        if (!newLanguages.ContainsKey(newMaster))
        {
            if (newLanguages.ContainsKey(master))
            {
                newMaster = master;
            }
            else
            {
                newMaster = newLanguages.Keys.First();
            }
        }

        var masterConfig = newLanguages[newMaster];

        if (masterConfig.IsOptional || masterConfig.Fallbacks.Any())
        {
            newLanguages[newMaster] = LanguageConfig.Default;
        }

        foreach (var (key, config) in newLanguages.ToList())
        {
            newLanguages[key] = config.Cleanup(key, newLanguages);
        }
    }

    public PartitionResolver ToResolver()
    {
        return partitioning =>
        {
            if (partitioning.Equals(Partitioning.Invariant))
            {
                return InvariantPartitioning.Instance;
            }

            return this;
        };
    }

    /// <summary>Returns <c>true</c> when <paramref name="key"/> is the master language key.</summary>
    public bool IsMaster(string key) => Equals(Master, key);

    public string? GetName(string key)
    {
        if (key != null && values.ContainsKey(key))
        {
            return Language.GetLanguage(key).EnglishName;
        }

        return null;
    }

    public bool IsOptional(string key)
    {
        if (key != null && values.TryGetValue(key, out var value))
        {
            return value.IsOptional;
        }

        return false;
    }

    public IEnumerable<string> GetPriorities(string key)
    {
        if (key != null)
        {
            if (Equals(Master, key))
            {
                yield return key;
            }
            else if (values.TryGetValue(key, out var config))
            {
                yield return key;

                foreach (var fallback in config.Fallbacks)
                {
                    yield return fallback;
                }

                if (config.Fallbacks.All(x => x.Iso2Code != Master))
                {
                    yield return Master;
                }
            }
        }
    }

    /// <summary>Returns <c>true</c> when <paramref name="key"/> is a non-null configured language key.</summary>
    public bool Contains(string key) => key != null && values.ContainsKey(key);

    public override string ToString()
    {
        return "language";
    }
}
