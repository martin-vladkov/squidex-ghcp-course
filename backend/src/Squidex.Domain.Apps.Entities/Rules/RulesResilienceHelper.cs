// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Rules;

/// <summary>
/// Lightweight retry helper with exponential backoff for transient failures
/// in the Rules pipeline (storage writes, enqueue operations).
///
/// Retry behaviour is controlled by <paramref name="maxAttempts"/> and
/// <paramref name="initialDelayMs"/>. Pass <c>initialDelayMs: 0</c> in
/// unit tests to skip actual delays.
///
/// Transient conditions that are retried:
///   <see cref="IOException"/>                — storage / network I/O error.
///   <see cref="TimeoutException"/>           — upstream service timeout.
///   <see cref="OperationCanceledException"/> where the caller's token is NOT
///                                              cancelled — inner-task timeout,
///                                              not an explicit user cancel.
///
/// Conditions that are NOT retried (propagate immediately):
///   Any other exception type                 — programming error; retrying
///                                              would not help.
///   <see cref="OperationCanceledException"/> where the caller's token IS
///                                              cancelled — honour it.
///
/// Rollback guidance:
///   Replace <see cref="ExecuteWithRetryAsync"/> with a direct await on the
///   original operation.  No state is persisted; rollback is safe at any time.
/// </summary>
public static class RulesResilienceHelper
{
    /// <summary>
    /// Executes <paramref name="operation"/> with exponential-backoff retry on
    /// transient storage/service failures.
    /// </summary>
    /// <param name="operation">
    ///   The async operation to run. Receives the active
    ///   <see cref="CancellationToken"/> so it remains cancellable between retries.
    /// </param>
    /// <param name="maxAttempts">
    ///   Total number of tries, including the first (range 1–10 recommended).
    ///   Set to 1 to disable retrying. Default is 3.
    /// </param>
    /// <param name="initialDelayMs">
    ///   Base delay in milliseconds before the first retry. Each subsequent
    ///   delay doubles (200 → 400 → 800 …). Pass 0 in unit tests to skip
    ///   real waits. Default is 200 ms.
    /// </param>
    /// <param name="ct">
    ///   Cancellation token forwarded to <paramref name="operation"/> and to
    ///   <see cref="Task.Delay(int, CancellationToken)"/> between retries.
    /// </param>
    public static async Task ExecuteWithRetryAsync(
        Func<CancellationToken, Task> operation,
        int maxAttempts = 3,
        int initialDelayMs = 200,
        CancellationToken ct = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await operation(ct).ConfigureAwait(false);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                // Transient storage/network I/O failure — retry after backoff.
            }
            catch (TimeoutException) when (attempt < maxAttempts)
            {
                // Upstream service timed out — retry after backoff.
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested && attempt < maxAttempts)
            {
                // Inner task timed out (not an explicit user cancellation) — retry.
            }

            // Exponential back-off: initialDelayMs → 2× → 4× …
            var delayMs = initialDelayMs * (1 << (attempt - 1));
            await Task.Delay(delayMs, ct).ConfigureAwait(false);
        }
    }
}
