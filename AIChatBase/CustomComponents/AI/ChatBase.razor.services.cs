using AIChatBase.Authentication;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Data.Services.DevExpressExtensions;
using AIChatBase.Data.Services.Memory;
using AIChatBase.Data.Services.Navigations;
using AIChatBase.Data.Services.OssData;
using AIChatBase.Data.Services.OssId;
using Blazored.LocalStorage;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParateraNetUtil.Utils.Https;
using ParateraNetUtil.Utils.Values;
using Polly;
using Polly.Extensions.Http;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using static AIChatBase.CustomComponents.AI.ChatBase;

namespace AIChatBase.CustomComponents.AI
{
    public partial class ChatBase
    {
        #region openAI请求
        public class ChatClient : BaseClient
        {
            public delegate void OnWaitEventHandler(object sender, CallBackModel data);
            public delegate void OnStartEventHandler(object sender, CallBackModel data);
            public delegate void OnMessageEventHandler(object sender, CallBackModel data);
            public delegate void OnErrorEventHandler(object sender, Exception e, CallBackModel data);
            public delegate void OnFinishEventHandler(object sender, CallBackModel data);

            public event OnWaitEventHandler? OnWaitEvent;
            public event OnStartEventHandler? OnStartEvent;
            public event OnMessageEventHandler? OnMessage;
            public event OnErrorEventHandler? OnErrorEvent;
            public event OnFinishEventHandler? OnFinishEvent;


            /// <summary>
            /// 
            /// </summary>
            /// <param name="logger"></param>
            /// <param name="httpClientFactory"></param>
            public ChatClient(ILogger<ChatClient> logger, IHttpClientFactory httpClientFactory) :
                base(logger, nameof(ChatClient), httpClientFactory)
            {

            }

            /// <summary>
            /// 设置请求接口
            /// </summary>
            public string? ChatApi { get; set; }

            /// <summary>
            /// 设置是否流式输出
            /// </summary>
            public bool Stream { get; set; }

            /// <summary>
            /// 问题内容的最大长度
            /// </summary>
            public int MaxTextLength { get; set; } = 2048;

            /// <summary>
            /// 验证设置
            /// </summary>
            public AuthenticationHeaderValue? Authentication { get; set; }

            /// <summary>
            /// 获取回复
            /// </summary>
            /// <returns></returns>
            public async Task GetAnswer(OpenAIRequest data, CallBackModel callBackData)
            {
                OnWaitEvent?.Invoke(this, callBackData);
                //var totalContent = 0;
                //if (data.Messages.Any())
                //{
                //    data.Messages.ForEach(n => totalContent += n.Content.Length);            
                //}
                //if (totalContent > MaxTextLength)
                //{
                //    OnErrorEvent?.Invoke(this, new Exception("内容过多"), callBackData);
                //    OnFinishEvent?.Invoke(this, callBackData);
                //    return;
                //}

                //重置为可以输出
                ResumeStreaming();

                if (Stream)
                {
                    GetStreamAnswer(data, callBackData);
                    return;
                }
                await GetStringAnswer(data, callBackData);
            }

            /// <summary>
            /// 获取非流式回复
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            private async Task GetStringAnswer(OpenAIRequest data, CallBackModel callBackData)
            {
                string? result;
                try
                {
                    using var client = CreateHttpClient();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.Timeout = TimeSpan.FromMinutes(5);

                    if (Authentication != null)
                    {
                        client.DefaultRequestHeaders.Authorization = Authentication;
                    }

                    OnStartEvent?.Invoke(this, callBackData);
                    var response = await client.PostAsync(ChatApi, new StringContent(System.Text.Json.JsonSerializer.Serialize(data), Encoding.UTF8, MediaTypeNames.Application.Json));

                    result = await response.Content.ReadAsStringAsync();


                    _logger.LogInformation($"SessionId={callBackData.SessionId}  提问：{data.messages[0].content} | {ChatApi} 返回：{(int)response.StatusCode} | {result}");


                    if (string.IsNullOrEmpty(result))
                    {
                        OnErrorEvent?.Invoke(this, new Exception("未返回结果"), callBackData);
                        return;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var answer = "";
                        var promptResult = JsonConvert.DeserializeObject<OpenAIOKResponse>(result);
                        if (promptResult != null && promptResult.Choices.Any())
                        {
                            promptResult.Choices.ForEach(n =>
                            {
                                answer += n.Message.content;
                            });
                        }
                        else
                        {
                            answer = "请求失败，请重试[2001]";
                        }

                        callBackData.Message = answer;
                        OnMessage?.Invoke(this, callBackData);
                    }
                    else
                    {
                        var answer = "请求失败，请重试[2002]";
                        var promptResult = JsonConvert.DeserializeObject<OpenAIErrorResponse>(result);
                        if (promptResult != null && promptResult.Error != null)
                        {
                            answer = promptResult.Error.Message;
                        }

                        OnErrorEvent?.Invoke(this, new Exception(answer), callBackData);
                    }
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError(e, $"SessionId={callBackData.SessionId}  提问：{data.messages[0].content} | {ChatApi} [2003]报错：{e.Message}");

                    try
                    {
                        OnErrorEvent?.Invoke(this, new Exception("请求失败，请重试[2003]"), callBackData);
                    }
                    catch (Exception) { }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"SessionId={callBackData.SessionId}  提问：{data.messages[0].content} | {ChatApi} [2004]报错：{e.Message}");

                    try
                    {
                        if (e.Message.Contains("401") || e.Message.Contains("403")) //无权限
                        {
                            OnErrorEvent.Invoke(this, new Exception("暂未开通AI助手功能"), callBackData);
                        }
                        else
                        {
                            OnErrorEvent.Invoke(this, new Exception("请求失败，请重试[2004]"), callBackData);
                        }
                    }
                    catch (Exception) { }
                }
                finally
                {
                    OnFinishEvent?.Invoke(this, callBackData);
                }
            }

