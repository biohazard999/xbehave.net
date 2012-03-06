﻿// <copyright file="GivenWhenThenExtensions.cs" company="Adam Ralph">
//  Copyright (c) Adam Ralph. All rights reserved.
// </copyright>

namespace Xbehave
{
    using System;
    using System.Collections.Generic;

    using Xbehave.Fluent;

    /// <summary>
    /// Provides extensions for a fluent specification syntax.
    /// </summary>
    public static class GivenWhenThenExtensions
    {
        /// <summary>
        /// Records the arrangement for this specification.
        /// </summary>
        /// <param name="message">A message describing the arrangment.</param>
        /// <param name="arrange">The action that will perform the arrangment.</param>
        /// <returns>An instance of <see cref="IGiven"/>.</returns>
        public static IGiven Given(this string message, Action arrange)
        {
            var context = Core.SpecificationContext.Given(
                message,
                () =>
                {
                    arrange();
                    return DisposableAction.None;
                });

            return new Given(context);
        }

        /// <summary>
        /// Records the disposable arrangement for this specification which will be disposed after all associated assertions have been executed.
        /// </summary>
        /// <param name="message">A message describing the arrangment.</param>
        /// <param name="arrange">The function that will perform and return the arrangement.</param>
        /// <returns>An instance of <see cref="IGiven"/>.</returns>
        public static IGiven Given(this string message, Func<IDisposable> arrange)
        {
            var context = Core.SpecificationContext.Given(
                message,
                () => arrange());

            return new Given(context);
        }

        /// <summary>
        /// Records the disposable arrangement for this specification which will be disposed after all associated assertions have been executed.
        /// </summary>
        /// <param name="message">A message describing the arrangment.</param>
        /// <param name="arrange">The function that will perform and return the arrangement.</param>
        /// <returns>An instance of <see cref="IGiven"/>.</returns>
        public static IGiven Given(this string message, Func<IEnumerable<IDisposable>> arrange)
        {
            var context = Core.SpecificationContext.Given(
                message,
                () =>
                {
                    var disposables = arrange();
                    return new Disposable(disposables);
                });

            return new Given(context);
        }

        /// <summary>
        /// Records the disposable arrangement for this specification which will be disposed after all associated assertions have been executed.
        /// </summary>
        /// <param name="message">A message describing the arrangment.</param>
        /// <param name="arrange">The action that will perform the arrangement.</param>
        /// <param name="dispose">The action that will dispose the arrangement.</param>
        /// <returns>An instance of <see cref="IGiven"/>.</returns>
        public static IGiven Given(this string message, Action arrange, Action dispose)
        {
            var context = Core.SpecificationContext.Given(
                message,
                () =>
                {
                    arrange();
                    return new Disposable(dispose);
                });

            return new Given(context);
        }

        /// <summary>
        /// Deprecated in version 0.4.0.
        /// </summary>
        /// <param name="message">A message describing the arrangment.</param>
        /// <param name="arrange">The function that will perform and return the arrangement.</param>
        /// <returns>An instance of <see cref="ISpecificationPrimitive"/>.</returns>
        [Obsolete("Use Given(Func<IDisposable>) instead.")]
        public static ISpecificationPrimitive GivenDisposable(this string message, ContextDelegate arrange)
        {
            return message.ContextFixture(arrange);
        }

        /// <summary>
        /// Records the act to be performed on the arrangment for this specification.
        /// </summary>
        /// <param name="message">A message describing the act.</param>
        /// <param name="act">The action that will perform the act.</param>
        /// <returns>An instance of <see cref="IWhen"/>.</returns>
        public static IWhen When(this string message, Action act)
        {
            return new When(message.Do(act));
        }

        /// <summary>
        /// Records an assertion of an expected outcome for this specification, to be executed on an isolated arrangement and action.
        /// </summary>
        /// <param name="message">A message describing the assertion.</param>
        /// <param name="assert">The action which will perform the assertion.</param>
        /// <returns>An instance of <see cref="IThen"/>.</returns>
        public static IThen ThenInIsolation(this string message, Action assert)
        {
            return new Then(message.Assert(assert));
        }

        /// <summary>
        /// Records an assertion of an expected outcome for this specification, to be executed on a shared arrangement and action.
        /// </summary>
        /// <param name="message">A message describing the assertion.</param>
        /// <param name="assert">The action which will perform the assertion.</param>
        /// <returns>An instance of <see cref="IThen"/>.</returns>
        public static IThen Then(this string message, Action assert)
        {
            return new Then(message.Observation(assert));
        }

        /// <summary>
        /// Records a skipped assertion of an expected outcome for this specification.
        /// </summary>
        /// <param name="message">A message describing the assertion.</param>
        /// <param name="assert">The action which would have performed the assertion.</param>
        /// <returns>An instance of <see cref="IThen"/>.</returns>
        /// <remarks>
        /// This is the equivalent of <see cref="Xunit.FactAttribute.Skip"/>.
        /// E.g. <code>[Fact(Skip = "Work in progress.")]</code>.
        /// </remarks>
        public static IThen ThenSkip(this string message, Action assert)
        {
            return new Then(message.Todo(assert));
        }

        private class Disposable : IDisposable
        {
            private readonly Action dispose;
            private readonly IEnumerable<IDisposable> disposables;

            public Disposable(Action dispose)
            {
                this.dispose = dispose;
            }

            public Disposable(IEnumerable<IDisposable> disposables)
            {
                this.disposables = disposables;
            }

            public void Dispose()
            {
                if (this.dispose != null)
                {
                    this.dispose();
                }

                if (this.disposables != null)
                {
                    foreach (var disposable in this.disposables)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
