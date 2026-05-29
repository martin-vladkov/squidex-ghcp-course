// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Metrics;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

/// <summary>
/// OpenTelemetry metric definitions for the Apps domain.
///
/// Meter name: <see cref="MeterName"/> ("Squidex.Apps").
/// Wire it to the OTel pipeline by calling
///   <c>builder.AddMeter(AppMetrics.MeterName)</c>
/// inside your <see cref="OpenTelemetry.Metrics.MeterProviderBuilder"/> setup
/// (see <c>Squidex/Config/Domain/TelemetryServices.cs</c>).
///
/// View in production:
///   - OTLP → Prometheus → Grafana: metric name <c>squidex_app_language_ops_total</c>
///   - dotnet-counters: <c>dotnet-counters monitor --counters Squidex.Apps</c>
///   - /metrics endpoint (if Prometheus exporter is enabled in appsettings.json)
/// </summary>
public static class AppMetrics
{
    /// <summary>OpenTelemetry meter name for all App-domain metrics.</summary>
    public const string MeterName = "Squidex.Apps";

    private static readonly Meter Meter = new Meter(MeterName);

    /// <summary>
    /// Counts language mutations on an app (Add / Remove / Update).
    ///
    /// Tags:
    ///   op       – "AddLanguage" | "RemoveLanguage" | "UpdateLanguage"
    ///   language – ISO 639-1 two-letter code, e.g. "en", "de"
    /// </summary>
    public static readonly Counter<long> LanguageOps =
        Meter.CreateCounter<long>(
            name: "squidex.app.language_ops",
            unit: "{operation}",
            description: "Number of language add/remove/update operations performed on apps.");
}
