// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

/// <summary>
/// Immutable value object that describes the retry behaviour used by
/// <see cref="HttpRetryHelper.ExecuteWithRetryAsync{T}"/>.
/// </summary>
public readonly record struct RetryPolicy
{
    /// <summary>Production default: 3 attempts with 200 ms initial backoff.</summary>
    public static readonly RetryPolicy Default = new RetryPolicy(3, 200);

    /// <summary>
    /// Total number of tries, including the first (range 1–10 recommended).
    /// Set to 1 to disable retrying.
    /// </summary>
    public int MaxAttempts { get; init; }

    /// <summary>
    /// Base delay in milliseconds before the first retry.  Each subsequent delay
    /// doubles (200 → 400 → 800 …).  Keep ≤ 2000 ms for interactive flows;
    /// increase for background jobs where latency tolerance is higher.
    /// </summary>
    public int InitialDelayMs { get; init; }

    /// <summary>Initialises a <see cref="RetryPolicy"/> with explicit values.</summary>
    public RetryPolicy(int maxAttempts, int initialDelayMs)
    {
        MaxAttempts = maxAttempts;
        InitialDelayMs = initialDelayMs;
    }
}
