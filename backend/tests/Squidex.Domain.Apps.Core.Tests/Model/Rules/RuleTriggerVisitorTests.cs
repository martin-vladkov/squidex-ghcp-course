// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Triggers;

namespace Squidex.Domain.Apps.Core.Model.Rules;

public class RuleTriggerVisitorTests
{
    private readonly IRuleTriggerVisitor<string, object> visitor =
        A.Fake<IRuleTriggerVisitor<string, object>>();

    [Fact]
    public void Should_dispatch_AssetChangedTriggerV2_to_visitor()
    {
        var trigger = new AssetChangedTriggerV2 { Condition = "my-condition" };

        trigger.Accept(visitor, new object());

        A.CallTo(() => visitor.Visit(trigger, A<object>._)).MustHaveHappenedOnceExactly();
        Assert.Equal("my-condition", trigger.Condition);
    }

    [Fact]
    public void Should_dispatch_CommentTrigger_to_visitor()
    {
        var trigger = new CommentTrigger();

        trigger.Accept(visitor, new object());

        A.CallTo(() => visitor.Visit(trigger, A<object>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_dispatch_ContentChangedTriggerV2_to_visitor()
    {
        var trigger = new ContentChangedTriggerV2();

        trigger.Accept(visitor, new object());

        A.CallTo(() => visitor.Visit(trigger, A<object>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_dispatch_CronJobTrigger_to_visitor()
    {
        var trigger = new CronJobTrigger();

        trigger.Accept(visitor, new object());

        A.CallTo(() => visitor.Visit(trigger, A<object>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_dispatch_ManualTrigger_to_visitor()
    {
        var trigger = new ManualTrigger();

        trigger.Accept(visitor, new object());

        A.CallTo(() => visitor.Visit(trigger, A<object>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_dispatch_SchemaChangedTrigger_to_visitor()
    {
        var trigger = new SchemaChangedTrigger();

        trigger.Accept(visitor, new object());

        A.CallTo(() => visitor.Visit(trigger, A<object>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_dispatch_UsageTrigger_to_visitor()
    {
        var trigger = new UsageTrigger();

        trigger.Accept(visitor, new object());

        A.CallTo(() => visitor.Visit(trigger, A<object>._)).MustHaveHappenedOnceExactly();
    }
}
