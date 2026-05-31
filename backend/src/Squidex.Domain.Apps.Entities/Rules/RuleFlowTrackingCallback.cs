// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleFlowTrackingCallback(IRuleUsageTracker ruleUsageTracker, IOptions<RulesOptions> options, ILogger<RuleFlowTrackingCallback> log) : IFlowExecutionCallback<FlowEventContext>
{
    private readonly int resilienceMaxAttempts = options.Value.ResilienceMaxAttempts;
    private readonly int resilienceInitialDelayMs = options.Value.ResilienceInitialDelayMs;
    public IClock Clock { get; set; } = SystemClock.Instance;

    public async Task OnUpdateAsync(FlowExecutionState<FlowEventContext> state,
        CancellationToken ct)
    {
        if (state.Status == FlowExecutionStatus.Completed)
        {
            await TrackAsync(state, 1, 0, ct).ConfigureAwait(false);
        }
        else
        {
            LogMessages.LogFlowExecutionFailed(log, state.DefinitionId, state.OwnerId);
            await TrackAsync(state, 0, 1, ct).ConfigureAwait(false);
        }
    }

    private async Task TrackAsync(
        FlowExecutionState<FlowEventContext> state,
        int totalSucceeded,
        int totalFailed,
        CancellationToken ct)
    {
        var today = Clock.GetCurrentInstant().ToDateOnly();

        await RulesResilienceHelper.ExecuteWithRetryAsync(
            t => ruleUsageTracker.TrackAsync(
                DomainId.Create(state.OwnerId),
                DomainId.Create(state.DefinitionId),
                today,
                0,
                totalSucceeded,
                totalFailed,
                t),
            maxAttempts: resilienceMaxAttempts,
            initialDelayMs: resilienceInitialDelayMs,
            ct: ct).ConfigureAwait(false);
    }
}
