// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Metrics;

namespace Squidex.Domain.Apps.Entities.Rules;

/// <summary>
/// OpenTelemetry metric definitions for the Rules domain.
///
/// Meter name: <see cref="MeterName"/> ("Squidex.Rules").
/// Wire it to the OTel pipeline by calling
///   <c>builder.AddMeter(RuleMetrics.MeterName)</c>
/// inside your <see cref="OpenTelemetry.Metrics.MeterProviderBuilder"/> setup
/// (see <c>Squidex/Config/Domain/TelemetryServices.cs</c>).
/// </summary>
public static class RuleMetrics
{
    /// <summary>OpenTelemetry meter name for all Rules-domain metrics.</summary>
    public const string MeterName = "Squidex.Rules";

    private static readonly Meter Meter = new Meter(MeterName);

    /// <summary>
    /// Counts cron-job rule trigger invocations skipped because
    /// <see cref="Squidex.Domain.Apps.Core.HandleRules.RulesOptions.EnableCronJobTrigger"/>
    /// is <c>false</c>.
    ///
    /// Tags: none (global kill-switch affects all apps equally).
    /// </summary>
    public static readonly Counter<long> CronJobTriggerSkipped =
        Meter.CreateCounter<long>(
            name: "squidex.rules.cronjob_trigger_skipped",
            unit: "{trigger}",
            description: "Number of cron-job rule triggers skipped due to EnableCronJobTrigger being disabled.");
}
