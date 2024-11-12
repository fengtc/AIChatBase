using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using static DevExpress.Data.Filtering.Helpers.SubExprHelper.CriteriaTokens;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Data.Services.OssId;


namespace AIChatBase.Authentication
{


    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState authenticationState;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorage;
        private readonly IOssIdService _ossIdService;
        private const string authToken = "oss_token";
        private CustomAuthenticationService _customAuthenticationService;


        public CustomAuthenticationStateProvider(CustomAuthenticationService customAuthenticationService, IConfiguration configuration, ILocalStorageService localStorageService, IOssIdService ossIdService)
        {


            _localStorage = localStorageService;
            _ossIdService = ossIdService;
            authenticationState = new AuthenticationState(customAuthenticationService.CurrentUser);
            _customAuthenticationService = customAuthenticationService;
            customAuthenticationService.UserChanged += (newUser) =>
            {
                authenticationState = new AuthenticationState(newUser);
                NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
            };
            _configuration = configuration;
        }


        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var ossToken = await _localStorage.GetItemAsync<string>(authToken);
            if (string.IsNullOrWhiteSpace(ossToken))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            var data = await _ossIdService.IdentifyVerify(ossToken);
            if (data.errcode == -1)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            List<Claim> claims = new List<Claim>();
            foreach (var item in data.data)
            {
                if (item.type == ClaimTypes.Role)
                {
                    item.value = item.value.ToUpper();
                }
                claims.Add(new Claim(item.type, item.value));
            }
            var identity = new ClaimsIdentity(claims, "paraAuthentication"); // 使用适当的认证类型
            var user = new ClaimsPrincipal(identity);
            // 更新认证状态
            _customAuthenticationService.CurrentUser = user;
            return new AuthenticationState(user);
        }

        public async Task<bool> AuthOssTokenAsync(string? ossToken = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ossToken))
                {
                    ossToken = await _localStorage.GetItemAsync<string>(authToken);
                    if (string.IsNullOrWhiteSpace(ossToken))
                        return false;
                }

                var data = await _ossIdService.IdentifyVerify(ossToken);
                if (data.errcode == -1)
                {
                    return false;
                }
                List<Claim> claims = new List<Claim>();
                foreach (var item in data.data)
                {
                    if (item.type == ClaimTypes.Role)
                    {
                        item.value = item.value.ToUpper();
                    }
                    claims.Add(new Claim(item.type, item.value));
                }
                var identity = new ClaimsIdentity(claims, "paraAuthentication");
                var user = new ClaimsPrincipal(identity);
                // 更新认证状态
                _customAuthenticationService.CurrentUser = user;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AuthenticationState> AuthOssTokenReAuthAsync(string? ossToken = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ossToken))
                {
                    ossToken = await _localStorage.GetItemAsync<string>(authToken);
                    if (string.IsNullOrWhiteSpace(ossToken))
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                var data = await _ossIdService.IdentifyVerify(ossToken);
                if (data.errcode == -1)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                List<Claim> claims = new List<Claim>();
                foreach (var item in data.data)
                {
                    if (item.type == ClaimTypes.Role)
                    {
                        item.value = item.value.ToUpper();
                    }
                    claims.Add(new Claim(item.type, item.value));
                }
                var identity = new ClaimsIdentity(claims, "paraAuthentication"); // 使用适当的认证类型
                var user = new ClaimsPrincipal(identity);
                // 更新认证状态
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task SignOutAsync()
        {
            var ossToken = await _localStorage.GetItemAsync<string>(authToken);
            if (!string.IsNullOrWhiteSpace(ossToken))
            {
                await _localStorage.RemoveItemAsync(authToken);
            }
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        }

    }
}
