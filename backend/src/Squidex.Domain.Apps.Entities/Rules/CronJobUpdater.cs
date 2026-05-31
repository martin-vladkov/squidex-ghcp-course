// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Events;
using Squidex.Flows.CronJobs;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class CronJobUpdater(
    IAppProvider appProvider,
    ICronJobManager<CronJobContext> cronJobs,
    IRuleEnqueuer ruleEnqueuer,
    IOptions<RulesOptions> options,
    ILogger<CronJobUpdater> log)
    : IEventConsumer, IInitializable
{
    private readonly bool cronJobTriggerEnabled = options.Value.EnableCronJobTrigger;
    public StreamFilter EventsFilter => StreamFilter.Prefix("rule-");

    public Task InitializeAsync(
        CancellationToken ct)
    {
        cronJobs.Subscribe(HandleCronJobAsync);
        return Task.CompletedTask;
    }

    public async Task HandleCronJobAsync(CronJob<CronJobContext> job,
        CancellationToken ct)
    {
        var (appId, ruleId) = job.Context;

        if (!cronJobTriggerEnabled)
        {
            LogMessages.LogCronJobTriggerDisabled(log, ruleId, appId.Id);
            RuleMetrics.CronJobTriggerSkipped.Add(1);
            return;
        }

        var rule = await appProvider.GetRuleAsync(appId.Id, ruleId, ct).ConfigureAwait(false);

        // The rule might have been updated or deleted in the meantime, but we are running asynchronously.
        if (rule == null || rule.Trigger is not CronJobTrigger cronJob)
        {
            LogMessages.LogCronJobSkipped(log, ruleId, appId.Id);
            return;
        }

        LogMessages.LogCronJobTriggered(log, ruleId, appId.Id);

        // The rule enqueue needs an event.
        var @event = new RuleCronJobTriggered { AppId = appId, RuleId = ruleId, Value = cronJob.Value };

        await ruleEnqueuer.EnqueueAsync(rule, Envelope.Create(@event), ct).ConfigureAwait(false);
    }

    public async Task On(Envelope<IEvent> @event)
    {
#pragma warning disable MA0004
        // MA0004: On() has no CancellationToken and fires under the event-consumer
        // pipeline (ASP.NET Core host, no SynchronizationContext).  The AddCronJobAsync
        // and cronJobs.RemoveAsync calls below run to completion regardless; context
        // capture would have no effect.  The hot-path call in HandleCronJobAsync
        // (with a real ct) was fixed above.
        if (@event.Payload is RuleCreated created)
        {
            if (created.Trigger is CronJobTrigger cronJob)
            {
                await AddCronJobAsync(created.AppId, created.RuleId, cronJob, default);
            }
        }
        else if (@event.Payload is RuleUpdated updated && updated.Trigger != null)
        {
            if (updated.Trigger is CronJobTrigger cronJob)
            {
                await AddCronJobAsync(updated.AppId, updated.RuleId, cronJob, default);
            }
            else
            {
                await cronJobs.RemoveAsync(updated.RuleId.ToString());
            }
        }
        else if (@event.Payload is RuleDeleted deleted)
        {
            await cronJobs.RemoveAsync(deleted.RuleId.ToString());
        }
#pragma warning restore MA0004
    }

    private async Task AddCronJobAsync(NamedId<DomainId> appId, DomainId id, CronJobTrigger trigger,
        CancellationToken ct)
    {
#pragma warning disable MA0004
        // MA0004: cronJobs.AddAsync and the second await below run under
        // ASP.NET Core hosting (no SynchronizationContext); suppressed until the
        // full-file clean-up pass tracked in docs/backlog.md.
        await cronJobs.AddAsync(new CronJob<CronJobContext>
        {
            Id = id.ToString(),
            CronExpression = trigger.CronExpression,
            CronTimezone = trigger.CronTimezone,
            Context = new CronJobContext(appId, id),
        }, ct);
#pragma warning restore MA0004

        LogMessages.LogCronJobRegistered(log, id, appId.Id, trigger.CronExpression);
    }
}
