using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AIChatBase.Authentication
{
    public class AuthUtils
    {
        public static async Task<ClaimsPrincipal> GetAuthUser(AuthenticationStateProvider provider)
        {
            var authUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = await provider.GetAuthenticationStateAsync();
            if (authState != null && authState.User != null)
            {
                authUser = authState.User;
            }
            return authUser;
        }
    }
}
