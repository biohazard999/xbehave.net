﻿// <copyright file="TeardownFeature.cs" company="xBehave.net contributors">
//  Copyright (c) xBehave.net contributors. All rights reserved.
// </copyright>

#if NET40 || NET45
namespace Xbehave.Test.Acceptance
{
    using System;
#if !V2
    using System.Globalization;
#endif
    using System.Linq;
    using FluentAssertions;
    using Xbehave.Test.Acceptance.Infrastructure;
    using Xunit.Abstractions;

    // In order to release allocated resources
    // As a developer
    // I want to execute teardowns after a scenario has run
    public class TeardownFeature : Feature
    {
        [Background]
        public void Background()
        {
            "Given no events have occurred"
                .f(() => typeof(TeardownFeature).ClearTestEvents());
        }

        [Scenario]
        public void ManyTeardownsInASingleStep(Type feature, ITestResultMessage[] results)
        {
            "Given a step with many teardowns"
                .f(() => feature = typeof(StepWithManyTeardowns));

            "When running the scenario"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be two results"
                .f(() => results.Length.Should().Be(2));

            "And there should be no failures"
                .f(() => results.Should().ContainItemsAssignableTo<ITestPassed>());

            "And the first result should be generated by the step"
                .f(() => results[0].Test.DisplayName.Should().NotContainEquivalentOf("(Teardown)"));

            "And the second result should be generated by the teardown"
                .f(() => results[1].Test.DisplayName.Should().Contain("(Teardown)"));

            "Ann the teardowns should be executed in reverse order after the step"
                .f(() => typeof(TeardownFeature).GetTestEvents()
                    .Should().Equal("step1", "teardown3", "teardown2", "teardown1"));
        }

        [Scenario]
        public void TeardownsWhichThrowExceptionsWhenExecuted(Type feature, ITestResultMessage[] results)
        {
            "Given a step with three teardowns which throw exceptions when executed"
                .f(() => feature = typeof(StepWithThreeBadTeardowns));

            "When running the scenario"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be two results"
                .f(() => results.Length.Should().Be(2));

            "And the first result should be a pass"
                .f(() => results[0].Should().BeAssignableTo<ITestPassed>());

            "And the second result should be a failure"
                .f(() => results[1].Should().BeAssignableTo<ITestFailed>());

            "Then the teardowns should be executed in reverse order after the step"
                .f(() => typeof(TeardownFeature).GetTestEvents()
                    .Should().Equal("step1", "teardown3", "teardown2", "teardown1"));
        }

        [Scenario]
        public void ManyTeardownsInManySteps(Type feature, ITestResultMessage[] results)
        {
            "Given two steps with three teardowns each"
                .f(() => feature = typeof(TwoStepsWithThreeTeardownsEach));

            "When running the scenario"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be four results"
                .f(() => results.Length.Should().Be(3));

            "And there should be no failures"
                .f(() => results.Should().ContainItemsAssignableTo<ITestPassed>());

            "And the teardowns should be executed in reverse order after the steps"
                .f(() => typeof(TeardownFeature).GetTestEvents().Should().Equal(
                    "step1", "step2", "teardown6", "teardown5", "teardown4", "teardown3", "teardown2", "teardown1"));
        }

#if !V2
        [Scenario]
        public void MultipleContexts(Type feature, ITestResultMessage[] results)
        {
            "Given a step with a teardown and steps which generate two contexts"
                .f(() => feature = typeof(SingleStepTwoContexts));

            "When running the scenario"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be no failures"
                .f(() => results.Should().ContainItemsAssignableTo<ITestPassed>());

            "And the teardown should be executed after each context"
                .f(() => typeof(TeardownFeature).GetTestEvents().Should().Equal(
                    "step1.1", "step1.2", "step1.3", "teardown1.1", "step2.1", "step2.2", "step2.4", "teardown2.1"));
        }
#endif

        [Scenario]
        public void FailingSteps(Type feature, ITestResultMessage[] results)
        {
            "Given two steps with teardowns and a failing step"
                .f(() => feature = typeof(TwoStepsWithTeardownsAndAFailingStep));

            "When running the scenario"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be one failure"
                .f(() => results.OfType<ITestFailed>().Count().Should().Be(1));

            "And the teardowns should be executed after each step"
                .f(() => typeof(TeardownFeature).GetTestEvents()
                    .Should().Equal("step1", "step2", "step3", "teardown2", "teardown1"));
        }

