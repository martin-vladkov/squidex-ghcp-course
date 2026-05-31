// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

/// <summary>
/// Configuration for the app-templates subsystem.
/// Bound from the <c>templates</c> section in <c>appsettings.json</c>.
/// See <c>docs/templates-subsystem.md</c> for the full configuration reference.
/// </summary>
public sealed class TemplatesOptions
{
    /// <summary>
    /// Override the Squidex base URL used by the CLI sync session.
    /// When <see langword="null"/>, <c>IUrlGenerator.Root()</c> is used.
    /// Useful in local dev or Docker where the public URL differs from the
    /// internal network address.
    /// </summary>
    public string? LocalUrl { get; set; }

    /// <summary>
    /// One or more remote template repositories to search.
    /// Each entry must expose a <c>README.md</c> at <c>ContentUrl/README.md</c>
    /// listing available templates in the expected Markdown format.
    /// </summary>
    public TemplateRepository[] Repositories { get; set; }
}