            /// <summary>
            /// 获取流式回复
            /// </summary>
            /// <returns></returns>
            private void GetStreamAnswer(OpenAIRequest data, CallBackModel callBackData)
            {
                var client = CreateHttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(data), Encoding.UTF8, MediaTypeNames.Application.Json);

                var eventSource = new EventSource(ChatApi, client, HttpMethod.Post, Authentication, content);
                if (OnStartEvent != null)
                {
                    eventSource.OnOpenEvent += delegate (object sender)
                    {
                        OnStartEvent.Invoke(this, callBackData);
                    };
                }
                eventSource.OnEvent += delegate (object sender, OnEventArgs e)
                {
                    //_logger.LogInformation($"SessionId={callBackData.SessionId}  提问：{data.Messages[0].Content} | {ChatApi} 返回：{e.Message}");

                    if (shouldPause)
                    {
                        // 如果暂停，不处理事件
                        // _logger.LogInformation("暂停流式输出");

                        eventSource.Close();
                        return;
                    }

                    //结束
                    if (e.Message.Equals("[DONE]"))
                    {
                        return;
                    }

                    var answer = "";
                    var promptResult = JsonConvert.DeserializeObject<OpenAIOKResponse>(e.Message);

                    if (promptResult != null && promptResult.Choices.Any())
                    {
                        promptResult.Choices.ForEach(n =>
                        {
                            answer += n.Delta.content;
                        });

                        callBackData.ContentReferences = promptResult.References == null ? "" : JsonConvert.SerializeObject(promptResult.References);
                    }
                    else
                    {
                        answer = "请求失败，请重试[2001]";
                    }

                    callBackData.Message = answer;
                    OnMessage?.Invoke(this, callBackData);
                };
                if (OnErrorEvent != null)
                {
                    eventSource.OnErrorEvent += delegate (object sender, Exception e)
                    {
                        _logger.LogError(e, $"SessionId={callBackData.SessionId}  提问：{data.messages[0].content} | {ChatApi} 报错：{e.Message}");
                        if (e.Message.Contains("401") || e.Message.Contains("403"))
                        {
                            OnErrorEvent.Invoke(this, new Exception("无权限"), callBackData);
                        }
                        else
                        {
                            OnErrorEvent.Invoke(this, new Exception("请求失败，请重试[2004]"), callBackData);
                        }
                    };
                }
                if (OnFinishEvent != null)
                {
                    eventSource.OnCloseEvent += delegate (object sender)
                    {
                        OnFinishEvent?.Invoke(this, callBackData);
                    };
                }
                eventSource?.Open();
            }

            private bool shouldPause = false;

            // 在需要的时候调用该方法来暂停或者恢复流式输出
            public void PauseStreaming()
            {
                shouldPause = true;
            }

