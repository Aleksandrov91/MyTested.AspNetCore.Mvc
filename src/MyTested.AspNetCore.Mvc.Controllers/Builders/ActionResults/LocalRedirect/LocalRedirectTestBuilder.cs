﻿namespace MyTested.AspNetCore.Mvc.Builders.ActionResults.LocalRedirect
{
    using System;
    using Builders.Base;
    using Contracts.ActionResults.LocalRedirect;
    using Contracts.Uri;
    using Exceptions;
    using Internal;
    using Internal.TestContexts;
    using Microsoft.AspNetCore.Mvc;
    using Utilities.Validators;

    /// <summary>
    /// Used for testing <see cref="LocalRedirectResult"/>.
    /// </summary>
    public class LocalRedirectTestBuilder : BaseTestBuilderWithActionResult<LocalRedirectResult>,
        IAndLocalRedirectTestBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalRedirectTestBuilder"/> class.
        /// </summary>
        /// <param name="testContext"><see cref="ControllerTestContext"/> containing data about the currently executed assertion chain.</param>
        public LocalRedirectTestBuilder(ControllerTestContext testContext)
            : base(testContext)
        {
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder Permanent()
        {
            if (!this.ActionResult.Permanent)
            {
                this.ThrowNewRedirectResultAssertionException(
                    "to",
                    "be permanent",
                    "in fact it was not");
            }

            return this;
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder ToUrl(string localUrl)
        {
            var uri = LocationValidator.ValidateAndGetWellFormedUriString(
                localUrl,
                this.ThrowNewRedirectResultAssertionException);

            return this.ToUrl(uri);
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder ToUrlPassing(Action<string> assertions)
        {
            assertions(this.ActionResult.Url);
            return this;
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder ToUrlPassing(Func<string, bool> predicate)
        {
            var url = this.ActionResult.Url;
            if (!predicate(url))
            {
                this.ThrowNewRedirectResultAssertionException(
                    $"location ('{url}')",
                    "to pass the given predicate",
                    "it failed");
            }

            return this;
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder ToUrl(Action<IUriTestBuilder> uriTestBuilder)
        {
            LocationValidator.ValidateLocation(
                this.ActionResult,
                uriTestBuilder,
                this.ThrowNewRedirectResultAssertionException);

            return this;
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder ToUrl(Uri localUrl)
        {
            LocationValidator.ValidateUri(
                this.ActionResult,
                localUrl.OriginalString,
                this.ThrowNewRedirectResultAssertionException);

            return this;
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder WithUrlHelper(IUrlHelper urlHelper)
        {
            RouteActionResultValidator.ValidateUrlHelper(
                this.ActionResult,
                urlHelper,
                this.ThrowNewRedirectResultAssertionException);

            return this;
        }

        /// <inheritdoc />
        public IAndLocalRedirectTestBuilder WithUrlHelperOfType<TUrlHelper>()
            where TUrlHelper : IUrlHelper
        {
            RouteActionResultValidator.ValidateUrlHelperOfType<TUrlHelper>(
                this.ActionResult,
                this.ThrowNewRedirectResultAssertionException);

            return this;
        }

        /// <inheritdoc />
        public ILocalRedirectTestBuilder AndAlso() => this;
        
        public void ThrowNewRedirectResultAssertionException(string propertyName, string expectedValue, string actualValue) 
            => throw new RedirectResultAssertionException(string.Format(
                ExceptionMessages.ActionResultFormat,
                this.TestContext.ExceptionMessagePrefix,
                "local redirect",
                propertyName,
                expectedValue,
                actualValue));
    }
}
