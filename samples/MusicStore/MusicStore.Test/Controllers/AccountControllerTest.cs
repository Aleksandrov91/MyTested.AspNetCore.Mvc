namespace MusicStore.Test.Controllers
{
    using Microsoft.Extensions.Caching.Memory;
    using Mocks;
    using Models;
    using MusicStore.Controllers;
    using MyTested.AspNetCore.Mvc;
    using Xunit;

    using HttpMethod = System.Net.Http.HttpMethod;

    public class AccountControllerTest
    {
        private const string ValidUserId = "ValidUserId";
        private const string ValidToken = "ValidToken";

        [Fact]
        public void AccountControllerShouldHaveAuthorizeFilter()
        {
            MyMvc
                .Controller<AccountController>()
                .ShouldHave()
                .Attributes(attrs => attrs
                    .RestrictingForAuthorizedRequests());
        }

        [Fact]
        public void GetLoginShouldHaveAllowAnonymousFilter()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(With.No<string>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .AllowingAnonymousRequests());
        }

        [Fact]
        public void GetLoginShouldHaveCorrectViewBagEntriesWithReturnUrlAndShouldReturnCorrectView()
        {
            const string returnUrl = "MyReturnUrl";

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(returnUrl))
                .ShouldHave()
                .ViewBag(viewBag => viewBag
                    .ContainingEntry("ReturnUrl", returnUrl))
                .AndAlso()
                .ShouldReturn()
                .View();
        }

        [Fact]
        public void PostLoginShouldHaveCorrectActionFilters()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(
                    With.Default<LoginViewModel>(),
                    With.No<string>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethod(HttpMethod.Post)
                    .AllowingAnonymousRequests()
                    .ValidatingAntiForgeryToken());
        }

        [Fact]
        public void PostLoginShouldReturnDefaultViewWithInvalidModel()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(
                    With.Default<LoginViewModel>(),
                    With.No<string>()))
                .ShouldHave()
                .ModelState(modelState => modelState
                    .For<LoginViewModel>()
                    .ContainingErrorFor(m => m.Email)
                    .ContainingErrorFor(m => m.Password))
                .AndAlso()
                .ShouldReturn()
                .View();
        }

        [Fact]
        public void PostLoginShouldReturnRedirectToActionWithValidUserName()
        {
            var model = new LoginViewModel
            {
                Email = SignInManagerMock.ValidUser,
                Password = SignInManagerMock.ValidUser
            };
            
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(
                    model,
                    With.No<string>()))
                .ShouldReturn()
                .Redirect(redirect => redirect
                    .To<HomeController>(c => c.Index(
                        With.No<MusicStoreContext>(),
                        With.No<IMemoryCache>())));
        }
        
        [Fact]
        public void PostLoginShouldReturnRedirectToLocalWithValidUserNameAndReturnUrl()
        {
            var model = new LoginViewModel
            {
                Email = SignInManagerMock.ValidUser,
                Password = SignInManagerMock.ValidUser
            };

            var returnUrl = "/Store/Index";

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(
                    model,
                    returnUrl))
                .ShouldReturn()
                .Redirect(redirect => redirect
                    .ToUrl(returnUrl));
        }
        
        [Fact]
        public void PostLoginShouldReturnRedirectWithTwoFactor()
        {
            var model = new LoginViewModel
            {
                Email = SignInManagerMock.TwoFactorRequired,
                Password = SignInManagerMock.TwoFactorRequired,
                RememberMe = true
            };

            var returnUrl = "/Store/Index";

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(
                    model,
                    returnUrl))
                .ShouldReturn()
                .Redirect(redirect => redirect
                    .To<AccountController>(c => c.SendCode(model.RememberMe, returnUrl)));
        }
        
        [Fact]
        public void PostLoginShouldReturnViewWithLockout()
        {
            var model = new LoginViewModel
            {
                Email = SignInManagerMock.LockedOutUser,
                Password = SignInManagerMock.LockedOutUser
            };

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(
                    model,
                    With.No<string>()))
                .ShouldReturn()
                .View("Lockout");
        }

        [Fact]
        public void PostLoginShouldReturnReturnViewWithInvalidCredentials()
        {
            var model = new LoginViewModel
            {
                Email = "Invalid@invalid.com",
                Password = "Invalid"
            };

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Login(
                    model,
                    With.No<string>()))
                .ShouldHave()
                .ModelState(modelState => modelState
                    .For<ValidationSummary>()
                    .ContainingError(string.Empty)
                    .ThatEquals("Invalid login attempt."))
                .AndAlso()
                .ShouldReturn()
                .View(model);
        }

        [Fact]
        public void GetVerifyCodeShouldHaveAllowAnonymosActionFilter()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.VerifyCode(With.No<string>(), With.No<bool>(), With.No<string>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                        .AllowingAnonymousRequests());
        }

        [Fact]
        public void GetVerifyCodeWithoutUserShouldReturnErrorView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.VerifyCode(With.No<string>(), With.No<bool>(), With.No<string>()))
                .ShouldReturn()
                .View("Error");
        }

        [Fact]
        public void GetVerifyCodeWithUserShouldReturnCorrectView()
        {
            var model = new VerifyCodeViewModel
            {
                Provider = "MyTest.com",
                RememberMe = true,
                ReturnUrl = "MyReturnUrl"
            };

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.VerifyCode(model.Provider, model.RememberMe, model.ReturnUrl))
                .ShouldReturn()
                .View(model);
        }

        [Fact]
        public void PostVerifyCodeShouldHaveCorrectActionAttributes()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.VerifyCode(With.Default<VerifyCodeViewModel>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethod(HttpMethod.Post)
                    .AllowingAnonymousRequests()
                    .ValidatingAntiForgeryToken());
        }

        [Fact]
        public void PostVerifyCodeWithInvalidModelShouldContainErrorsAndReturnView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.VerifyCode(With.Default<VerifyCodeViewModel>()))
                .ShouldHave()
                .ModelState(modelstate => modelstate
                    .For<VerifyCodeViewModel>()
                    .ContainingErrorFor(m => m.Code)
                    .ContainingErrorFor(m => m.Provider))
                .AndAlso()
                .ShouldReturn()
                .View(With.No<VerifyCodeViewModel>());
        }

        [Fact]
        public void PostVerifyCodeWithoutCodeShouldContainModelStateErrorAndReturnView()
        {
            var model = new VerifyCodeViewModel
            {
                Provider = "MyProvider",
                ReturnUrl = "MyReturnUrl"
            };

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.VerifyCode(model))
                .ShouldHave()
                .ModelState(modelstate => modelstate
                    .For<VerifyCodeViewModel>()
                    .ContainingErrorFor(m => m.Code))
                .AndAlso()
                .ShouldReturn()
                .View(model);
        }

        [Fact]
        public void GetRegisterShouldHaveAllowAnonymosFilter()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Register())
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .AllowingAnonymousRequests());
        }

        [Fact]
        public void GetRegisterShouldReturnView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Register())
                .ShouldReturn()
                .View();
        }

        [Fact]
        public void PostRegisterShouldHaveCorrectActionFilters()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Register(With.Default<RegisterViewModel>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethod(HttpMethod.Post)
                    .AllowingAnonymousRequests()
                    .ValidatingAntiForgeryToken());
        }

        [Fact]
        public void PostRegisterShouldReturnDefaultViewWithInvalidModel()
        {
            var model = new RegisterViewModel
            {
                Email = "Invalid@invalid.com",
                Password = "Invalid",
                ConfirmPassword = "Not Valid"
            };

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Register(model))
                .ShouldHave()
                .ModelState(modelstate => modelstate
                    .For<RegisterViewModel>()
                    .ContainingErrorFor(m => m.ConfirmPassword))
                .AndAlso()
                .ShouldReturn()
                .View(model);

        }

        [Fact]
        public void PostRegisterWithValidModelShouldReturnRedirectToAction()
        {
            var model = new RegisterViewModel
            {
                Email = "Valid@valid.com",
                Password = "ValidPassword#123",
                ConfirmPassword = "ValidPassword#123"
            };

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.Register(model))
                .ShouldReturn()
                .Redirect(redirect => redirect
                    .To<HomeController>(c => c.Index(
                        With.No<MusicStoreContext>(),
                        With.No<IMemoryCache>())));
        }

        [Fact]
        public void GetConfirmEmailShouldHaveAllowAnonymosFilter()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ConfirmEmail(With.No<string>(), With.No<string>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .AllowingAnonymousRequests());
        }

        [Fact]
        public void GetConfirmEmailWithInvalidParametersShouldReturnErrorView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ConfirmEmail(With.No<string>(), With.No<string>()))
                .ShouldReturn()
                .View("Error");
        }

        [Fact]
        public void GetConfirmEmailWithValidUserIdAndNoCodeShouldReturnErrorView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ConfirmEmail(ValidUserId, With.No<string>()))
                .ShouldReturn()
                .View("Error");
        }

        [Fact]
        public void GetConfirmEmailWithCodeANdNoUserShouldReturnErrorView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ConfirmEmail(With.No<string>(), ValidToken))
                .ShouldReturn()
                .View("Error");
        }

        [Fact]
        public void GetConfirmEmailWithInvalidUserAndValidTokenShouldReturnErrorView()
        {
            var invalidUser = "InvalidUser";

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ConfirmEmail(invalidUser, ValidToken))
                .ShouldReturn()
                .View("Error");
        }

        [Fact]
        public void GetConfirmEmailWithInvalidCodeShouldReturnView()
        {
            var invalidCode = "InvalidCode";

            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ConfirmEmail(ValidUserId, invalidCode))
                .ShouldReturn()
                .View("Error");
        }

        [Fact]
        public void GetConfirmEmailWithValidParametersShouldReturnView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ConfirmEmail(ValidUserId, ValidToken))
                .ShouldReturn()
                .View("ConfirmEmail");
        }

        [Fact]
        public void GetForgotPasswordShouldHaveAllowAnonymosFilter()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ForgotPassword())
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .AllowingAnonymousRequests());
        }

        [Fact]
        public void GetForgotPasswordShouldReturnView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ForgotPassword())
                .ShouldReturn()
                .View();
        }

        [Fact]
        public void PostForgotPasswordShouldHaveCorrectActionAttributes()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ForgotPassword(With.Default<ForgotPasswordViewModel>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethods(HttpMethod.Post)
                    .AllowingAnonymousRequests()
                    .ValidatingAntiForgeryToken());
        }

        [Fact]
        public void PostLogOffShouldHaveCorrectActionAttributes()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.LogOff())
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethods(HttpMethod.Post)
                    .ValidatingAntiForgeryToken());
        }

        [Fact]
        public void PostLogOffShouldRedirectToHome()
        {
            MyMvc
                .Controller<AccountController>()
                .WithUser("Administrator@test.com")
                .Calling(c => c.LogOff())
                .ShouldReturn()
                .RedirectToAction("Index");
        }

        [Fact]
        public void PostLogOffShouldClearUserFromContext()
        {
            //TODO
        }

        [Fact]
        public void PostLogOffWithoutAuthorizedUserShouldReturnError()
        {
            //TODO
        }

        [Fact]
        public void GetExternalLoginFailureHaveAllowAnonymosActionAttribute()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ExternalLoginFailure())
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .AllowingAnonymousRequests());
        }

        [Fact]
        public void GetExternalLoginFaiulreShouldReturnView()
        {
            MyMvc
                .Controller<AccountController>()
                .Calling(c => c.ExternalLoginFailure())
                .ShouldReturn()
                .View();
        }
    }
}