        [Scenario]
        public void FailureToCompleteAStep(Type feature, ITestResultMessage[] results)
        {
            "Given a failing step with three teardowns"
                .f(() => feature = typeof(FailingStepWithThreeTeardowns));

            "When running the scenario"
                .f(() => results = this.Run<ITestResultMessage>(feature));

            "Then there should be one failure"
                .f(() => results.OfType<ITestFailed>().Count().Should().Be(1));

            "And the teardowns should be executed in reverse order after the step"
                .f(() => typeof(TeardownFeature).GetTestEvents()
                    .Should().Equal("step1", "teardown3", "teardown2", "teardown1"));
        }

        private static class StepWithManyTeardowns
        {
            [Scenario]
            public static void Scenario()
            {
                "Given something"
                    .f(() => typeof(TeardownFeature).SaveTestEvent("step1"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown1"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown2"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown3"));
            }
        }

        private static class StepWithThreeBadTeardowns
        {
            [Scenario]
            public static void Scenario()
            {
                "Given something"
                    .f(() => typeof(TeardownFeature).SaveTestEvent("step1"))
                    .Teardown(() =>
                    {
                        typeof(TeardownFeature).SaveTestEvent("teardown1");
                        throw new InvalidOperationException();
                    })
                    .Teardown(() =>
                    {
                        typeof(TeardownFeature).SaveTestEvent("teardown2");
                        throw new InvalidOperationException();
                    })
                    .Teardown(() =>
                    {
                        typeof(TeardownFeature).SaveTestEvent("teardown3");
                        throw new InvalidOperationException();
                    });
            }
        }

        private static class TwoStepsWithThreeTeardownsEach
        {
            [Scenario]
            public static void Scenario()
            {
                "Given something"
                    .f(() => typeof(TeardownFeature).SaveTestEvent("step1"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown1"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown2"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown3"));

                "And something else"
                    .f(() => typeof(TeardownFeature).SaveTestEvent("step2"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown4"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown5"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown6"));
            }
        }

#if !V2
        private static class SingleStepTwoContexts
        {
            private static int context;

            [Scenario]
            public static void Scenario()
            {
                "Given something"
                    .f(() =>
                    {
                        ++context;
                        typeof(TeardownFeature).SaveTestEvent(
                            string.Concat("step", context.ToString(CultureInfo.InvariantCulture), ".1"));
                    })
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent(
                        string.Concat("teardown", context.ToString(CultureInfo.InvariantCulture), ".1")));

                "When something happens"
                    .f(() => typeof(TeardownFeature).SaveTestEvent(
                        string.Concat("step", context.ToString(CultureInfo.InvariantCulture), ".2")));

                "Then something"
                    .f(() => typeof(TeardownFeature).SaveTestEvent(
                        string.Concat("step", context.ToString(CultureInfo.InvariantCulture), ".3")))
                    .InIsolation();

                "And something"
                    .f(() => typeof(TeardownFeature).SaveTestEvent(
                        string.Concat("step", context.ToString(CultureInfo.InvariantCulture), ".4")));
            }
        }
#endif

        private static class TwoStepsWithTeardownsAndAFailingStep
        {
            [Scenario]
            public static void Scenario()
            {
                "Given something"
                    .f(() => typeof(TeardownFeature).SaveTestEvent("step1"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown1"));

                "When something"
                    .f(() => typeof(TeardownFeature).SaveTestEvent("step2"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown2"));

                "Then something happens"
                    .f(() =>
                    {
                        typeof(TeardownFeature).SaveTestEvent("step3");
                        1.Should().Be(0);
                    });
            }
        }

        private static class FailingStepWithThreeTeardowns
        {
            [Scenario]
            public static void Scenario()
            {
                "Given something"
                    .f(() =>
                    {
                        typeof(TeardownFeature).SaveTestEvent("step1");
                        throw new InvalidOperationException();
                    })
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown1"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown2"))
                    .Teardown(() => typeof(TeardownFeature).SaveTestEvent("teardown3"));
            }
        }
    }
}
#endif
