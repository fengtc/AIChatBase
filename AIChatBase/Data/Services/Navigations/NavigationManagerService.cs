using Microsoft.AspNetCore.Components;
using System.Web;
using AIChatBase.Data.Models.Customs;

namespace AIChatBase.Data.Services.Navigations;

public class NavigationManagerService : INavigationManagerService
{
    private readonly NavigationManager _navigationManager;
    public NavigationManagerService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// 获取企业微信Code
    /// </summary>
    /// <returns></returns>
    public string? GetWxUserCode()
    {
        // 获取Uri对象
        var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);

        // HttpUtility 获取参数值
        return HttpUtility.ParseQueryString(uri.Query).Get("code");
    }

    /// <summary>
    /// 获取地址（不包括BaseUrl）
    /// </summary>
    /// <returns></returns>
    public string? GetLocation()
    {
        var location = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);

        return location;
    }
    /// <summary>
    /// 用户端类型
    /// </summary>
    /// <param name="agent"></param>
    /// <returns></returns>
    public UserAgentEnum GetEnvjudge(string? agent)
    {
        if (agent == null)
        {
            return UserAgentEnum.OTHER;
        }
        bool isMobile = agent.Contains("Mobile", StringComparison.OrdinalIgnoreCase);
        bool isWx = agent.Contains("MicroMessenger", StringComparison.OrdinalIgnoreCase);
        bool isComWx = agent.Contains("wxwork", StringComparison.OrdinalIgnoreCase);
        if (isComWx && isMobile)
        {
            return UserAgentEnum.COM_WX_MOBILE;
        }
        else if (isComWx && !isMobile)
        {
            return UserAgentEnum.COM_WX_PC;
        }
        else if (isWx && isMobile)
        {
            return UserAgentEnum.WX_MOBILE;
        }
        else if (isWx && !isMobile)
        {
            return UserAgentEnum.WX_PC;
        }
        else
        {
            return UserAgentEnum.OTHER;
        }
    }

    /// <summary>
    /// 获取url传入OssToken
    /// </summary>
    /// <returns></returns>
    public string? GetUriOssToken()
    {
        // 获取Uri对象
        var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);

        // HttpUtility 获取参数值
        return HttpUtility.ParseQueryString(uri.Query).Get("OssToken");
    }

    /// <summary>
    /// 获取url传入show
    /// </summary>
    /// <returns></returns>
    public bool GetUriShow()
    {
        // 获取Uri对象
        var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);

        // HttpUtility 获取参数值
        var show = HttpUtility.ParseQueryString(uri.Query).Get("show");
        if (string.IsNullOrEmpty(show))
        {
            return false;
        }
        return show.ToUpper() == "TRUE" ? true : false;
    }

    /// <summary>
    /// 获取url传入ServiceName
    /// </summary>
    /// <returns></returns>
    public string? GetUriServiceName()
    {
        // 获取Uri对象
        var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);

        // HttpUtility 获取参数值
        return HttpUtility.ParseQueryString(uri.Query).Get("ServiceName");
    }
}
