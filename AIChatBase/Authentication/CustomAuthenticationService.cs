using System.Security.Claims;


namespace AIChatBase.Authentication
{

    public class CustomAuthenticationService
    {
        public event Action<ClaimsPrincipal>? UserChanged;
        private ClaimsPrincipal? currentUser;

        public ClaimsPrincipal CurrentUser
        {
            get { return currentUser ?? new(); }
            set
            {
                currentUser = value;

                if (UserChanged is not null)
                {
                    UserChanged(currentUser);
                }
            }
        }

        public string GetUserId()
        {
            return currentUser?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetOssUserId()
        {
            return currentUser?.FindFirst("oss_userId")?.Value;
        }

        public bool IsIncludeRole(string roles)
        {
            var roleLists = roles.Contains(",") ? new List<string>(roles.Split(",")) : new List<string>() { roles };
            if (currentUser == null)
            {
                return false;
            }
            return roleLists
                .Count(c => currentUser.IsInRole(c)) > 0;
        }

        public string GetEmail()
        {
            return currentUser?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public string GetUserName()
        {
            return currentUser?.FindFirst(ClaimTypes.Name)?.Value;
        }

        public string GetMemberId()
        {
            return currentUser?.FindFirst("memberid")?.Value;
        }


        /// <summary>
        /// 获取Claim对应值
        /// </summary>
        /// <returns></returns>
        public string? GetClaimValue(string ClaimName)
        {
            if (currentUser != null)
                return currentUser.FindFirstValue(ClaimName);

            return string.Empty;
        }
    }
}
