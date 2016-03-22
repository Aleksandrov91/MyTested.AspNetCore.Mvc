﻿namespace MyTested.Mvc.Test.BuildersTests.ModelsTests
{
    using Exceptions;
    using Setups;
    using Setups.Controllers;
    using Setups.Models;
    using Xunit;
    
    public class ModelErrorDetailsTestBuilderTests
    {
        [Fact]
        public void ThatEqualsShouldNotThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            MyMvc
                .Controller<MvcController>()
                .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                .ShouldHave()
                .ModelStateFor<RequestModel>(modelState => modelState
                    .ContainingNoErrorFor(m => m.NonRequiredString)
                    .ContainingErrorFor(m => m.RequiredString).ThatEquals("The RequiredString field is required.")
                    .AndAlso()
                    .ContainingErrorFor(m => m.RequiredString)
                    .AndAlso()
                    .ContainingNoErrorFor(m => m.NotValidateInteger)
                    .AndAlso()
                    .ContainingError("RequiredString")
                    .ContainingErrorFor(m => m.Integer).ThatEquals($"The field Integer must be between {1} and {int.MaxValue}.")
                    .ContainingError("RequiredString")
                    .ContainingError("Integer")
                    .ContainingNoErrorFor(m => m.NotValidateInteger));
        }

        [Fact]
        public void ThatEqualsShouldThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            Test.AssertException<ModelErrorAssertionException>(
                () =>
                {
                    MyMvc
                        .Controller<MvcController>()
                        .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                        .ShouldHave()
                        .ModelStateFor<RequestModel>(modelState => modelState
                            .ContainingNoErrorFor(m => m.NonRequiredString)
                            .AndAlso()
                            .ContainingErrorFor(m => m.RequiredString).ThatEquals("RequiredString field is required.")
                            .ContainingErrorFor(m => m.Integer).ThatEquals(string.Format("Integer must be between {0} and {1}.", 1, int.MaxValue)));
                }, 
                "When calling ModelStateCheck action in MvcController expected error message for key RequiredString to be 'RequiredString field is required.', but instead found 'The RequiredString field is required.'.");
        }

        [Fact]
        public void BeginningWithShouldNotThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            MyMvc
                .Controller<MvcController>()
                .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                .ShouldHave()
                .ModelStateFor<RequestModel>(modelState => modelState
                    .ContainingNoErrorFor(m => m.NonRequiredString)
                    .ContainingErrorFor(m => m.RequiredString).BeginningWith("The RequiredString")
                    .ContainingErrorFor(m => m.Integer).BeginningWith("The field Integer"));
        }

        [Fact]
        public void BeginningWithShouldThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            Test.AssertException<ModelErrorAssertionException>(
                () =>
                {
                    MyMvc
                        .Controller<MvcController>()
                        .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                        .ShouldHave()
                        .ModelStateFor<RequestModel>(modelState => modelState
                            .ContainingNoErrorFor(m => m.NonRequiredString)
                            .ContainingErrorFor(m => m.RequiredString).BeginningWith("RequiredString")
                            .ContainingErrorFor(m => m.Integer).BeginningWith("Integer"));
                }, 
                "When calling ModelStateCheck action in MvcController expected error message for key 'RequiredString' to begin with 'RequiredString', but instead found 'The RequiredString field is required.'.");
        }

        [Fact]
        public void EngingWithShouldNotThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            MyMvc
                .Controller<MvcController>()
                .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                .ShouldHave()
                .ModelStateFor<RequestModel>(modelState => modelState
                    .ContainingNoErrorFor(m => m.NonRequiredString)
                    .ContainingErrorFor(m => m.RequiredString).EndingWith("required.")
                    .ContainingErrorFor(m => m.Integer).EndingWith($"{1} and {int.MaxValue}."));
        }

        [Fact]
        public void EngingWithShouldThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            Test.AssertException<ModelErrorAssertionException>(
                () =>
                {
                    MyMvc
                        .Controller<MvcController>()
                        .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                        .ShouldHave()
                        .ModelStateFor<RequestModel>(modelState => modelState
                            .ContainingNoErrorFor(m => m.NonRequiredString)
                            .ContainingErrorFor(m => m.RequiredString).EndingWith("required!")
                            .ContainingErrorFor(m => m.Integer).EndingWith($"{1} and {int.MaxValue}!"));
                }, 
                "When calling ModelStateCheck action in MvcController expected error message for key 'RequiredString' to end with 'required!', but instead found 'The RequiredString field is required.'.");
        }

        [Fact]
        public void ContainingShouldNotThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            MyMvc
                .Controller<MvcController>()
                .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                .ShouldHave()
                .ModelStateFor<RequestModel>(modelState => modelState
                    .ContainingNoErrorFor(m => m.NonRequiredString)
                    .ContainingErrorFor(m => m.RequiredString).Containing("required")
                    .ContainingErrorFor(m => m.Integer).Containing("between"));
        }

        [Fact]
        public void ContainingShouldThrowExceptionWhenProvidedMessageIsValid()
        {
            var requestModelWithErrors = TestObjectFactory.GetRequestModelWithErrors();

            Test.AssertException<ModelErrorAssertionException>(
                () =>
                {
                    MyMvc
                        .Controller<MvcController>()
                        .Calling(c => c.ModelStateCheck(requestModelWithErrors))
                        .ShouldHave()
                        .ModelStateFor<RequestModel>(modelState => modelState
                            .ContainingNoErrorFor(m => m.NonRequiredString)
                            .ContainingErrorFor(m => m.RequiredString).Containing("invalid")
                            .ContainingErrorFor(m => m.Integer).Containing("invalid"));
                },
                "When calling ModelStateCheck action in MvcController expected error message for key 'RequiredString' to contain 'invalid', but instead found 'The RequiredString field is required.'.");
        }

        [Fact]
        public void NestedModelsShouldBeResolvedCorrectlyWithModelStateFor()
        {
            MyMvc
                .Controller<MvcController>()
                .Calling(c => c.ModelStateWithNestedError())
                .ShouldHave()
                .ModelStateFor<NestedModel>(modelState => modelState
                    .ContainingErrorFor(m => m.Nested.Integer)
                    .ContainingErrorFor(m => m.Nested.String));
        }
    }
}
