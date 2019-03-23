﻿namespace MyTested.AspNetCore.Mvc
{
    using Builders.ActionResults.Json;
    using Builders.Actions.ShouldReturn;
    using Builders.Contracts.ActionResults.Json;
    using Builders.Contracts.Actions;
    using Microsoft.AspNetCore.Mvc;
    using Utilities.Validators;

    /// <summary>
    /// Contains <see cref="JsonResult"/> extension methods for <see cref="IShouldReturnTestBuilder{TActionResult}"/>.
    /// </summary>
    public static class ShouldReturnTestBuilderJsonResultExtensions
    {
        /// <summary>
        /// Tests whether the action result is <see cref="JsonResult"/>.
        /// </summary>
        /// <typeparam name="TActionResult">Type of action result type.</typeparam>
        /// <param name="builder">Instance of <see cref="IShouldReturnTestBuilder{TActionResult}"/> type.</param>
        /// <returns>Test builder of <see cref="IAndJsonTestBuilder"/> type.</returns>
        public static IAndJsonTestBuilder Json<TActionResult>(this IShouldReturnTestBuilder<TActionResult> builder)
        {
            var actualShouldReturnTestBuilder = (ShouldReturnTestBuilder<TActionResult>)builder;

            InvocationResultValidator.ValidateInvocationResultType<JsonResult>(actualShouldReturnTestBuilder.TestContext);

            return new JsonTestBuilder(actualShouldReturnTestBuilder.TestContext);
        }
    }
}
