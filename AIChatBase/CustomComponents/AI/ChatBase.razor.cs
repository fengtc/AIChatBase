using DevExpress.Blazor;
using DevExtreme.AspNet.Data;
using Markdig.Extensions.Tables;
using Markdig;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using ParateraNetUtil.Utils.Auths;
using System.Text.RegularExpressions;

namespace AIChatBase.CustomComponents.AI
{
    public partial class ChatBase
    {
        #region 参数配置
        /// <summary>
        /// 无数据显示快捷提问列表
        /// </summary>
        [Parameter] public List<string> EmptyQuickQuestions { get; set; } = new List<string>();
        /// <summary>
        /// 是否获取历史数据
        /// </summary>
        [Parameter] public bool EnableHistoryData { get; set; } = false;
        /// <summary>
        /// 登录用户信息
        /// </summary>
        [Parameter] public ClaimInfo ClaimsInfo { get; set; } = new ClaimInfo()
        {
            WXUserId = "user",
            OAMemberId = "user",
            UserName = "用户"
        };
        [Parameter] public EventCallback OnNoAuth { get; set; }
        #endregion
        #region 内部参数
        [Inject] IToastNotificationService ToastService { get; set; }
        DxListBox<ChatMessage, ChatMessage> AIListBox;
        List<ChatMessage> ChatData { get; set; } = new List<ChatMessage>();
        DxMemo ChatMemo;
        ChatMessage BindChatValue { get; set; }
        private string LoginUserSessionId { get; set; }
        private string ServiceName { get; set; }
        private bool IsMobileLogin = true;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            ServiceName = _configuration["ServiceName"];
            LoginUserSessionId = $"{ServiceName}{ClaimsInfo.WXUserId}";
            var isMobile = await _localStorage.GetItemAsync<bool?>("is_mobile_login");
            if (isMobile == false) { IsMobileLogin = false; };
            EmptyQuickQuestions.ForEach(item =>
            {
                EmptyItems.Add(new ChatToolbarItem() { Text = item });
            });
            if (EnableHistoryData) 
            {
                await SetChatData();
            }
            //初始化OpenAI接口请求对象
            await InitChatClient();
            await base.OnInitializedAsync();
            StateHasChanged();
            await JS.InvokeVoidAsync("scrollListBoxToBottom", "chat-list-box");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (EnableHistoryData)
                {
                    await JS.InvokeVoidAsync("registerScrollEvent", "chat-list-box", DotNetObjectReference.Create(this));
                }
            }
            if (IsSending)
            {
                await JS.InvokeVoidAsync("scrollListBoxToBottom", "chat-list-box");
            }

        }

        #region 初始化AI请求参数
        /// <summary>
        /// AI问答模型配置
        /// </summary>
        private ChatModelConfig ModelConfig { get; set; }
        //openai接口入参对象
        private OpenAIRequest openAIRequest { get; set; }
        private List<Message> sendMessageList = new List<Message>();
        //初始化OpenAI接口请求对象
        async Task InitChatClient()
        {
            var userToken = Convert.ToBoolean(_configuration["OssAgentUseUserToken"]);
            var token = _configuration["OssAgentServiceToken"];
            if (userToken)
            {
                token = await _localStorage.GetItemAsync<string>("oss_token");
            }
            ModelConfig = new ChatModelConfig()
            {
                ModeName = _configuration["ChatModelConfig:ModeName"],
                MaxTokens = Convert.ToInt32(_configuration["ChatModelConfig:MaxTokens"]),
                IsStream = Convert.ToBoolean(_configuration["ChatModelConfig:IsStream"]),
                ChatApi = $"{_configuration["OssAgentServiceApi"]}{_configuration["ChatModelConfig:ChatApi"]}",
                Token = token,
            };
            #region 初始化OpenAI接口
            //入参对象
            openAIRequest = new OpenAIRequest()
            {
                model = ModelConfig.ModeName,
                max_tokens = ModelConfig.MaxTokens,
                stream = ModelConfig.IsStream,
                messages = new List<Message>()
            };


            _chatClient.ChatApi = ModelConfig.ChatApi;
            _chatClient.Stream = ModelConfig.IsStream;
            _chatClient.MaxTextLength = ModelConfig.MaxTokens;
            _chatClient.Authentication = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ModelConfig.Token); ;

            #endregion

            //流式输出 配置
            await InitOutputEvents(ModelConfig.IsStream);
        }

        //初始化OpenAI接口的输出事件
        private async Task InitOutputEvents(bool isStream)
        {
            string outputType = isStream ? "流式" : "非流式";

            //等待请求
            _chatClient.OnWaitEvent += delegate (object sender, CallBackModel callBackData)
            {
                _ = InvokeAsync(async () =>
                {
                    _logger.LogInformation($"[{outputType}] OnWaitEvent");
                });
            };
            //开始请求
            _chatClient.OnStartEvent += delegate (object sender, CallBackModel callBackData)
            {
                _ = InvokeAsync(() =>
                {
                    _logger.LogInformation($"[{outputType}] OnStartEvent");
                });
            };
            //回复内容
            _chatClient.OnMessage += delegate (object sender, CallBackModel callBackData)
            {
                _ = InvokeAsync(async () =>
                {
                    if (!isStream)
                    {
                        _logger.LogInformation($"[{outputType}] OnMessage");
                    }

                    string answer = callBackData.Message;

                    //没有回复内容时，直接返回
                    if (string.IsNullOrEmpty(answer))
                    {
                        return;
                    }

                    //记录回复内容
                    var item = ChatData.FirstOrDefault(n => n.GroupId == callBackData.GroupId && n.UserType == "AI");
                    if (item != null)
                    {
                        item.ReplyContent += answer;
                    }
                    StateHasChanged();
                    //调整滚动条的位置 到底部
                    //await AIListBox.MakeDataItemVisibleAsync(item);
                });
            };
            //报错信息
            _chatClient.OnErrorEvent += delegate (object sender, Exception e, CallBackModel callBackData)
            {
                _ = InvokeAsync(async () =>
                {
                    _logger.LogInformation($"[{outputType}] OnErrorEvent");

                    //记录报错内容
                    var item = ChatData.FirstOrDefault(n => n.GroupId == callBackData.GroupId && n.UserType == "AI");
                    if (item != null)
                    {
                        item.ReplyContent += e.Message;
                    }
                    StateHasChanged();
                    //调整滚动条的位置 到底部
                    //await AIListBox.MakeDataItemVisibleAsync(item);
                });
            };
            //请求完成
            _chatClient.OnFinishEvent += delegate (object sender, CallBackModel callBackData)
            {
                _ = InvokeAsync(async () =>
                {
                    _logger.LogInformation($"[{outputType}] OnFinishEvent");


                    //记录耗时
                    var item = ChatData.FirstOrDefault(n => n.GroupId == callBackData.GroupId && n.UserType == "AI");
                    if (item != null)
                    {
                        item.ReplyTime = DateTime.Now;
                        item.asnwer_times = (DateTime.Now - item.Time).TotalSeconds;
                        item.IsFinish = true;
                        sendMessageList.Add(new Message() { role = "assistant", content = item.ReplyContent });
                        UpdateChatLog(item);
                    }


                    //提问按钮启用
                    IsSending = false;
                    //刷新页面数据
                    StateHasChanged();
                    await JS.InvokeVoidAsync("scrollListBoxToBottom", "chat-list-box");
                    if (!IsMobileLogin)
                    {
                        await ChatMemo.FocusAsync();
                    }
                });
            };
        }
        #endregion

        #region 初始化历史数据
        int HistoryDataCount = 0;
        bool NoLoadData = false;
        public async Task SetChatData()
        {
            // 默认实现
            if (ClaimsInfo != null)
            {
                ChatData = ChatData ?? new List<ChatMessage>();
                var options = new DataSourceLoadOptionsBase();
                options.Skip = HistoryDataCount;
                options.Take = 10;
                options.Sort = new SortingInfo[]
                {
               new SortingInfo(){Desc = true,Selector = "start_date"}
                };
                options.Filter = new List<object>() {
               new List<object>(){ "state", "=", 0},
               "and",
               new List<object>(){ "start_member_id", "=", ClaimsInfo.OAMemberId },
               "and",
               new List<object>(){ nameof(formain_98007.field0004), "=", LoginUserSessionId },
               "and",
               new List<object>(){ nameof(formain_98007.field0008), "=", ServiceName },
            };

                var formain98007s = await _ossAgentService.GetTEntityDataListV2<formain_98007>(options);
                if (formain98007s != null)
                {
                    if (formain98007s.Count > 0)
                    {
                        HistoryDataCount += formain98007s.Count;
                        var oldChatData = new List<ChatMessage>();
                        foreach (var formain98007 in formain98007s.OrderBy(o => o.start_date).ToList())
                        {
                            oldChatData.Add(new ChatMessage()
                            {
                                Sender = formain98007.start_member_name,
                                UserType = "User",
                                Content = formain98007.field0001,
                                Time = Convert.ToDateTime(formain98007.start_date),
                                GroupId = formain98007.field0003,
                                SessionId = formain98007.field0004
                            });
                            oldChatData.Add(new ChatMessage()
                            {
                                Sender = "AI",
                                UserType = "AI",
                                ReplyContent = formain98007.field0002,
                                ReplyTime = formain98007.field0009,
                                GroupId = formain98007.field0003,
                                asnwer_times = formain98007.field0006,
                                SessionId = formain98007.field0004,
                                IsFinish = true,
                            });
                        }
                        ChatData.InsertRange(0, oldChatData);
                        var visibleIndex = HistoryDataCount == formain98007s.Count ? oldChatData.Count : (oldChatData.Count - 1);
                        AIListBox.MakeItemVisible(visibleIndex);
                        //await AIListBox.MakeDataItemVisibleAsync(ChatData.LastOrDefault());
                    }
                    else
                    {
                        NoLoadData = true;
                    }
                }
            }
        }

        bool IsLoadMoreData { get; set; } = false;

        [JSInvokable]
        public async Task LoadMoreData()
        {
            if (!NoLoadData)
            {
                try
                {
                    IsLoadMoreData = true;
                    StateHasChanged();
                    await SetChatData();
                }
                catch { }
                IsLoadMoreData = false;
                StateHasChanged();
            }
        }
        #endregion

        #region 发送消息
        int SendType = 0;
        string SendText { get; set; }
        bool IsSending { get; set; } = false;

        public virtual async Task Send()
        {
            // 默认实现
            try
            {
                //处理换行符
                var message = SendText?.Trim().Replace("\n", "<br>");

                //清空提问内容
                SendText = string.Empty;

                //没有提问内容或者提问按钮没有禁用
                if (string.IsNullOrWhiteSpace(message) || IsSending)
                {
                    return;
                }
                IsSending = true;
                string groupId = Guid.NewGuid().ToString().Replace("-", "");
                var chatDataQ = new ChatMessage()
                {
                    Sender = ClaimsInfo.UserName,
                    UserType = "User",
                    Content = message,  //测试消息：scontrol如何查看正在运行的作业信息
                    Time = DateTime.Now,
                    GroupId = groupId,
                    SessionId = LoginUserSessionId
                };
                ChatData.Add(chatDataQ);
                var chatDataA = new ChatMessage()
                {
                    Sender = "AI",
                    UserType = "AI",
                    Time = DateTime.Now,
                    GroupId = groupId,
                    SessionId = LoginUserSessionId
                };
                ChatData.Add(chatDataA);
                await AIListBox.MakeDataItemVisibleAsync(chatDataA);
                AddChatLog(groupId, message);
                sendMessageList.Add(new Message() { role = "user", content = message });
                //发起OpenAI接口请求
                openAIRequest.messages = sendMessageList;
                var callBackModel = new CallBackModel() { GroupId = groupId, SessionId = LoginUserSessionId };
                await _chatClient.GetAnswer(openAIRequest, callBackModel);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"[发起提问] 保存数据报异常：{ex.Message}");
                IsSending = false;
            }
        }
       
        //快捷方式提问
        private async Task OnKeyDown(KeyboardEventArgs e)
        {

            if (e.ShiftKey == false && e.Key == "Enter")
            {
                await Send();
            }
        }

        #endregion

        #region 无权限处理
        async Task ApplyPermissionBotSend()
        {
            await OnNoAuth.InvokeAsync();
            ToastService.ShowToast(new ToastOptions
            {
                ProviderName = "CustomToast",
                Text = "申请已提交，请等通知!",
                RenderStyle = ToastRenderStyle.Success
            });
        }
        #endregion

        #region 显示文本转换
        //将内容转化成MarkDown格式输出
        private MarkupString ConvertToMarkdown(string markdownData, string userType)
        {
            if (!string.IsNullOrEmpty(markdownData))
            {
                var builder = (new MarkdownPipelineBuilder());
                builder.Extensions.Add(new PipeTableExtension());
                var pipeline = builder.Build();

                // markdown 转为 html
                var htmlData = Markdown.ToHtml(markdownData, pipeline);

                // 使用正则表达式替换所有图片链接
                // htmlData = Regex.Replace(htmlData, @"<a href=""(https://[^""]+\.(?:png|jpg|jpeg|gif))"">([^<]+)</a>",
                //     match => $"<a href=\"#\" onclick=\"ShowImageModal('{match.Groups[1].Value}')\">{match.Groups[2].Value}</a>");

                // 使用正则表达式替换所有 img 标签
                // htmlData = Regex.Replace(htmlData, @"<img src=""(https://[^""]+/([^""]+)\.(?:png|jpg|jpeg|gif))""[^>]*>",
                //     match => $"<a href=\"#\" onclick=\"ShowImageModal('{match.Groups[1].Value}')\">{Path.GetFileNameWithoutExtension(match.Groups[2].Value)}</a>");

                // 使用正则表达式替换所有图片链接
                htmlData = Regex.Replace(htmlData, @"<a href=""(https://[^""]+\.(?:png|jpg|jpeg|gif))"">([^<]+)</a>",
                    match => $"<a href=\"{match.Groups[1].Value}\" target=\"_blank\">{match.Groups[2].Value}</a>");

                // 使用正则表达式替换所有 img 标签
                htmlData = Regex.Replace(htmlData, @"<img src=""(https://[^""]+/([^""]+)\.(?:png|jpg|jpeg|gif))""[^>]*>",
                    match => $"<a href=\"{match.Groups[1].Value}\" target=\"_blank\">{Path.GetFileNameWithoutExtension(match.Groups[2].Value)}</a>");

                // 使用正则表达式替换所有 h1, h2, h3, h4, h5, h6 标签为 strong 标签
                htmlData = Regex.Replace(htmlData, @"<h[1-6]>", "<strong>");
                htmlData = Regex.Replace(htmlData, @"</h[1-6]>", "</strong>");

                // 使用正则表达式替换表格标签增加自动行滚动
                htmlData = Regex.Replace(htmlData, @"<table>", "<div style=\"overflow-x:auto;\"><table>");
                htmlData = Regex.Replace(htmlData, @"</table>", "</table></div>");

                // 将 普通文本 转为 可以渲染的html的类型
                return (MarkupString)htmlData;
            }

            return (MarkupString)"";
        }
        #endregion

        #region 快捷输入
        private List<ChatToolbarItem> EmptyItems { get; set; } = new List<ChatToolbarItem>();
        async Task EmptyItemClick(string text)
        {
            SendText = text;
            await Send();
        }
        #endregion

        #region 日志记录
        //保存聊天内容
        async Task AddChatLog(string groupId, string message)
        {
            try
            {
                //聊天内容保存到数据库
                var dateTimeNow = DateTimeHelper.GetDateTime(DateTime.Now);
                var newChatLogs = new formain_98007()
                {
                    field0004 = LoginUserSessionId,
                    field0001 = message,
                    field0003 = groupId,
                    //field0007 = _userInitialState.IP,
                    field0005 = ModelConfig.ModeName,
                    // field0011 = SelectedModelType,
                    // field0015 = chatTaskName,
                    field0002 = "",
                    field0012 = ModelConfig.ChatApi,
                    field0009 = dateTimeNow,
                    field0008 = ServiceName,
                    state = 0,
                    start_date = dateTimeNow,
                    start_member_name = ClaimsInfo.UserName,
                    start_member_id = ClaimsInfo.OAMemberId,
                    modify_date = dateTimeNow,
                    modify_member_name = ClaimsInfo.UserName,
                    modify_member_id = ClaimsInfo.OAMemberId
                };
                await _ossAgentService.PostTEntityV2<formain_98007>(newChatLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[记录发送内容] 保存数据报异常：{ex.Message}  SessionId={LoginUserSessionId}  GroupId={groupId}");
            }
        }

        public async Task UpdateChatLog(ChatMessage item)
        {
            //保存到数据库(回复内容、耗时)
            try
            {
                var options = new DataSourceLoadOptionsBase();
                options.Skip = 0;
                options.Take = 1;
                options.Filter = new List<object>() {
                    new List<object>(){ nameof(formain_98007.field0003), "=", item.GroupId },
                    "and",
                    new List<object>(){ nameof(formain_98007.field0008), "=", ServiceName },
                };

                var formain98007s = await _ossAgentService.GetTEntityDataListV2<formain_98007>(options);

                var info = formain98007s?.FirstOrDefault();
                if (info != null)
                {
                    info.field0002 = item.ReplyContent;
                    info.modify_member_name = ClaimsInfo.UserName;
                    info.modify_member_id = ClaimsInfo.OAMemberId;
                    info.field0006 = item.asnwer_times;
                    info.field0009 = DateTimeHelper.GetDateTime(item.ReplyTime.Value);
                    info.field0014 = item.content_references;

                    await _ossAgentService.PutTEntityV2<formain_98007>(info.ID.ToString(), info);
                }
                _logger.LogInformation($"将回复内容 保存到数据库。 SessionId={item.SessionId}  GroupId={item.GroupId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[记录回复内容] 保存数据报异常：{ex.Message}  SessionId={item.SessionId}  GroupId={item.GroupId}");
            }
        }
        #endregion

        #region 样式动态获取 
        string getContetClass(string userType)
        {
            return userType == "AI" ? "chat-content-left" : "chat-content-right";
        }

        string getMessageClass(string userType)
        {
            return userType == "AI" ? "chat-message-left" : "chat-message-right";
        }
        #endregion
    }
}
