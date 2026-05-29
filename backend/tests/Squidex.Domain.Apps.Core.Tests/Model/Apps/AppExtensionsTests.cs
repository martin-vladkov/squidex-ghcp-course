// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Apps;

#pragma warning disable SA1310 // Field names must not contain underscore

public class AppExtensionsTests
{
    private const string AppName = "my-app";

    private readonly DomainId appId = DomainId.NewGuid();

    private App BuildApp(
        string? label = null,
        Contributors? contributors = null,
        AppClients? clients = null)
    {
        return new App
        {
            Id = appId,
            Name = AppName,
            Label = label!,
            Contributors = contributors ?? Contributors.Empty,
            Clients = clients ?? AppClients.Empty,
        };
    }

    [Fact]
    public void Should_return_named_id_with_app_id_and_name()
    {
        var app = BuildApp();

        var result = app.NamedId();

        Assert.Equal(appId, result.Id);
        Assert.Equal(AppName, result.Name);
    }

    [Fact]
    public void Should_return_name_as_display_name_when_label_is_null()
    {
        var app = BuildApp(label: null);

        var result = app.DisplayName();

        Assert.Equal(AppName, result);
    }

    [Fact]
    public void Should_return_label_as_display_name_when_label_is_set()
    {
        var app = BuildApp(label: "My Application");

        var result = app.DisplayName();

        Assert.Equal("My Application", result);
    }

    [Fact]
    public void Should_return_false_from_TryGetContributorRole_when_contributor_not_found()
    {
        var app = BuildApp();

        var found = app.TryGetContributorRole("unknown-user", isFrontend: false, out var role);

        Assert.False(found);
        Assert.Null(role);
    }

    [Fact]
    public void Should_return_true_from_TryGetContributorRole_when_contributor_exists()
    {
        var contributors = Contributors.Empty.Assign("user-1", Role.Editor);
        var app = BuildApp(contributors: contributors);

        var found = app.TryGetContributorRole("user-1", isFrontend: false, out var role);

        Assert.True(found);
        Assert.NotNull(role);
        Assert.Equal(Role.Editor, role.Name);
    }

    [Fact]
    public void Should_return_false_from_TryGetClientRole_when_client_not_found()
    {
        var app = BuildApp();

        var found = app.TryGetClientRole("unknown-client", isFrontend: false, out var role);

        Assert.False(found);
        Assert.Null(role);
    }

    [Fact]
    public void Should_return_true_from_TryGetClientRole_when_client_exists()
    {
        var clients = AppClients.Empty.Add("client-1", "my-secret", Role.Reader);
        var app = BuildApp(clients: clients);

        var found = app.TryGetClientRole("client-1", isFrontend: false, out var role);

        Assert.True(found);
        Assert.NotNull(role);
        Assert.Equal(Role.Reader, role.Name);
    }

    [Fact]
    public void Should_return_false_from_TryGetRole_when_role_not_found()
    {
        var app = BuildApp();

        var found = app.TryGetRole("non-existent-role", isFrontend: false, out var role);

        Assert.False(found);
        Assert.Null(role);
    }

    [Fact]
    public void Should_return_true_from_TryGetRole_for_built_in_role()
    {
        var app = BuildApp();

        var found = app.TryGetRole(Role.Developer, isFrontend: false, out var role);

        Assert.True(found);
        Assert.NotNull(role);
        Assert.Equal(Role.Developer, role.Name);
    }
}
