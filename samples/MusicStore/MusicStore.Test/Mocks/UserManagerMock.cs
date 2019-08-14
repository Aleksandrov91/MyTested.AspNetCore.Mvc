using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MusicStore.Test.Mocks
{
    public class UserManagerMock : UserManager<ApplicationUser>
    {
        internal const string ValidPassword = "ValidPassword#123";
        internal const string ValidUserId = "ValidUserId";
        internal const string ValidToken = "ValidToken";

        public UserManagerMock(IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            user.SecurityStamp = Guid.NewGuid().ToString();

            if (user.Email == SignInManagerMock.ValidUser && password == ValidPassword)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            return Task.FromResult(IdentityResult.Failed(new IdentityError() { Description = "Invalid user" }));
        }

        public override Task<ApplicationUser> GetUserAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity.Name == SignInManagerMock.ValidUser)
            {
                return Task.FromResult(new ApplicationUser
                {
                    UserName = SignInManagerMock.ValidUser,
                    SecurityStamp = Guid.NewGuid().ToString()
                });
            }

            return Task.FromResult<ApplicationUser>(null);
        }

        public override Task<ApplicationUser> FindByIdAsync(string userId)
        {
            if (userId == ValidUserId)
            {
                return Task.FromResult(new ApplicationUser()
                {
                    Email = SignInManagerMock.ValidUser,
                    Id = ValidUserId,
                    SecurityStamp = Guid.NewGuid().ToString()
                });
            }

            return Task.FromResult<ApplicationUser>(null);
        }

        public override Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            if (user.Id == ValidUserId && token == ValidToken)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            return Task.FromResult(IdentityResult.Failed(new IdentityError() { Description = "Invalid token." }));
        }
    }
}