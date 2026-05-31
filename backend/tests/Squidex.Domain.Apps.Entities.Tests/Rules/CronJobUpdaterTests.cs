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
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Events;
using Squidex.Flows.CronJobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class CronJobUpdaterTests : GivenContext
{
    private readonly ICronJobManager<CronJobContext> cronJobs = A.Fake<ICronJobManager<CronJobContext>>();
    private readonly IRuleEnqueuer ruleEnqueuer = A.Fake<IRuleEnqueuer>();
    private readonly ILogger<CronJobUpdater> log = A.Fake<ILogger<CronJobUpdater>>();
    private readonly CronJobUpdater sut;

    public CronJobUpdaterTests()
    {
        sut = new CronJobUpdater(AppProvider, cronJobs, ruleEnqueuer,
            Options.Create(new RulesOptions()), log);
    }

    [Fact]
    public void Should_return_rules_filter_for_events_filter()
    {
        Assert.Equal(StreamFilter.Prefix("rule-"), sut.EventsFilter);
    }

    [Fact]
    public async Task Should_register_handler_when_initialized()
    {
        await sut.InitializeAsync(default);

        A.CallTo(() => cronJobs.Subscribe(A<Func<CronJob<CronJobContext>, CancellationToken, Task>>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_register_new_cron_job()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleCreated
                {
                    AppId = AppId,
                    Trigger = new CronJobTrigger
                    {
                        CronExpression = "* */5 * * *",
                        CronTimezone = "Europe/Berlin",
                    },
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.AddAsync(
                A<CronJob<CronJobContext>>.That.Matches(x =>
                    x.Id == ruleId.ToString() &&
                    x.Context.RuleId == ruleId &&
                    x.Context.AppId == AppId &&
                    x.CronExpression == "* */5 * * *" &&
                    x.CronTimezone == "Europe/Berlin"),
                default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_register_updated_cron_job()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleUpdated
                {
                    AppId = AppId,
                    Trigger = new CronJobTrigger
                    {
                        CronExpression = "* */5 * * *",
                        CronTimezone = "Europe/Berlin",
                    },
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.AddAsync(
                A<CronJob<CronJobContext>>.That.Matches(x =>
                    x.Id == ruleId.ToString() &&
                    x.Context.RuleId == ruleId &&
                    x.Context.AppId == AppId &&
                    x.CronExpression == "* */5 * * *" &&
                    x.CronTimezone == "Europe/Berlin"),
                default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_unregister_when_trigger_changed()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleUpdated
                {
                    AppId = AppId,
                    Trigger = new ManualTrigger
                    {
                    },
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.RemoveAsync(ruleId.ToString(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_unregister_cron_job_when_rule_deleted()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleDeleted
                {
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.RemoveAsync(ruleId.ToString(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_do_nothing_when_trigger_not_changed()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleUpdated
                {
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(cronJobs)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_call_rule_enqueue_when_cron_job_due()
    {
        var rule = CreateAndSetupRule(new CronJobTrigger());

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sut.HandleCronJobAsync(job, CancellationToken);

        A.CallTo(() => ruleEnqueuer.EnqueueAsync(rule, A<Envelope<IEvent>>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_now_call_rule_enqueue_when_rule_has_no_cron_job_anymore()
    {
        var rule = CreateAndSetupRule();

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sut.HandleCronJobAsync(job, CancellationToken);

        A.CallTo(ruleEnqueuer)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_now_call_rule_enqueue_when_rule_is_not_found()
    {
        var rule = CreateAndSetupRule();

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sut.HandleCronJobAsync(job, CancellationToken);

        A.CallTo(ruleEnqueuer)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_skip_enqueue_when_cron_trigger_flag_is_disabled()
    {
        // Feature flag OFF — enqueuer must never be called.
        var sutFlagOff = new CronJobUpdater(AppProvider, cronJobs, ruleEnqueuer,
            Options.Create(new RulesOptions { EnableCronJobTrigger = false }), log);

        var rule = CreateAndSetupRule(new CronJobTrigger());

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        A.CallTo(() => log.IsEnabled(LogLevel.Warning)).Returns(true);

        await sutFlagOff.HandleCronJobAsync(job, CancellationToken);

        A.CallTo(ruleEnqueuer).MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_increment_counter_when_cron_trigger_flag_is_disabled()
    {
        // OTel metric contract test: RuleMetrics.CronJobTriggerSkipped must
        // record exactly one measurement when EnableCronJobTrigger is false.
        var measurements = new List<long>();

        using var listener = new System.Diagnostics.Metrics.MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == RuleMetrics.MeterName &&
                instrument.Name == "squidex.rules.cronjob_trigger_skipped")
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => measurements.Add(value));
        listener.Start();

        var sutFlagOff = new CronJobUpdater(AppProvider, cronJobs, ruleEnqueuer,
            Options.Create(new RulesOptions { EnableCronJobTrigger = false }), log);

        var rule = CreateAndSetupRule(new CronJobTrigger());

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sutFlagOff.HandleCronJobAsync(job, CancellationToken);

        Assert.Single(measurements);
        Assert.Equal(1L, measurements[0]);
    }

    // ── resilience tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task Should_retry_enqueue_on_IOException_and_succeed()
    {
        // Zero-delay options so the test does not wait for the backoff timer.
        var sutFast = new CronJobUpdater(AppProvider, cronJobs, ruleEnqueuer,
            Options.Create(new RulesOptions { ResilienceMaxAttempts = 3, ResilienceInitialDelayMs = 0 }), log);

        var rule = CreateAndSetupRule(new CronJobTrigger());
        var calls = 0;

        // First EnqueueAsync call throws; second succeeds.
        A.CallTo(() => ruleEnqueuer.EnqueueAsync(rule, A<Envelope<IEvent>>._, A<CancellationToken>._))
            .Invokes(_ => calls++)
            .ReturnsLazily(() =>
            {
                if (calls <= 1)
                {
                    throw new IOException("simulated transient queue failure");
                }

                return Task.CompletedTask;
            });

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sutFast.HandleCronJobAsync(job, CancellationToken);

        // Two calls: one transient failure + one successful retry.
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Should_not_retry_enqueue_when_token_is_cancelled()
    {
        var sutFast = new CronJobUpdater(AppProvider, cronJobs, ruleEnqueuer,
            Options.Create(new RulesOptions { ResilienceMaxAttempts = 3, ResilienceInitialDelayMs = 0 }), log);

        var rule = CreateAndSetupRule(new CronJobTrigger());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        A.CallTo(() => ruleEnqueuer.EnqueueAsync(rule, A<Envelope<IEvent>>._, A<CancellationToken>._))
            .Throws<OperationCanceledException>();

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            sutFast.HandleCronJobAsync(job, cts.Token));

        // Explicit cancellation — must NOT retry.
        A.CallTo(() => ruleEnqueuer.EnqueueAsync(rule, A<Envelope<IEvent>>._, cts.Token))
            .MustHaveHappenedOnceExactly();
    }
}
