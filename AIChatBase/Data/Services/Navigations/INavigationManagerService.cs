using AIChatBase.Data.Models.Customs;

namespace AIChatBase.Data.Services.Navigations
{
    public interface INavigationManagerService
    {
        string? GetLocation();
        string? GetWxUserCode();
        UserAgentEnum GetEnvjudge(string? agent);
        string? GetUriOssToken();
        bool GetUriShow();
    }
}