// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

/// <summary>
/// Represents one remote template repository configured in <see cref="TemplatesOptions"/>.
/// </summary>
public sealed class TemplateRepository
{
    /// <summary>
    /// Base URL for raw file access (e.g. GitHub raw content).
    /// Used to fetch <c>README.md</c> listings and per-template detail pages.
    /// </summary>
    public string ContentUrl { get; set; }

    /// <summary>
    /// Git clone URL passed to the CLI sync engine when applying a template.
    /// Falls back to <see cref="ContentUrl"/> when <see langword="null"/>.
    /// </summary>
    public string? GitUrl { get; set; }
}
