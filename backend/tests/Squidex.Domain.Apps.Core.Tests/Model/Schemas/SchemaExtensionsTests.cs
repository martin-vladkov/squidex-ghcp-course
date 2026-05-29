// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Schemas;

#pragma warning disable SA1310 // Field names must not contain underscore

public class SchemaExtensionsTests
{
    private readonly DomainId schemaId = DomainId.NewGuid();
    private readonly Schema schema_0 = new Schema { Name = "my-schema" };

    [Fact]
    public void Should_return_named_id_with_schema_id_and_name()
    {
        var schema = schema_0 with { Id = schemaId };

        var result = schema.NamedId();

        Assert.Equal(schemaId, result.Id);
        Assert.Equal("my-schema", result.Name);
    }

    [Fact]
    public void Should_return_zero_max_id_for_empty_schema()
    {
        var result = schema_0.MaxId();

        Assert.Equal(0L, result);
    }

    [Fact]
    public void Should_return_max_id_from_root_fields()
    {
        var schema_1 = schema_0
            .AddField(Fields.Number(1, "field1", Partitioning.Invariant))
            .AddField(Fields.Number(3, "field3", Partitioning.Invariant))
            .AddField(Fields.Number(2, "field2", Partitioning.Invariant));

        var result = schema_1.MaxId();

        Assert.Equal(3L, result);
    }

    [Fact]
    public void Should_return_max_id_from_nested_fields_inside_array()
    {
        var arrayField = Fields.Array(1, "myArray", Partitioning.Invariant,
            new ArrayFieldProperties(),
            Fields.String(10, "nested1"),
            Fields.String(20, "nested2"));

        var schema_1 = schema_0
            .AddField(arrayField)
            .AddField(Fields.Number(5, "field5", Partitioning.Invariant));

        var result = schema_1.MaxId();

        Assert.Equal(20L, result);
    }

    [Fact]
    public void Should_return_resolving_references_fields_when_match()
    {
        var refsField = Fields.References(1, "refs", Partitioning.Invariant,
            new ReferencesFieldProperties { ResolveReference = true, MaxItems = 1 });

        var schema_1 = schema_0.AddField(refsField);

        var result = schema_1.ResolvingReferences().ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Should_return_empty_resolving_references_when_no_match()
    {
        var refsField = Fields.References(1, "refs", Partitioning.Invariant,
            new ReferencesFieldProperties { ResolveReference = false });

        var schema_1 = schema_0.AddField(refsField);

        var result = schema_1.ResolvingReferences().ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Should_return_resolving_assets_fields_when_match()
    {
        var assetsField = Fields.Assets(1, "assets", Partitioning.Invariant,
            new AssetsFieldProperties { ResolveFirst = true });

        var schema_1 = schema_0.AddField(assetsField);

        var result = schema_1.ResolvingAssets().ToList();

        Assert.Single(result);
    }

    [Fact]
    public void Should_return_empty_resolving_assets_when_no_match()
    {
        var assetsField = Fields.Assets(1, "assets", Partitioning.Invariant,
            new AssetsFieldProperties { ResolveFirst = false });

        var schema_1 = schema_0.AddField(assetsField);

        var result = schema_1.ResolvingAssets().ToList();

        Assert.Empty(result);
    }
}