            public void ResumeStreaming()
            {
                shouldPause = false;
            }
        }
        #endregion

        #region OssAgent服务
        public interface IOssAgentService
        {
            Task<CustomGridDevExtremeDataSource<T>> GetTEntityDataSourceV2<T>();

            Task<DevResponseDto<T>> GetTEntityListV2<T>(DataSourceLoadOptionsBase options);

            Task<DevResponseDto<T>> PostGetTEntityV2<T>(DataSourceLoadOptionsBase options);
            Task<List<T>> GetTEntityDataListV2<T>(DataSourceLoadOptionsBase options);

            Task<T> PostTEntityV2<T>(T model);

            Task<T> PutTEntityV2<T>(string key, T model);

            Task DeleteTEntityV2<T>(string key);

            Task<Dictionary<string, string>?> GetQuestionGeneration(string vecterName, int topN = 5);
        }
        public class OssAgentService : BaseClient, IOssAgentService
        {

            private readonly ILogger<OssAgentService> _logger;

            private readonly IConfiguration _configuration;

            private readonly ILocalStorageService _localStorage;
            public OssAgentService(ILogger<OssAgentService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILocalStorageService localStorageService)
        : base(logger, nameof(OssAgentService), httpClientFactory)
            {
                _logger = logger;
                _configuration = configuration;
                _localStorage = localStorageService;
            }

            protected override async Task ConfigClient(HttpClient client)
            {
                var userToken = Convert.ToBoolean(_configuration["OssAgentUseUserToken"]);
                var token = _configuration["OssAgentServiceToken"];
                if (userToken)
                {
                    token = await _localStorage.GetItemAsync<string>("oss_token");
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            /// <summary>
            /// 获取DataSouece
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="url"></param>
            /// <returns></returns>
            [DisplayName("获取数据源")]
            protected async Task<CustomGridDevExtremeDataSource<T>> GetCustomGridDevExtremeDataSource<T>(string url)
            {
                var client = CreateHttpClient();
                await ConfigClient(client);
                url = await SetBaseQueryParameter(url);
                return new CustomGridDevExtremeDataSource<T>(client, new Uri(client.BaseAddress, url));
            }

            public async Task<Dictionary<string, string>?> GetQuestionGeneration(string vecterName, int topN = 5)
            {
                var url = $"/api/QuestionGeneration/generate-questions?tableName=km-{vecterName}&numberOfTexts={topN}";
                _logger.LogInformation($"请求推荐问题接口：{url}");

                var result = await Get<Dictionary<string, string>>(url);

                _logger.LogInformation($"请求推荐问题接口：{url} 返回结果：{JsonConvert.SerializeObject(result)}");

                return result;
            }

            #region V2接口
            /// <summary>
            /// 获取dataSource
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="TEntityApiName"></param>
            /// <returns></returns>
            [DisplayName("获取V2数据源")]
            public async Task<CustomGridDevExtremeDataSource<T>> GetTEntityDataSourceV2<T>()
            {
                Type type = typeof(T);
                try
                {
                    var dataSource = await GetCustomGridDevExtremeDataSource<T>($"/api/v2/List/pgContext/{type?.Name}");
                    return dataSource;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    return null;
                }
            }

            /// <summary>
            /// 获取列表
            /// </summary>
            /// <param name="options"></param>
            /// <returns></returns>
            public async Task<DevResponseDto<T>> GetTEntityListV2<T>(DataSourceLoadOptionsBase options)
            {
                try
                {
                    Type type = typeof(T);
                    var str = StringUtil.ModelToUriParam(options, $"/api/v2/List/pgContext/{type?.Name}");
                    return await Get<DevResponseDto<T>>(str);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    return null;
                }
            }

            /// <summary>
            /// 获取列表List
            /// </summary>
            /// <param name="options"></param>
            /// <returns></returns>
            public async Task<List<T>> GetTEntityDataListV2<T>(DataSourceLoadOptionsBase options)
            {
                try
                {
                    Type type = typeof(T);
                    var str = StringUtil.ModelToUriParam(options, $"/api/v2/List/pgContext/{type?.Name}");
                    var result = await Get<DevResponseDto<T>>(str);
                    if (result != null && result.data != null)
                    {
                        return result.data;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    return null;
                }
            }

            public async Task<DevResponseDto<T>> PostGetTEntityV2<T>(DataSourceLoadOptionsBase options)
            {
                Type type = typeof(T);
                var client = CreateHttpClient();
                await ConfigClient(client);
                using (var content = new MultipartFormDataContent())
                {
                    // 添加文本字段
                    content.Add(new StringContent(JsonConvert.SerializeObject(options)), "loadOptionsStr");

                    // 发送请求
                    var response = await client.PostAsync($"/api/v2/List/pgContext/{type?.Name}", content);

                    // 确保HTTP响应状态为成功
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<DevResponseDto<T>>(responseString);
                    return result;
                }
            }

            /// <summary>
            /// 添加记录
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            public async Task<T> PostTEntityV2<T>(T model)
            {
                Type type = typeof(T);
                var a = JsonConvert.SerializeObject(model);
                return await Post<T>($"/api/v2/Create/pgContext/{type?.Name}", model);
            }

            /// <summary>
            /// 修改记录
            /// </summary>
            /// <param name="model"></param>
            /// <returns></returns>
            public async Task<T> PutTEntityV2<T>(string key, T model)
            {
                // 获取泛型参数的类型
                Type type = typeof(T);
                return await Put<T>($"/api/v2/Update/pgContext/{type?.Name}/{key}", model);
            }

            /// <summary>
            /// 删除记录
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public async Task DeleteTEntityV2<T>(string key)
            {
                Type type = typeof(T);
                await Delete($"/api/v2/Delete/pgContext/{type?.Name}/{key}");
            }
            #endregion
        }
        #endregion
    }

    public static class ServicesExtensions
    {
        public static void AddAIServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddHttpClient(nameof(ChatClient)).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return httpClientHandler;
            }).AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(3)));

            services.AddHttpClient(nameof(OssAgentService), options =>
            {
                options.BaseAddress = new Uri(configuration["OssAgentServiceApi"]);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return httpClientHandler;
            }).AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(3)));

            services.AddTransient<IOssAgentService, OssAgentService>();
            services.AddTransient<ChatClient>(); //openai请求对象
        }
    }
}
