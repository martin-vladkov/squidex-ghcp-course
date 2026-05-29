// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

/// <summary>
/// Lightweight retry helper with exponential backoff for transient HTTP failures.
///
/// Tuning parameters (pass at each call site via optional arguments):
///   maxAttempts    – total number of tries including the first (default 3; range
///                    1-10 recommended; set to 1 to disable retrying).
///   initialDelayMs – base delay in milliseconds before the first retry (default
///                    200 ms); each subsequent delay doubles (200 → 400 → 800 …).
///                    Keep ≤ 2000 ms for interactive flows; increase for background
///                    jobs where latency tolerance is higher.
///
/// Transient conditions that are retried:
///   HttpRequestException           – network/transport failure (DNS, TCP reset, etc.)
///   TaskCanceledException where the caller's CancellationToken is NOT cancelled
///                                  – HttpClient request timeout (not user cancel).
///
/// Conditions that are NOT retried (fail immediately):
///   Any other exception type       – programming error; retrying would not help.
///   TaskCanceledException where the caller's CancellationToken IS cancelled
///                                  – explicit user/request cancellation; honour it.
///
/// Rollback guidance:
///   Remove the call to ExecuteWithRetryAsync and replace it with a direct await on
///   the original operation call.  No state is persisted, so rollback is safe at
///   any time.
/// </summary>
public static class HttpRetryHelper
{
    /// <summary>
    /// Executes <paramref name="operation"/> with exponential-backoff retry on
    /// transient HTTP failures.
    /// </summary>
    /// <typeparam name="T">Return type of the operation.</typeparam>
    /// <param name="operation">
    ///   The async operation to execute. Receives the active <see cref="CancellationToken"/>
    ///   so individual HTTP requests remain cancellable between retries.
    /// </param>
    /// <param name="maxAttempts">Maximum number of attempts (first try + retries). Default 3.</param>
    /// <param name="initialDelayMs">
    ///   Base delay in milliseconds before the first retry.  The delay doubles on each
    ///   subsequent attempt.  Default 200 ms.
    /// </param>
    /// <param name="ct">
    ///   Cancellation token forwarded to <paramref name="operation"/> and to
    ///   <see cref="Task.Delay(int, CancellationToken)"/> between retries.
    /// </param>
    /// <returns>The result returned by <paramref name="operation"/> on a successful attempt.</returns>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        int maxAttempts = 3,
        int initialDelayMs = 200,
        CancellationToken ct = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(ct);
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                // Transient network/transport failure — retry after exponential backoff.
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested && attempt < maxAttempts)
            {
                // HttpClient request timeout (not an explicit user cancellation) — retry after backoff.
            }

            // Exponential backoff: 200 ms, 400 ms, 800 ms, …
            var delayMs = initialDelayMs * (1 << (attempt - 1));
            await Task.Delay(delayMs, ct);
        }
    }
}
