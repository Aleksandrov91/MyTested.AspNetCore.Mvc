﻿namespace MyTested.AspNetCore.Mvc.Builders.ActionResults.Json
{
    using System;
    using System.Net;
    using Builders.Base;
    using Contracts.ActionResults.Json;
    using Exceptions;
    using Internal;
    using Internal.Services;
    using Internal.TestContexts;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Utilities.Extensions;
    using Utilities.Validators;

    /// <summary>
    /// Used for testing <see cref="JsonResult"/>.
    /// </summary>
    public class JsonTestBuilder : BaseTestBuilderWithResponseModel<JsonResult>, IAndJsonTestBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTestBuilder"/> class.
        /// </summary>
        /// <param name="testContext"><see cref="ControllerTestContext"/> containing data about the currently executed assertion chain.</param>
        public JsonTestBuilder(ControllerTestContext testContext)
            : base(testContext)
        {
        }

        /// <inheritdoc />
        public IAndJsonTestBuilder WithStatusCode(int statusCode)
            => this.WithStatusCode((HttpStatusCode)statusCode);

        /// <inheritdoc />
        public IAndJsonTestBuilder WithStatusCode(HttpStatusCode statusCode)
        {
            HttpStatusCodeValidator.ValidateHttpStatusCode(
                statusCode,
                this.GetJsonResult().StatusCode,
                this.ThrowNewJsonResultAssertionException);

            return this;
        }

        /// <inheritdoc />
        public IAndJsonTestBuilder WithContentType(string contentType)
        {
            ContentTypeValidator.ValidateContentType(
                contentType,
                this.GetJsonResult().ContentType,
                this.ThrowNewJsonResultAssertionException);

            return this;
        }

        /// <inheritdoc />
        public IAndJsonTestBuilder WithContentType(MediaTypeHeaderValue contentType)
            => this.WithContentType(contentType?.MediaType.Value);

        /// <inheritdoc />
        public IAndJsonTestBuilder WithDefaultJsonSerializerSettings()
            => this.WithJsonSerializerSettings(s => this.PopulateFullJsonSerializerSettingsTestBuilder(s));

        /// <inheritdoc />
        public IAndJsonTestBuilder WithJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
            => this.WithJsonSerializerSettings(s => this.PopulateFullJsonSerializerSettingsTestBuilder(s, jsonSerializerSettings));

        /// <inheritdoc />
        public IAndJsonTestBuilder WithJsonSerializerSettings(
            Action<IJsonSerializerSettingsTestBuilder> jsonSerializerSettingsBuilder)
        {
            var actualJsonSerializerSettings = this.GetJsonResult().SerializerSettings
                ?? this.GetServiceDefaultSerializerSettings()
                ?? new JsonSerializerSettings();

            var newJsonSerializerSettingsTestBuilder = new JsonSerializerSettingsTestBuilder(this.TestContext as ControllerTestContext);
            jsonSerializerSettingsBuilder(newJsonSerializerSettingsTestBuilder);
            var expectedJsonSerializerSettings = newJsonSerializerSettingsTestBuilder.GetJsonSerializerSettings();

            var validations = newJsonSerializerSettingsTestBuilder.GetJsonSerializerSettingsValidations();
            validations.ForEach(v => v(expectedJsonSerializerSettings, actualJsonSerializerSettings));

            return this;
        }

        /// <inheritdoc />
        public IJsonTestBuilder AndAlso() => this;

        public override object GetActualModel()
        {
            return this.GetJsonResult()?.Value;
        }

        /// <summary>
        /// Throws new JSON result assertion exception for the provided property name, expected value and actual value.
        /// </summary>
        /// <param name="propertyName">Property name on which the testing failed..</param>
        /// <param name="expectedValue">Expected value of the tested property.</param>
        /// <param name="actualValue">Actual value of the tested property.</param>
        protected override void ThrowNewFailedValidationException(string propertyName, string expectedValue, string actualValue)
            => this.ThrowNewJsonResultAssertionException(propertyName, expectedValue, actualValue);

        private JsonResult GetJsonResult() => this.TestContext.MethodResult as JsonResult;

        private JsonSerializerSettings GetServiceDefaultSerializerSettings()
            => TestServiceProvider.GetService<IOptions<MvcJsonOptions>>()?.Value?.SerializerSettings;

        private void PopulateFullJsonSerializerSettingsTestBuilder(
            IJsonSerializerSettingsTestBuilder jsonSerializerSettingsTestBuilder,
            JsonSerializerSettings jsonSerializerSettings = null)
        {
            jsonSerializerSettings = jsonSerializerSettings
                ?? this.GetServiceDefaultSerializerSettings()
                ?? new JsonSerializerSettings();

            jsonSerializerSettingsTestBuilder
                .WithCulture(jsonSerializerSettings.Culture)
                .WithContractResolverOfType(jsonSerializerSettings.ContractResolver?.GetType())
                .WithConstructorHandling(jsonSerializerSettings.ConstructorHandling)
                .WithDateFormatHandling(jsonSerializerSettings.DateFormatHandling)
                .WithDateParseHandling(jsonSerializerSettings.DateParseHandling)
                .WithDateTimeZoneHandling(jsonSerializerSettings.DateTimeZoneHandling)
                .WithDefaultValueHandling(jsonSerializerSettings.DefaultValueHandling)
                .WithFormatting(jsonSerializerSettings.Formatting)
                .WithMaxDepth(jsonSerializerSettings.MaxDepth)
                .WithMissingMemberHandling(jsonSerializerSettings.MissingMemberHandling)
                .WithNullValueHandling(jsonSerializerSettings.NullValueHandling)
                .WithObjectCreationHandling(jsonSerializerSettings.ObjectCreationHandling)
                .WithPreserveReferencesHandling(jsonSerializerSettings.PreserveReferencesHandling)
                .WithReferenceLoopHandling(jsonSerializerSettings.ReferenceLoopHandling)
                .WithTypeNameAssemblyFormatHandling(jsonSerializerSettings.TypeNameAssemblyFormatHandling)
                .WithTypeNameHandling(jsonSerializerSettings.TypeNameHandling);
        }

        private void ThrowNewJsonResultAssertionException(string propertyName, string expectedValue, string actualValue) 
            => throw new JsonResultAssertionException(string.Format(
                ExceptionMessages.ActionResultFormat,
                this.TestContext.ExceptionMessagePrefix,
                "JSON",
                propertyName,
                expectedValue,
                actualValue));
    }
}
