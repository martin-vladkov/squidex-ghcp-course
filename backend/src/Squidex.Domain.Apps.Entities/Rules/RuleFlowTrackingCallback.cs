// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleFlowTrackingCallback(IRuleUsageTracker ruleUsageTracker, ILogger<RuleFlowTrackingCallback> log) : IFlowExecutionCallback<FlowEventContext>
{
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

        await ruleUsageTracker.TrackAsync(
            DomainId.Create(state.OwnerId),
            DomainId.Create(state.DefinitionId),
            today,
            0,
            totalSucceeded,
            totalFailed,
            ct).ConfigureAwait(false);
    }
}
