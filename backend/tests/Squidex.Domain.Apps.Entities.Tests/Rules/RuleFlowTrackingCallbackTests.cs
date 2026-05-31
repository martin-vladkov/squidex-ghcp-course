// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleFlowTrackingCallbackTests : GivenContext
{
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly ILogger<RuleFlowTrackingCallback> log = A.Fake<ILogger<RuleFlowTrackingCallback>>();
    private readonly RuleFlowTrackingCallback sut;

    // Options with zero delay so resilience retries complete immediately in tests.
    private static readonly IOptions<RulesOptions> FastOptions =
        Options.Create(new RulesOptions { ResilienceMaxAttempts = 3, ResilienceInitialDelayMs = 0 });

    public RuleFlowTrackingCallbackTests()
    {
        sut = new RuleFlowTrackingCallback(ruleUsageTracker, FastOptions, log);
    }

    [Fact]
    public async Task Should_track_usage_with_success()
    {
        var ruleId = DomainId.NewGuid();

        await sut.OnUpdateAsync(
            new FlowExecutionState<FlowEventContext>
            {
                InstanceId = default,
                Context = new FlowEventContext(),
                Definition = null!,
                DefinitionId = ruleId.ToString(),
                OwnerId = AppId.Id.ToString(),
                Status = FlowExecutionStatus.Completed,
            },
            CancellationToken);

        A.CallTo(() => ruleUsageTracker.TrackAsync(
                AppId.Id,
                ruleId,
                A<DateOnly>._,
                0,
                1,
                0,
                CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_usage_with_failure()
    {
        var ruleId = DomainId.NewGuid();

        await sut.OnUpdateAsync(
            new FlowExecutionState<FlowEventContext>
            {
                InstanceId = default,
                Context = new FlowEventContext(),
                Definition = null!,
                DefinitionId = ruleId.ToString(),
                OwnerId = AppId.Id.ToString(),
                Status = FlowExecutionStatus.Failed,
            },
            CancellationToken);

        A.CallTo(() => ruleUsageTracker.TrackAsync(
                AppId.Id,
                ruleId,
                A<DateOnly>._,
                0,
                0,
                1,
                CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_log_warning_when_flow_execution_fails()
    {
        var ruleId = DomainId.NewGuid();

        A.CallTo(() => log.IsEnabled(LogLevel.Warning)).Returns(true);

        await sut.OnUpdateAsync(
            new FlowExecutionState<FlowEventContext>
            {
                InstanceId = default,
                Context = new FlowEventContext(),
                Definition = null!,
                DefinitionId = ruleId.ToString(),
                OwnerId = AppId.Id.ToString(),
                Status = FlowExecutionStatus.Failed,
            },
            CancellationToken);

        A.CallTo(log)
            .Where(x => x.Method.Name == "Log" &&
                x.GetArgument<LogLevel>(0) == LogLevel.Warning)
            .MustHaveHappenedOnceExactly();
    }

    // ── resilience tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task Should_retry_tracking_on_IOException_and_succeed()
    {
        var ruleId = DomainId.NewGuid();
        var calls = 0;

        // First call throws a transient I/O error; second succeeds.
        A.CallTo(() => ruleUsageTracker.TrackAsync(
                A<DomainId>._,
                A<DomainId>._,
                A<DateOnly>._,
                A<int>._,
                A<int>._,
                A<int>._,
                A<CancellationToken>._))
            .Invokes(_ => { calls++; })
            .ReturnsLazily(() =>
            {
                if (calls <= 1)
                {
                    throw new IOException("simulated transient I/O failure");
                }

                return Task.CompletedTask;
            });

        await sut.OnUpdateAsync(
            new FlowExecutionState<FlowEventContext>
            {
                InstanceId = default,
                Context = new FlowEventContext(),
                Definition = null!,
                DefinitionId = ruleId.ToString(),
                OwnerId = AppId.Id.ToString(),
                Status = FlowExecutionStatus.Completed,
            },
            CancellationToken);

        // Must have been retried: two calls total.
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_not_retry_tracking_when_token_is_cancelled()
    {
        var ruleId = DomainId.NewGuid();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        A.CallTo(() => ruleUsageTracker.TrackAsync(
                A<DomainId>._,
                A<DomainId>._,
                A<DateOnly>._,
                A<int>._,
                A<int>._,
                A<int>._,
                A<CancellationToken>._))
            .Throws<OperationCanceledException>();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await sut.OnUpdateAsync(
                new FlowExecutionState<FlowEventContext>
                {
                    InstanceId = default,
                    Context = new FlowEventContext(),
                    Definition = null!,
                    DefinitionId = ruleId.ToString(),
                    OwnerId = AppId.Id.ToString(),
                    Status = FlowExecutionStatus.Completed,
                },
                cts.Token));

        // Explicit cancellation — must NOT retry: exactly one attempt.
        A.CallTo(() => ruleUsageTracker.TrackAsync(
                A<DomainId>._,
                A<DomainId>._,
                A<DateOnly>._,
                A<int>._,
                A<int>._,
                A<int>._,
                cts.Token))
            .MustHaveHappenedOnceExactly();
    }
}
