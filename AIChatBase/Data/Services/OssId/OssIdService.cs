using Newtonsoft.Json;
using ParateraNetUtil.Utils.Https;
using ParateraNetUtil.Utils.Values;
using System.Net.Http.Headers;
using AIChatBase.Data.Services.Memory;
using AIChatBase.Data.Models.Customs;

namespace AIChatBase.Data.Services.OssId
{
    public class OssIdService : BaseClient, IOssIdService
    {
        private readonly IConfiguration _configuration;
        /// <summary>
        /// 内存
        /// </summary>
        protected readonly IBxMemoryService _bxMemoryService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public OssIdService(ILogger<OssIdService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, IBxMemoryService bxMemoryService)
            : base(logger, nameof(OssIdService), httpClientFactory)
        {
            _configuration = configuration;
            _bxMemoryService = bxMemoryService;
        }

        /// <summary>
        /// 配置客户端
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected override async Task ConfigClient(HttpClient client)
        {
            var ossIdToken = await GetOssIdToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ossIdToken);
        }

        /// <summary>
        /// 获取OssData访问Token
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">请求失败后，抛出异常</exception>
        public async Task<string?> GetOssIdToken()
        {
            return await _bxMemoryService.GetOrCreateAsync(nameof(OssIdService) + nameof(GetOssIdToken), async () =>
            {
                using var client = CreateHttpClient();
                var url = "/api/OssData/login";
                LogRequestInformation("POST", url);
                HttpResponseMessage? httpResponseMessage = await client.PostAsJsonAsync(url, new
                {
                    email = _configuration["OssIdEmail"],
                    password = _configuration["OssIdPassword"]
                });
                LogRequestInformation("POST", url);

                if (httpResponseMessage == null)
                {
                    throw new Exception("Request Type Not Support");
                }

                httpResponseMessage.EnsureSuccessStatusCode();

                var str = await httpResponseMessage.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<BaseRespDto<IdentifyToken>>(str);

                return result?.data?.token;
            }, TimeSpan.FromHours(1));
        }


        /// <summary>
        /// OssId登录接口
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="servicename">服务名称</param>
        /// <param name="ip">用户Ip</param>
        /// <param name="agent">用户设备</param>
        /// <returns></returns>
        public async Task<IdentifyToken> Login(string userid, string servicename, string ip, string agent)
        {
            var requestStr = $"/api/OssUI/login?userid={userid}&userIp={ip}&userAgent={agent}&loginService=1000304&projcet_id=1000304";
            var result = await Get<BaseRespDto<IdentifyToken>>(requestStr);
            return result.data;
        }

        /// <summary>
        /// 新-登录接口(同时获取用户信息)
        /// </summary>
        public async Task<IdentifyToken> NewLogin(string appid, string code, string servicename, string ip, string agent)
        {

            var requestStr = $"/api/OssUI/newlogin?appid={appid}&code={code}&userIp={ip}&userAgent={agent}&loginService={servicename}";
            var result = await Get<BaseRespDto<IdentifyToken>>(requestStr);
            return result.data;
        }

        public async Task<BaseRespDto<IdentifyToken>> LoginOut(string token, string username)
        {
            var requestStr = $"/api/OssUI/logout";
            var result = await Post<BaseRespDto<IdentifyToken>>(requestStr, new { token });
            if (result?.errcode == -1)
            {
                _logger.LogError($"用户({username})登出失败，错误信息:{result?.errmsg}");
            }
            return result;
        }



        /// <summary>
        /// 获取身份验证信息
        /// </summary>
        /// <returns></returns>
        public async Task<BaseRespDto<List<IdentifyVerify>>> IdentifyVerify(string token)
        {
            var requestStr = $"/api/OssUI/verify";
            var result = await Post<BaseRespDto<List<IdentifyVerify>>>(requestStr, new { token });
            return result;
        }
    }
}
