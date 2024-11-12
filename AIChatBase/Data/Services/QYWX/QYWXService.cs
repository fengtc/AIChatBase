using DevExtreme.AspNet.Data;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AIChatBase.Authentication;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Data.Services.DevExpressExtensions;
using ParateraNetUtil.Utils.Https;
using ParateraNetUtil.Utils.Values;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace AIChatBase.Data.Services.QYWX
{
    public partial class QYWXService : BaseClient, IQYWXService
    {

        private readonly ILogger<QYWXService> _logger;

        private readonly IConfiguration _configuration;
        public QYWXService(ILogger<QYWXService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : base(logger, nameof(QYWXService), httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 机器人通知结果
        /// </summary>
        /// <param name="message"></param>
        /// <param name="bot"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PushMessageUseBot(string message, string bot, string userid = null)
        {
            var client = CreateHttpClient();
            await ConfigClient(client);
        // 发送请求
            var response = await client.PostAsJsonAsync($"/cgi-bin/webhook/send?key={bot}", new { msgtype = "text", text = new { content = message} });
            _logger.LogInformation($"机器人通知结果({message}-{bot})：{response?.Content}");
            return response;
        }

        /// <summary>
        /// 机器人Markdown通知
        /// </summary>
        /// <param name="message"></param>
        /// <param name="bot"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PushMessageUseBotMarkdown(string message, string bot, string userid = null)
        {
            var client = CreateHttpClient();
            await ConfigClient(client);
            // 发送请求
            var response = await client.PostAsJsonAsync($"/cgi-bin/webhook/send?key={bot}", new { msgtype = "markdown", markdown = new { content = message } });
            _logger.LogInformation($"机器人通知结果({message}-{bot})：{response?.Content}");
            return response;
        }

        public async Task<string> UploadFile(byte[] fileBytes, string fileName, string bot)
        {
            using (var client = new HttpClient())
            {                
                using (var form = new MultipartFormDataContent())
                {
                    var boundary = "--------" + DateTime.Now.Ticks.ToString("X");
                    form.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    form.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
                    form.Add(new StreamContent(BuildMultipartStream("media", fileName, fileBytes, boundary)));
                    var uploadUrl = $"https://qyapi.weixin.qq.com/cgi-bin/webhook/upload_media?key={bot}&type=file&debug=1";
                    var response = await client.PostAsync(uploadUrl, form);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseString);

                    return jsonResponse["media_id"]?.ToString();
                }
            }
        }

       
        private static Stream BuildMultipartStream(string name, string fileName, byte[] fileBytes, string boundary)
        {
            byte[] firstBytes = Encoding.UTF8.GetBytes(String.Format(
                "--{0}\r\n" +
                "Content-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\n" +
                "Content-Type: application/octet-stream\r\n" +
                "Content-Transfer-Encoding: binary\r\n\r\n",
                boundary,
                name,
                fileName));

            byte[] lastBytes = Encoding.UTF8.GetBytes(String.Format(
                "\r\n" +
                "--{0}--\r\n",
                boundary));

            int contentLength = firstBytes.Length + fileBytes.Length + lastBytes.Length;
            byte[] contentBytes = new byte[contentLength];

            Array.Copy(firstBytes, 0, contentBytes, 0, firstBytes.Length);
            Array.Copy(fileBytes, 0, contentBytes, firstBytes.Length, fileBytes.Length);
            Array.Copy(lastBytes, 0, contentBytes, firstBytes.Length + fileBytes.Length, lastBytes.Length);
            return new MemoryStream(contentBytes);
        }

        /// <summary>
        /// 机器人文件通知
        /// </summary>
        /// <param name="message"></param>
        /// <param name="bot"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PushFileUseBot(string mediaId, string bot, string userid = null)
        {
            var client = CreateHttpClient();
            await ConfigClient(client);
            var payload = new
            {
                msgtype = "file",
                file = new
                {
                    media_id = mediaId
                }
            };
            // 发送请求
            var response = await client.PostAsJsonAsync($"/cgi-bin/webhook/send?key={bot}", payload);
            _logger.LogInformation($"机器人通知结果({mediaId}-{bot})：{response?.Content}");
            return response;
        }
    }
}
