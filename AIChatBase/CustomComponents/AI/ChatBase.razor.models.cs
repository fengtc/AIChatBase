using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text;

namespace AIChatBase.CustomComponents.AI
{
    public partial class ChatBase
    {

        public class ChatMessage
        {
            /// <summary>
            /// 提问用户名
            /// </summary>
            public string? Sender { get; set; }

            /// <summary>
            /// 提问内容
            /// </summary>
            public string? Content { get; set; }

            /// <summary>
            /// 类型 机器人，用户
            /// </summary>
            public string? UserType { get; set; }

            /// <summary>
            /// 提问时间
            /// </summary>
            public DateTime Time { get; set; } = DateTime.Now;

            /// <summary>
            /// 回复内容
            /// </summary>
            public string? ReplyContent { get; set; }

            /// <summary>
            /// 回复时间
            /// </summary>
            public DateTime? ReplyTime { get; set; } = DateTime.Now;

            /// <summary>
            /// 回复用户名
            /// </summary>
            public string? ReplySender { get; set; }

            /// <summary>
            /// 分组编号
            /// </summary>
            public string GroupId { get; set; }

            /// <summary>
            /// 会话ID
            /// </summary>
            public string? SessionId { get; set; }

            /// <summary>
            /// 答复耗时(秒)
            /// </summary>
            public double? asnwer_times { get; set; } = 0;

            /// <summary>
            /// 是否推荐
            /// </summary>
            public string? recommend { get; set; }

            /// <summary>
            /// 引用
            /// </summary>
            public string? content_references { get; set; }

            /// <summary>
            /// 已完成输出
            /// </summary>
            public bool IsFinish { get; set; } = false;


            /// <summary>
            /// 任务类型名称
            /// </summary>
            public string? ChatTaskName { get; set; }
        }

        #region AI提问请求
        public class OpenAIRequest
        {
            ///// <summary>
            ///// -2.0 和 2.0 之间的数字。正值会根据新标记在文本中的现有频率对其进行惩罚，从而降低模型逐字重复同一行的可能性。
            ///// [查看有关频率和存在惩罚的更多信息。](https://platform.openai.com/docs/api-reference/parameter-details)
            ///// </summary>
            //[JsonProperty("frequency_penalty", NullValueHandling = NullValueHandling.Ignore)]
            //public double? FrequencyPenalty { get; set; }

            ///// <summary>
            ///// 修改指定标记出现在完成中的可能性。  接受一个 json 对象，该对象将标记（由标记器中的标记 ID 指定）映射到从 -100 到 100
            ///// 的关联偏差值。从数学上讲，偏差会在采样之前添加到模型生成的 logits 中。确切的效果因模型而异，但 -1 和 1 之间的值应该会减少或增加选择的可能性；像 -100 或
            ///// 100 这样的值应该导致相关令牌的禁止或独占选择。
            ///// </summary>
            //[JsonProperty("logit_bias")]
            //public object LogitBias { get; set; }

