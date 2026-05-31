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
/// Retry behaviour is controlled by a <see cref="RetryPolicy"/> value object.
/// Pass <see cref="RetryPolicy.Default"/> (or omit the parameter) for standard
/// production settings: 3 attempts, 200 ms initial delay.
///
/// Transient conditions that are retried:
///   <see cref="HttpRequestException"/>        – network/transport failure.
///   <see cref="TaskCanceledException"/> where the caller's token is NOT cancelled
///                                             – HttpClient request timeout.
///
/// Conditions that are NOT retried (fail immediately):
///   Any other exception type                  – programming error; retrying would not help.
///   <see cref="TaskCanceledException"/> where the caller's token IS cancelled
///                                             – explicit user/request cancellation; honour it.
///
/// Rollback guidance:
///   Replace <see cref="ExecuteWithRetryAsync{T}"/> with a direct await on the
///   original operation.  No state is persisted, so rollback is safe at any time.
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
    /// <param name="policy">
    ///   Retry configuration. Defaults to <see cref="RetryPolicy.Default"/>
    ///   (3 attempts, 200 ms initial backoff).
    /// </param>
    /// <param name="ct">
    ///   Cancellation token forwarded to <paramref name="operation"/> and to
    ///   <see cref="Task.Delay(int, CancellationToken)"/> between retries.
    /// </param>
    /// <returns>The result returned by <paramref name="operation"/> on a successful attempt.</returns>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryPolicy? policy = null,
        CancellationToken ct = default)
    {
        var p = policy ?? RetryPolicy.Default;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(ct);
            }
            catch (HttpRequestException) when (attempt < p.MaxAttempts)
            {
                // Transient network/transport failure — retry after exponential backoff.
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested && attempt < p.MaxAttempts)
            {
                // HttpClient request timeout (not an explicit user cancellation) — retry after backoff.
            }

            // Exponential backoff: initialDelayMs → 2× → 4× …
            var delayMs = p.InitialDelayMs * (1 << (attempt - 1));
            await Task.Delay(delayMs, ct);
        }
    }
}
