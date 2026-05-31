// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleQueueWriterTests : GivenContext
{
    private readonly IFlowManager<FlowEventContext> flowManager = A.Fake<IFlowManager<FlowEventContext>>();
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly RuleQueueWriter sut;

    public RuleQueueWriterTests()
    {
        sut = new RuleQueueWriter(flowManager, ruleUsageTracker, null);
    }

    [Fact]
    public async Task Should_not_enqueue_result_without_rule()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
        };

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_enqueue_result_without_job()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Rule = CreateRule(),
        };

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_enqueue_result_with_skip_reason()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.FromRule,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enqueue_success()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[] { result.Job.Value }, writes);
    }

    [Fact]
    public async Task Should_enqueue_disabled()
    {
        var result = new JobResult
        {
            SkipReason = SkipReason.Disabled,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        var writes = await EnqueueAndFlushAsync(result);

        Assert.Equal(new[] { result.Job.Value }, writes);
    }

    [Fact]
    public async Task Should_write_batched()
    {
        var result = new JobResult
        {
            SkipReason = default,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        for (var i = 0; i < 250; i++)
        {
            await sut.WriteAsync(AppId.Id, result);
        }

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 2);
    }

    private async Task<CreateFlowInstanceRequest<FlowEventContext>[]> EnqueueAndFlushAsync(JobResult result)
    {
        var writes = Array.Empty<CreateFlowInstanceRequest<FlowEventContext>>();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .Invokes(x => { writes = x.GetArgument<CreateFlowInstanceRequest<FlowEventContext>[]>(0)!; });

        await sut.WriteAsync(AppId.Id, result);
        await sut.FlushAsync();

        return writes;
    }

    // ── resilience tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task Should_retry_usage_tracking_on_IOException_and_succeed()
    {
        // Zero-delay options so the test does not wait for the backoff timer.
        var fastOptions = Options.Create(new RulesOptions { ResilienceMaxAttempts = 3, ResilienceInitialDelayMs = 0 });
        var sutFast = new RuleQueueWriter(flowManager, ruleUsageTracker, null, fastOptions);

        var calls = 0;

        // First TrackAsync call throws; second succeeds.
        A.CallTo(() => ruleUsageTracker.TrackAsync(
                A<DomainId>._,
                A<DomainId>._,
                A<DateOnly>._,
                A<int>._,
                A<int>._,
                A<int>._,
                A<CancellationToken>._))
            .Invokes(_ => calls++)
            .ReturnsLazily(() =>
            {
                if (calls <= 1)
                {
                    throw new IOException("simulated transient I/O failure");
                }

                return Task.CompletedTask;
            });

        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = null,
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        await sutFast.WriteAsync(AppId.Id, result);

        // Two calls: one transient failure + one successful retry.
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_not_retry_usage_tracking_for_non_transient_exception()
    {
        var fastOptions = Options.Create(new RulesOptions { ResilienceMaxAttempts = 3, ResilienceInitialDelayMs = 0 });
        var sutFast = new RuleQueueWriter(flowManager, ruleUsageTracker, null, fastOptions);

        A.CallTo(() => ruleUsageTracker.TrackAsync(
                A<DomainId>._,
                A<DomainId>._,
                A<DateOnly>._,
                A<int>._,
                A<int>._,
                A<int>._,
                A<CancellationToken>._))
            .Throws<InvalidOperationException>();

        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = null,
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = CreateRule(),
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sutFast.WriteAsync(AppId.Id, result));

        // Non-transient exception — must NOT retry: exactly one attempt.
        A.CallTo(() => ruleUsageTracker.TrackAsync(
                A<DomainId>._,
                A<DomainId>._,
                A<DateOnly>._,
                A<int>._,
                A<int>._,
                A<int>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
