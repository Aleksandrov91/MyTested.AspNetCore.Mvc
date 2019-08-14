namespace MusicStore.Test.Routing
{
    using MusicStore.Controllers;
    using MusicStore.Models;
    using MyTested.AspNetCore.Mvc;
    using Xunit;

    public class AccountRouteTest
    {
        [Fact]
        public void GetLoginShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/Login")
                .To<AccountController>(c => c.Login(With.No<string>()));
        }

        [Theory]
        [InlineData("ValidUser@user.com", "1ValidPassword", true, "test.com")]
        [InlineData("ValidUser@user.com", "1ValidPassword", true, null)]
        public void PostLoginShouldBeRoutedCorrectly(string email, string password, bool rememberMe, string returnUrl)
        {
            MyRouting
                .Configuration()
                .ShouldMap(request => request
                    .WithMethod(HttpMethod.Post)
                    .WithLocation("/Account/Login")
                    .WithFormFields(new
                    {
                        Email = email,
                        Password = password,
                        RememberMe = rememberMe.ToString(),
                        ReturnUrl = returnUrl
                    })
                    .WithAntiForgeryToken())
                .To<AccountController>(c => c.Login(new LoginViewModel
                {
                    Email = email,
                    Password = password,
                    RememberMe = rememberMe
                }, returnUrl))
                .AndAlso()
                .ToValidModelState();
        }

        [Fact]
        public void GetRegisterShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/Register")
                .To<AccountController>(c => c.Register());
        }

        [Theory]
        [InlineData("ValidUser@user.com", "1ValidPassword", "1ValidPassword")]
        public void PostRegisterShouldBeRoutedCorrectly(string email, string password, string confirmPassword)
        { 
            MyRouting
                .Configuration()
                .ShouldMap(request => request
                    .WithMethod(HttpMethod.Post)
                    .WithLocation("/Account/Register")
                    .WithFormFields(new
                    {
                        Email = email,
                        Password = password,
                        ConfirmPassword = confirmPassword
                    })
                    .WithAntiForgeryToken())
                .To<AccountController>(c => c.Register(new RegisterViewModel
                {
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword
                }))
                .AndAlso()
                .ToValidModelState();
        }

        [Fact]
        public void GetVerifyCodeShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/VerifyCode")
                .To<AccountController>(c => c.VerifyCode(
                    With.No<string>(),
                    With.No<bool>(),
                    With.No<string>()));
        }

        [Fact]
        public void GetConfirmEmailShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/ConfirmEmail")
                .To<AccountController>(c => c.ConfirmEmail(
                    With.No<string>(),
                    With.No<string>()));
        }

        [Fact]
        public void GetForgotPasswordShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/ForgotPassword")
                .To<AccountController>(c => c.ForgotPassword());
        }

        [Fact]
        public void GetForgotPasswordConfirmationShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/ForgotPasswordConfirmation")
                .To<AccountController>(c => c.ForgotPasswordConfirmation());
        }

        [Fact]
        public void GetResetPasswordShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/ResetPassword")
                .To<AccountController>(c => c.ResetPassword(With.No<string>()));
        }

        [Fact]
        public void GetResetPasswordConfirmationShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/ResetPasswordConfirmation")
                .To<AccountController>(c => c.ResetPasswordConfirmation());
        }

        [Fact]
        public void GetSendCodeShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/SendCode")
                .To<AccountController>(c => c.SendCode(
                    With.No<bool>(),
                    With.No<string>()));
        }

        [Fact]
        public void GetExternalLoginCallbackShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/ExternalLoginCallback")
                .To<AccountController>(c => c.ExternalLoginCallback(With.No<string>()));
        }

        [Fact]
        public void GetExternalLoginFailureShouldBeRoutedCorrectly()
        {
            MyRouting
                .Configuration()
                .ShouldMap("/Account/ExternalLoginFailure")
                .To<AccountController>(c => c.ExternalLoginFailure());
        }

        //TODO: Test all post methods
    }
}
