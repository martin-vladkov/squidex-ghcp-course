// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public class HttpRetryHelperTests
{
    [Fact]
    public async Task Should_return_result_on_first_attempt()
    {
        var calls = 0;

        var result = await HttpRetryHelper.ExecuteWithRetryAsync<int>(
            _ =>
            {
                calls++;
                return Task.FromResult(42);
            },
            policy: new RetryPolicy(3, 0));

        Assert.Equal(42, result);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Should_retry_on_HttpRequestException_and_succeed()
    {
        var calls = 0;

        var result = await HttpRetryHelper.ExecuteWithRetryAsync<string>(
            _ =>
            {
                calls++;
                if (calls < 2)
                {
                    throw new HttpRequestException("transient network error");
                }

                return Task.FromResult("ok");
            },
            policy: new RetryPolicy(3, 0));

        Assert.Equal("ok", result);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_retry_on_http_timeout_and_succeed()
    {
        var calls = 0;

        // Simulate an HttpClient timeout: TaskCanceledException is thrown but
        // the caller's CancellationToken is NOT cancelled (it is a timeout, not
        // a user-requested cancellation).
        var result = await HttpRetryHelper.ExecuteWithRetryAsync<string>(
            _ =>
            {
                calls++;
                if (calls < 2)
                {
                    throw new TaskCanceledException("http timeout", new TimeoutException());
                }

                return Task.FromResult("after-timeout");
            },
            policy: new RetryPolicy(3, 0));

        Assert.Equal("after-timeout", result);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_throw_after_all_attempts_exhausted()
    {
        var calls = 0;

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await HttpRetryHelper.ExecuteWithRetryAsync<string>(
                _ =>
                {
                    calls++;
                    throw new HttpRequestException("always fails");
                },
                policy: new RetryPolicy(3, 0)));

        // All 3 attempts must have been made before giving up.
        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task Should_not_retry_non_transient_exceptions()
    {
        var calls = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await HttpRetryHelper.ExecuteWithRetryAsync<string>(
                _ =>
                {
                    calls++;
                    throw new InvalidOperationException("programming error");
                },
                policy: new RetryPolicy(3, 0)));

        // Non-transient exceptions must fail immediately — no retry.
        Assert.Equal(1, calls);
    }
}
