// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class RulesOptions
{
    public int MaxEnrichedEvents { get; set; } = 500;

    public TimeSpan RulesCacheDuration { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Feature flag: enable or disable cron-job rule triggers system-wide.
    /// Set to <c>false</c> during maintenance or when cron-trigger behaviour needs
    /// to be rolled back without a deployment. Default is <c>true</c> (enabled).
    /// Rollback path: set <c>rules:enableCronJobTrigger</c> to <c>false</c> in
    /// appsettings.json (or the equivalent environment variable
    /// <c>RULES__ENABLECRONJOBTRIGGER=false</c>) and restart the application.
    /// </summary>
    public bool EnableCronJobTrigger { get; set; } = true;

    /// <summary>
    /// Total number of attempts (including the first) for resilience retries
    /// in the Rules pipeline. Range 1–10; set to 1 to disable retrying.
    /// Config key: <c>rules:resilienceMaxAttempts</c>.
    /// </summary>
    public int ResilienceMaxAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds before the first retry. Doubles on each
    /// subsequent attempt (200 → 400 → 800 …).
    /// Config key: <c>rules:resilienceInitialDelayMs</c>.
    /// </summary>
    public int ResilienceInitialDelayMs { get; set; } = 200;

    public TimeSpan StaleTime { get; set; } = TimeSpan.FromDays(2);

    public HashSet<Type> Actions { get; set; } = [];
}
