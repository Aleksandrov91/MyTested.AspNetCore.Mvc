namespace MusicStore.Test.Mocks
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Models;

    public class SignInManagerMock : SignInManager<ApplicationUser>
    {
        private static int FailedTwoFactorSignInCount = 0;

        internal const string ValidUser = "Valid@valid.com";
        internal const string TwoFactorRequired = "TwoFactor@invalid.com";
        internal const string LockedOutUser = "Locked@invalid.com";

        public SignInManagerMock(
            UserManager<ApplicationUser> userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<ApplicationUser>> logger, 
            IAuthenticationSchemeProvider schemes)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
        {
        }

        public override Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            if (userName == ValidUser && password == ValidUser)
            {
                return Task.FromResult(SignInResult.Success);
            }

            if (userName == TwoFactorRequired && password == TwoFactorRequired)
            {
                return Task.FromResult(SignInResult.TwoFactorRequired);
            }

            if (userName == LockedOutUser && password == LockedOutUser)
            {
                return Task.FromResult(SignInResult.LockedOut);
            }

            return Task.FromResult(SignInResult.Failed);
        }

        public override Task<ApplicationUser> GetTwoFactorAuthenticationUserAsync() => UserManager.GetUserAsync(Context.User);


        public override Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberClient)
        {
            const string SuccessProvider = "SuccessProvider";
            const string SuccessCode = "SuccessCode";
            const int FailedTwoFactorSignInLimitCount = 5;

            if (FailedTwoFactorSignInCount == FailedTwoFactorSignInLimitCount)
            {
                return Task.FromResult(SignInResult.LockedOut);
            }

            if (provider == SuccessProvider && code == SuccessCode)
            {
                return Task.FromResult(SignInResult.Success);
            }

            FailedTwoFactorSignInCount++;

            return Task.FromResult(SignInResult.Failed);
        }
    }
}
