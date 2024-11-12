using DevExtreme.AspNet.Data;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Data.Services.DevExpressExtensions;

namespace AIChatBase.Data.Services.QYWX
{
    public partial interface IQYWXService
    {
        Task<HttpResponseMessage> PushMessageUseBot(string message, string bot, string userid = null);
        Task<HttpResponseMessage> PushMessageUseBotMarkdown(string message, string bot, string userid = null);
        Task<string> UploadFile(byte[] fileBytes, string fileName, string bot);
        Task<HttpResponseMessage> PushFileUseBot(string mediaId, string bot, string userid = null);
    }
}