            /// <summary>
                    /// 聊天完成时生成的最大令牌数。  输入标记和生成标记的总长度受模型上下文长度的限制。
                    /// </summary>
            [JsonPropertyName("max_tokens")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public int? max_tokens { get; set; }

            /// <summary>
                    /// 以[聊天格式](https://platform.openai.com/docs/guides/chat/introduction)生成聊天完成的消息。
                    /// </summary>
            [JsonPropertyName("messages")]
            public List<Message> messages { get; set; }

            /// <summary>
                    /// 要使用的模型的 ID。有关哪些模型适用于聊天 API
                    /// 的详细信息，请参阅[模型端点兼容性表。](https://platform.openai.com/docs/models/model-endpoint-compatibility)
                    /// </summary>
            [JsonPropertyName("model")]
            public string model { get; set; }

            ///// <summary>
            ///// 为每个输入消息生成多少个聊天完成选项。
            ///// </summary>
            //[JsonProperty("n", NullValueHandling = NullValueHandling.Ignore)]
            //public long? N { get; set; }

            ///// <summary>
            ///// -2.0 和 2.0 之间的数字。正值会根据到目前为止是否出现在文本中来惩罚新标记，从而增加模型谈论新主题的可能性。
            ///// [查看有关频率和存在惩罚的更多信息。](https://platform.openai.com/docs/api-reference/parameter-details)
            ///// </summary>
            //[JsonProperty("presence_penalty", NullValueHandling = NullValueHandling.Ignore)]
            //public double? PresencePenalty { get; set; }

            ///// <summary>
            ///// API 将停止生成更多令牌的最多 4 个序列。
            ///// </summary>
            //[JsonProperty("stop", NullValueHandling = NullValueHandling.Ignore)]
            //public string Stop { get; set; }

            /// <summary>
                    /// 如果设置，将发送部分消息增量，就像在 ChatGPT
                    /// 中一样。当令牌可用时，令牌将作为纯数据[服务器发送事件](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#Event_stream_format)`data:
                    /// [DONE]`发送，流由消息终止。[有关示例代码](https://github.com/openai/openai-cookbook/blob/main/examples/How_to_stream_completions.ipynb)，请参阅
                    /// OpenAI Cookbook 。
                    /// </summary>      
            [JsonPropertyName("stream")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public bool? stream { get; set; }

            /// <summary>
                    /// 使用什么采样温度，介于 0 和 2 之间。较高的值（如 0.8）将使输出更加随机，而较低的值（如 0.2）将使输出更加集中和确定。
                    /// 我们通常建议改变这个或`top_p`但不是两者。
                    /// </summary>
            [JsonPropertyName("temperature")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public decimal? temperature { get; set; }

            /// <summary>
                    /// 一种替代温度采样的方法，称为核采样，其中模型考虑具有 top_p 概率质量的标记的结果。所以 0.1 意味着只考虑构成前 10% 概率质量的标记。
                    /// 我们通常建议改变这个或`temperature`但不是两者。
                    /// </summary>
            [JsonPropertyName("top_p")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public decimal? top_p { get; set; }

            ///// <summary>
            ///// 代表您的最终用户的唯一标识符，可以帮助 OpenAI
            ///// 监控和检测滥用行为。[了解更多](https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids)。
            ///// </summary>
            //[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
            //public string User { get; set; }     


            #region 自定义参数

            /// <summary>
            /// 联网搜索
            /// </summary>
            public bool? online { get; set; } = false;

            #endregion
        }

        public class Message
        {
            [JsonPropertyName("content")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? content { get; set; }

            [JsonPropertyName("role")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? role { get; set; }
        }
        #endregion

        /// <summary>
        /// Chat模式配置
        /// </summary>
        public class ChatModelConfig
        {
            /// <summary>
            /// 产品名称
            /// </summary>
            public string? ProductName { get; set; }

            /// <summary>
            /// 接口地址
            /// </summary>
            public string? ChatApi { get; set; }

            /// <summary>
            /// 模式名称
            /// </summary>
            public string? ModeName { get; set; }

            /// <summary>
            /// 请求凭证
            /// </summary>
            public string? Token { get; set; }

            /// <summary>
            /// 默认使用
            /// </summary>
            public bool IsDefault { get; set; } = false;

            /// <summary>
            /// 是流式输出
            /// </summary>
            public bool IsStream { get; set; } = false;

            /// <summary>
            /// 开启
            /// </summary>
            public bool IsEnable { get; set; } = false;

            /// <summary>
            /// 模型名称(大模型，本地库  )
            /// </summary>
            public string? ModelType { get; set; }

            /// <summary>
            /// 最大tokens数量
            /// </summary>
            public int MaxTokens { get; set; }
            /// <summary>
            /// 设置多轮对话数（0 为单轮）
            /// </summary>
            public int MutilAnswerNum { get; set; }

            /// <summary>
            /// 启用联网功能
            /// </summary>
            public bool Online { get; set; }
        }

        /// <summary>
        /// AIChat自定义的回调模型
        /// </summary>
        public class CallBackModel
        {
            public string? SessionId { get; set; }
            public string? GroupId { get; set; }
            public string? Message { get; set; }
            public string? ContentReferences { get; set; }
        }

        public class BaseModel
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Newtonsoft.Json.JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
            [Display(Name = "ID")]
            public long ID { get; set; }
            /// <summary>
            /// 状态   删除标记： 1删除    0或者null正常
            /// </summary>
            [Display(Name = "状态")]
            public int? state { get; set; }
            /// <summary>
            /// 发起人id
            /// </summary>
            public string start_member_id { get; set; }
            /// <summary>
            /// 发起人姓名
            /// </summary>
            public string start_member_name { get; set; }
            /// <summary>
            /// 发起时间
            /// </summary>
            [Display(Name = "发起时间")]
            public DateTime? start_date { get; set; }
            /// <summary>
            /// 审批人ID
            /// </summary>
            public string approve_member_id { get; set; }
            /// <summary>
            /// 审批人姓名
            /// </summary>
            public string approve_member_name { get; set; }
            public DateTime? approve_date { get; set; }
            /// <summary>
            /// 完成标识（表示整个数据不会再做变更）
            /// </summary>
            [Display(Name = "完成标识")]
            public int? finishedflag { get; set; } = 0;
            /// <summary>
            /// 批准标识
            /// </summary>
            public int? ratifyflag { get; set; }
            /// <summary>
            /// 批准人ID
            /// </summary>
            public string ratify_member_id { get; set; }
            /// <summary>
            /// 批准人姓名
            /// </summary>
            public string ratify_member_name { get; set; }
            /// <summary>
            /// 批准时间
            /// </summary>
            public DateTime? ratify_date { get; set; }
            /// <summary>
            /// 排序
            /// </summary>
            public int? sort { get; set; }
            /// <summary>
            /// 更新人ID
            /// </summary>
            public string modify_member_id { get; set; }
            /// <summary>
            /// 更新人姓名
            /// </summary>
            public string modify_member_name { get; set; }
            /// <summary>
            /// 更新时间
            /// </summary>
            [Display(Name = "更新时间")]
            [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
            public DateTime? modify_date { get; set; }


        }

        /// <summary>
        /// 聊天内容历史表  ChatLogs 改为 formain_98007
        /// </summary>
        [Table("formain_98007")]
        public class formain_98007 : BaseModel
        {

            /// <summary>
            /// 提问
            /// </summary>
            public string? field0001 { get; set; }


            /// <summary>
            /// 回答
            /// </summary>
            public string? field0002 { get; set; }


            /// <summary>
            /// 问题编号
            /// </summary>
            public string? field0003 { get; set; }

            /// <summary>
            /// 会话标识
            /// </summary>
            public string? field0004 { get; set; }

            /// <summary>
            /// 模式名称或知识库名
            /// </summary>
            public string? field0005 { get; set; }

            /// <summary>
            /// 答复耗时(秒)
            /// </summary>
            public double? field0006 { get; set; } = 0;

            /// <summary>
            /// 当前登录用户IP
            /// </summary>
            public string? field0007 { get; set; }

            /// <summary>
            /// 所属服务
            /// </summary>
            public string? field0008 { get; set; }


            /// <summary>
            /// 回复时间
            /// </summary>
            public DateTime? field0009 { get; set; }


            /// <summary>
            /// 是否推荐
            /// </summary>
            public string? field0010 { get; set; }

            /// <summary>
            /// 问答模式(大模型、并行知识库)
            /// </summary>
            public string? field0011 { get; set; }

            /// <summary>
            /// 问答接口地址
            /// </summary>
            public string? field0012 { get; set; }


            /// <summary>
            /// 标记为猜你所想
            /// </summary>
            public int? field0013 { get; set; }

            /// <summary>
            /// 内容引用
            /// </summary>
            public string? field0014 { get; set; }


            /// <summary>
            /// 知识库别称
            /// </summary>
            public string? field0015 { get; set; }
        }

        public class ChatToolbarItem
        {
            public string Text { get; set; }
            public bool Checked { get; set; }
            public bool BeginGroup { get; set; }
            public string IconCss { get; set; }
            public bool SplitMenuButton { get; set; }
            public string Category { get; set; }
            public string Tooltip { get; set; }
        }

        public class AIBotMsg
        {
            public string msgId { get; set; }
            public string msgContent { get; set; }
            public long timestamp { get; set; }
        }

        public class OpenAIOKResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice>? Choices { get; set; }

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("object")]
            public string? Object { get; set; }

            [JsonPropertyName("usage")]
            public Usage? Usage { get; set; }

            [JsonPropertyName("references")]
            public List<ContentReferences>? References { get; set; }
        }

        public class OpenAIErrorResponse
        {
            [JsonPropertyName("error")]
            public Error? Error { get; set; }
        }

        public class Error
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("param")]
            public object Param { get; set; }
        }

        public class Choice
        {
            [JsonPropertyName("finish_reason")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string FinishReason { get; set; }

            [JsonPropertyName("index")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public long? Index { get; set; }

            [JsonPropertyName("message")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public Message Message { get; set; }

            [JsonPropertyName("delta")]
            [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public Message? Delta { get; set; }

        }

        public class Usage
        {
            [JsonPropertyName("completion_tokens")]
            public long CompletionTokens { get; set; }

            [JsonPropertyName("prompt_tokens")]
            public long PromptTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public long TotalTokens { get; set; }
        }

        public class ContentReferences
        {
            public string? file { get; set; }
            public int? page { get; set; }
            public string? heading { get; set; }
        }

        public class EventSource : IDisposable
        {
            public delegate void OnOpenEventHandler(object sender);
            public delegate void OnEventHandler(object sender, OnEventArgs e);
            public delegate void OnErrorEventHandler(object sender, Exception e);
            public delegate void OnCloseEventHandler(object sender);

            public event OnOpenEventHandler? OnOpenEvent;
            public event OnEventHandler? OnEvent;
            public event OnErrorEventHandler? OnErrorEvent;
            public event OnCloseEventHandler? OnCloseEvent;

            private readonly HttpClient client;
            private readonly string url;
            private HttpMethod? method;
            private readonly HttpContent? content;
            private AuthenticationHeaderValue? authorization;
            public AuthenticationHeaderValue? Authorization { set { authorization = value; } }

            private bool isDisposed = false;
            private volatile bool isReading = false;
            private readonly object startLock = new();

            public EventSource(string url, HttpClient client)
            {
                this.url = url;
                this.client = client;
            }

            public EventSource(string url, HttpClient client, HttpMethod? method) : this(url, client)
            {
                this.method = method;
            }

            public EventSource(string url, HttpClient client, HttpMethod? method, AuthenticationHeaderValue? authorization) : this(url, client, method)
            {
                this.authorization = authorization;
            }

            public EventSource(string url, HttpClient client, HttpMethod? method, AuthenticationHeaderValue? authorization, HttpContent? content) : this(url, client, method, authorization)
            {
                this.content = content;
            }

            public bool Open()
            {
                return Open(content);
            }

            public bool Open(HttpContent? content)
            {
                if (isDisposed)
                {
                    return false;
                }
                lock (startLock)
                {
                    if (isReading == false)
                    {
                        isReading = true;
                        _ = StartAsync(content, authorization);
                    }
                }
                return true;
            }

            public void Close()
            {
                isReading = false;
            }

            public bool IsClosed()
            {
                return isReading == false;
            }

            public void Dispose()
            {
                Close();
                isDisposed = true;
            }

            private async Task StartAsync(HttpContent? content, AuthenticationHeaderValue? authorization)
            {
                if (method == null)
                {
                    method = HttpMethod.Get;
                }
                var request = new HttpRequestMessage(method, url);
                if (content != null)
                {
                    request.Content = content;
                }
                if (authorization != null)
                {
                    request.Headers.Authorization = authorization;
                }
                Stream? stream = null;
                try
                {
                    using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    OnOpenEvent?.Invoke(this);

                    if (response.Headers.TryGetValues("content-type", out IEnumerable<string>? contentTypes) &&
                        contentTypes.Contains("text/event-stream") == false)
                    {
                        throw new ArgumentException("接口类型错误");
                    }

                    stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);

                    string type = "message";
                    string id = string.Empty;
                    var data = new StringBuilder(string.Empty);
                    int blanksCount = 0;
                    bool isEnd = false; //标识正常结束
                    while (isReading)
                    {
                        string? line = await reader.ReadLineAsync();
                        if (reader.EndOfStream)
                        {
                            isEnd = true;

                            //特殊处理  data: [DONE]
                            if (line == "data: [DONE]")
                            {
                                data.Append("[DONE]");
                            }

                            if (!isReading)
                            {
                                // 如果 isReading 为 false，跳出循环
                                break;
                            }

                            if (data.Length > 0)
                            {
                                var text = data.Replace("\n\n", "\n").ToString();
                                await Task.Delay(3);
                                OnEvent?.Invoke(this, new OnEventArgs(text, type, id));
                            }
                            break;
                        }
                        if (string.IsNullOrEmpty(line))
                        {
                            // 双换行
                            if (data.Length > 0)
                            {
                                var text = data.Replace("\n\n", "\n").ToString();
                                await Task.Delay(3);
                                OnEvent?.Invoke(this, new OnEventArgs(text, type, id));
                            }
                            data.Clear();
                            id = string.Empty;
                            type = "message";
                            continue;
                        }
                        else if (line.First() == ':')
                        {
                            // 忽略备注                   
                            continue;
                        }

                        string messageType;
                        int index = line.IndexOf(':');
                        if (index == -1)
                        {
                            messageType = line;
                            index = line.Length;
                        }
                        else
                        {
                            messageType = line[..index];
                            index += 2;
                        }

                        string value = line[index..];
                        switch (messageType)
                        {
                            case "event":
                                type = value;
                                break;
                            case "data":
                                if (value == "")
                                {
                                    blanksCount++;
                                    if (blanksCount == 2)
                                    {
                                        blanksCount = 0;
                                        value = "\n";
                                    }
                                }
                                else
                                {
                                    blanksCount = 0;
                                }
                                data.Append(value);
                                break;
                            case "retry":
                                break;
                            case "id":
                                id = value;
                                break;
                            default:
                                break;
                        }
                    }

                    if (isEnd && data.Length == 0)
                    {
                        throw new Exception($"数据为空");
                    }

                }
                catch (HttpRequestException e)
                {
                    OnErrorEvent?.Invoke(this, e);
                }
                catch (Exception e)
                {
                    OnErrorEvent?.Invoke(this, e);
                }
                finally
                {
                    isReading = false;
                    stream?.Dispose();
                    try
                    {
                        OnCloseEvent?.Invoke(this);
                    }
                    catch (Exception) { }
                }
            }
        }

        public class OnEventArgs
        {
            public string Id { get; private set; }

            public string Event { get; private set; }

            public string Message { get; private set; }

            public OnEventArgs(string data, string type, string id)
            {
                Message = data;
                Event = type;
                Id = id;
            }
        }
    }
}
