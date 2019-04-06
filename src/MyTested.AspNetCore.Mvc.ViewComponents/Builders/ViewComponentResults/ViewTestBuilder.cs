﻿namespace MyTested.AspNetCore.Mvc.Builders.ViewComponentResults
{
    using System;
    using Base;
    using Contracts.ViewComponentResults;
    using Internal.TestContexts;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.AspNetCore.Mvc.ViewEngines;
    using Utilities;
    using Exceptions;

    /// <summary>
    /// Used for testing <see cref="ViewViewComponentResult"/>.
    /// </summary>
    public class ViewTestBuilder : BaseTestBuilderWithResponseModel, IAndViewTestBuilder
    {
        private readonly ViewViewComponentResult viewResult;

        public ViewTestBuilder(ActionTestContext testContext)
            : base(testContext) 
            => this.viewResult = testContext.MethodResultAs<ViewViewComponentResult>();

        /// <inheritdoc />
        public IAndViewTestBuilder WithViewEngine(IViewEngine viewEngine)
        {
            var actualViewEngine = this.viewResult.ViewEngine;
            if (viewEngine != actualViewEngine)
            {
                throw ViewViewComponentResultAssertionException
                    .ForViewEngineEquality(this.TestContext.ExceptionMessagePrefix);
            }

            return this;
        }

        /// <inheritdoc />
        public IAndViewTestBuilder WithViewEngineOfType<TViewEngine>()
            where TViewEngine : IViewEngine
        {
            var actualViewEngineType = this.viewResult?.ViewEngine?.GetType();
            var expectedViewEngineType = typeof(TViewEngine);

            if (actualViewEngineType == null
                || Reflection.AreDifferentTypes(expectedViewEngineType, actualViewEngineType))
            {
                throw ViewViewComponentResultAssertionException.ForViewEngineType(
                    this.TestContext.ExceptionMessagePrefix,
                    expectedViewEngineType.ToFriendlyTypeName(),
                    actualViewEngineType.ToFriendlyTypeName());
            }

            return this;
        }

        /// <inheritdoc />
        public IViewTestBuilder AndAlso() => this;

        public override object GetActualModel()
            => this.TestContext.MethodResultAs<ViewViewComponentResult>()?.ViewData?.Model;

        public override Type GetModelReturnType() => this.GetActualModel()?.GetType();

        public override void ValidateNoModel()
        {
            if (this.GetActualModel() != null)
            {
                throw new ResponseModelAssertionException(string.Format(
                    "{0} to not have a view model but in fact such was found.",
                    this.TestContext.ExceptionMessagePrefix));
            }
        }
    }
}
