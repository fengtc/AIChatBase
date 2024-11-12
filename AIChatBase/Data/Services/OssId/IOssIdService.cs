using AIChatBase.Data.Models.Customs;

namespace AIChatBase.Data.Services.OssId
{
    public interface IOssIdService
    {
        Task<string?> GetOssIdToken();
        Task<BaseRespDto<List<IdentifyVerify>>> IdentifyVerify(string token);
        Task<IdentifyToken> Login(string userid, string servicename, string ip, string agent);
        Task<BaseRespDto<IdentifyToken>> LoginOut(string token, string username);
        Task<IdentifyToken> NewLogin(string appid, string code, string servicename, string ip, string agent);
    }
}
