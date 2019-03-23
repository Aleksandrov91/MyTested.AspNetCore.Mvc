﻿namespace MyTested.AspNetCore.Mvc
{
    using System;
    using Builders.ActionResults.View;
    using Builders.Actions.ShouldReturn;
    using Builders.Contracts.ActionResults.View;
    using Builders.Contracts.Actions;
    using Microsoft.AspNetCore.Mvc;
    using Utilities;
    using Utilities.Validators;
    using Exceptions;

    /// <summary>
    /// Contains <see cref="ViewComponentResult"/> extension methods for <see cref="IShouldReturnTestBuilder{TActionResult}"/>.
    /// </summary>
    public static class ShouldReturnTestBuilderViewComponentResultExtensions
    {
        /// <summary>
        /// Tests whether the action result is <see cref="ViewComponentResult"/>.
        /// </summary>
        /// <typeparam name="TActionResult">Type of action result type.</typeparam>
        /// <param name="builder">Instance of <see cref="IShouldReturnTestBuilder{TActionResult}"/> type.</param>
        /// <returns>Test builder of <see cref="IAndViewComponentTestBuilder"/> type.</returns>
        public static IAndViewComponentTestBuilder ViewComponent<TActionResult>(this IShouldReturnTestBuilder<TActionResult> builder)
        {
            var actualShouldReturnTestBuilder = (ShouldReturnTestBuilder<TActionResult>)builder;
            
            InvocationResultValidator.ValidateInvocationResultType<ViewComponentResult>(actualShouldReturnTestBuilder.TestContext);

            return new ViewComponentTestBuilder(actualShouldReturnTestBuilder.TestContext);
        }

        /// <summary>
        /// Tests whether the action result is <see cref="ViewComponentResult"/> with the provided view component name.
        /// </summary>
        /// <typeparam name="TActionResult">Type of action result type.</typeparam>
        /// <param name="builder">Instance of <see cref="IShouldReturnTestBuilder{TActionResult}"/> type.</param>
        /// <param name="viewComponentName">Expected view component name.</param>
        /// <returns>Test builder of <see cref="IAndViewComponentTestBuilder"/> type.</returns>
        public static IAndViewComponentTestBuilder ViewComponent<TActionResult>(
            this IShouldReturnTestBuilder<TActionResult> builder,
            string viewComponentName)
        {
            var actualShouldReturnTestBuilder = (ShouldReturnTestBuilder<TActionResult>)builder;

            var viewComponentResult = InvocationResultValidator
                .GetInvocationResult<ViewComponentResult>(actualShouldReturnTestBuilder.TestContext);

            var actualViewComponentName = viewComponentResult.ViewComponentName;

            if (viewComponentName != actualViewComponentName)
            {
                throw ViewResultAssertionException.ForNameEquality(
                    actualShouldReturnTestBuilder.TestContext.ExceptionMessagePrefix,
                    "view component",
                    viewComponentName,
                    actualViewComponentName);
            }
            
            return new ViewComponentTestBuilder(actualShouldReturnTestBuilder.TestContext);
        }

        /// <summary>
        /// Tests whether the action result is <see cref="ViewComponentResult"/> with the provided view component type.
        /// </summary>
        /// <typeparam name="TActionResult">Type of action result type.</typeparam>
        /// <param name="builder">Instance of <see cref="IShouldReturnTestBuilder{TActionResult}"/> type.</param>
        /// <param name="viewComponentType">Expected view component type.</param>
        /// <returns>Test builder of <see cref="IAndViewComponentTestBuilder"/> type.</returns>
        public static IAndViewComponentTestBuilder ViewComponent<TActionResult>(
            this IShouldReturnTestBuilder<TActionResult> builder,
            Type viewComponentType)
        {
            var actualShouldReturnTestBuilder = (ShouldReturnTestBuilder<TActionResult>)builder;

            var viewComponentResult = InvocationResultValidator
                .GetInvocationResult<ViewComponentResult>(actualShouldReturnTestBuilder.TestContext);

            var actualViewComponentType = viewComponentResult.ViewComponentType;

            if (viewComponentType != actualViewComponentType)
            {
                throw ViewResultAssertionException.ForNameEquality(
                    actualShouldReturnTestBuilder.TestContext.ExceptionMessagePrefix,
                    "view component",
                    viewComponentType.ToFriendlyTypeName(),
                    actualViewComponentType.ToFriendlyTypeName());
            }
            
            return new ViewComponentTestBuilder(actualShouldReturnTestBuilder.TestContext);
        }
    }
}
