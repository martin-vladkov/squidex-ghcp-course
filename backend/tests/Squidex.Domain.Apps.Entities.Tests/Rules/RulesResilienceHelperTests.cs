// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Rules;

/// <summary>
/// Tests for <see cref="RulesResilienceHelper"/>.
/// Uses <c>initialDelayMs: 0</c> throughout so that the exponential-backoff
/// <see cref="Task.Delay"/> calls complete immediately and the suite stays fast.
/// </summary>
public class RulesResilienceHelperTests
{
    // ── success path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_succeed_on_first_attempt_without_retry()
    {
        var calls = 0;

        await RulesResilienceHelper.ExecuteWithRetryAsync(
            _ =>
            {
                calls++;
                return Task.CompletedTask;
            },
            maxAttempts: 3,
            initialDelayMs: 0);

        Assert.Equal(1, calls);
    }

    // ── IOException ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_retry_on_IOException_and_succeed()
    {
        var calls = 0;

        await RulesResilienceHelper.ExecuteWithRetryAsync(
            _ =>
            {
                calls++;
                if (calls < 2)
                {
                    throw new IOException("transient storage error");
                }

                return Task.CompletedTask;
            },
            maxAttempts: 3,
            initialDelayMs: 0);

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_throw_IOException_after_all_attempts_exhausted()
    {
        var calls = 0;

        await Assert.ThrowsAsync<IOException>(async () =>
            await RulesResilienceHelper.ExecuteWithRetryAsync(
                _ =>
                {
                    calls++;
                    throw new IOException("always fails");
                },
                maxAttempts: 3,
                initialDelayMs: 0));

        Assert.Equal(3, calls);
    }

    // ── TimeoutException ─────────────────────────────────────────────────────

    [Fact]
    public async Task Should_retry_on_TimeoutException_and_succeed()
    {
        var calls = 0;

        await RulesResilienceHelper.ExecuteWithRetryAsync(
            _ =>
            {
                calls++;
                if (calls < 2)
                {
                    throw new TimeoutException("upstream service timeout");
                }

                return Task.CompletedTask;
            },
            maxAttempts: 3,
            initialDelayMs: 0);

        Assert.Equal(2, calls);
    }

    // ── OperationCanceledException ───────────────────────────────────────────

    [Fact]
    public async Task Should_retry_on_inner_timeout_when_caller_token_is_not_cancelled()
    {
        var calls = 0;

        // Simulate a timed-out inner task: OperationCanceledException is thrown
        // but the caller's CancellationToken is NOT cancelled (it is an inner
        // timeout, not a user-requested cancellation).
        await RulesResilienceHelper.ExecuteWithRetryAsync(
            _ =>
            {
                calls++;
                if (calls < 2)
                {
                    throw new OperationCanceledException("inner task timed out");
                }

                return Task.CompletedTask;
            },
            maxAttempts: 3,
            initialDelayMs: 0);

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_not_retry_when_caller_token_is_cancelled()
    {
        var calls = 0;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await RulesResilienceHelper.ExecuteWithRetryAsync(
                ct =>
                {
                    calls++;
                    ct.ThrowIfCancellationRequested();
                    return Task.CompletedTask;
                },
                maxAttempts: 3,
                initialDelayMs: 0,
                ct: cts.Token));

        // Cancelled on the first attempt — no retry.
        Assert.Equal(1, calls);
    }

    // ── non-transient exceptions ─────────────────────────────────────────────

    [Fact]
    public async Task Should_not_retry_on_unexpected_exception()
    {
        var calls = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await RulesResilienceHelper.ExecuteWithRetryAsync(
                _ =>
                {
                    calls++;
                    throw new InvalidOperationException("programming error");
                },
                maxAttempts: 3,
                initialDelayMs: 0));

        // Non-transient exception — must fail on the very first attempt.
        Assert.Equal(1, calls);
    }

    // ── maxAttempts = 1 (retry disabled) ─────────────────────────────────────

    [Fact]
    public async Task Should_not_retry_when_maxAttempts_is_one()
    {
        var calls = 0;

        await Assert.ThrowsAsync<IOException>(async () =>
            await RulesResilienceHelper.ExecuteWithRetryAsync(
                _ =>
                {
                    calls++;
                    throw new IOException("transient");
                },
                maxAttempts: 1,
                initialDelayMs: 0));

        Assert.Equal(1, calls);
    }
}
