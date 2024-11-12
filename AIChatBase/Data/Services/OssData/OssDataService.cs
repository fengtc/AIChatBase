using DevExpress.Blazor.Internal;
using DevExtreme.AspNet.Data;
using ParateraNetUtil.Utils.Https;
using ParateraNetUtil.Utils.Values;
using System.Net.Http.Headers;
using System.Security.Claims;
using OSSEncryption;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using AIChatBase.Data.Services.OssId;
using AIChatBase.Data.Services.Memory;
using AIChatBase.Data.Models.BaseData;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Authentication;
using AIChatBase.Data.Services.DevExpressExtensions;

namespace AIChatBase.Data.Services.OssData
{
    public partial class OssDataService : BaseClient,IOssDataService
    {
        private readonly IOssIdService _ossIdService;
        /// <summary>
        /// 内存
        /// </summary>
        protected readonly IBxMemoryService _bxMemoryService;

        private readonly CustomAuthenticationService _customAuthenticationService;

        private EncryptionService _encryptionService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public OssDataService(ILogger<OssDataService> logger, IOssIdService ossIdService, IHttpClientFactory httpClientFactory, IBxMemoryService bxMemoryService, CustomAuthenticationService customAuthenticationService, EncryptionService encryptionService)
            : base(logger, nameof(OssDataService), httpClientFactory)
        {
            _ossIdService = ossIdService;
            _bxMemoryService = bxMemoryService;
            _customAuthenticationService = customAuthenticationService;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// 配置客户端
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected override async Task ConfigClient(HttpClient client)
        {
            var ossIdToken = await _ossIdService.GetOssIdToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ossIdToken);
            client.DefaultRequestHeaders.Add("MethodDisplayName", GetCallerMethodDisplayName());
        }

        [Display(Name = "获取方法名称")]
        public static string GetCallerMethodDisplayName()
        {
            // 获取当前堆栈跟踪
            StackTrace stackTrace = new StackTrace();

            // 获取上一级调用方法的堆栈帧
            StackFrame frame = stackTrace.GetFrame(4);

            // 获取方法信息
            var method = frame.GetMethod();

            // 获取DisplayName属性
            var displayNameAttribute = method.GetCustomAttribute<DisplayNameAttribute>();

            // 返回DisplayName属性的值，如果没有则返回方法名称
            return displayNameAttribute != null ? displayNameAttribute.DisplayName : method.Name;
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

        #region 部门
        /// <summary>
        /// 获取部门列表信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<units>> GetUnits()
        {
            return await Get<List<units>>("/api/Units");
        }
        /// <summary>
        /// 基于内存
        /// </summary>
        /// <param name="containsStop"></param>
        /// <returns></returns>
        public async Task<List<units>> GetUnitsMemory(bool containsStop = true)
        {
            var result = await _bxMemoryService.GetOrCreateAsync(nameof(OssDataService) + nameof(GetUnitsMemory), async () =>
            {
                return await GetUnits();
            });

            if (containsStop) return result;


            var allunits = result.Where(o => o.iS_ENABLE == 1 && o.status == 1 && !o.name.Contains("-停") && !o.name.Contains("-停用")).OrderBy(o => o.sorT_ID).ToList();
            var repeatData = allunits.GroupBy(x => x.name).Where(x => x.Count() > 1).ToList();

            // 过滤重复数据
            foreach (var item in repeatData)
            {
                var currentData = item.ToList().FirstOrDefault();
                if (currentData != null)
                {
                    allunits.Remove(currentData);
                }
            }
            return allunits;

        }
        #endregion

        #region 枚举信息
        /// <summary>
        /// 读取枚举信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<enums>> GetEnums()
        {
            return await Get<List<enums>>("/api/Enums");

        }


        /// <summary>
        /// 获取所有的枚举
        /// </summary>
        /// <returns></returns>
        public async Task<List<enums>> GetAllEnumsMemory()
        {
            return await _bxMemoryService.GetOrCreateAsync(nameof(OssDataService) + nameof(GetAllEnumsMemory), async () =>
            {
                return await GetEnums();
            });
        }

        #endregion

        #region 微信用户信息
        /// <summary>
        /// 读取所有微信用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<wx_user>> GetAllWxUser()
        {
            var resp = await Get<List<wx_user>>($"/api/Wxusers/single");
            return resp;
        }
        /// <summary>
        /// 根据memberId获取WXUser列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<wx_user>> GetWXUserByMemberId(string memberId)
        {
            var resp = await Get<List<wx_user>>($"/api/Wxusers/member/{memberId}");
            return resp;
        }


        /// <summary>
        /// 根据Id获取WXUser
        /// </summary>
        /// <returns></returns>
        public async Task<wx_user> GetWXUserById(string Id)
        {
            var resp = await Get<wx_user>($"/api/Wxusers/{Id}");
            return resp;
        }


        /// <summary>
        /// 获取所有的微信用户
        /// </summary>
        /// <returns></returns>
        public async Task<List<wx_user>> GetAllWxUserMemory()
        {
            return await _bxMemoryService.GetOrCreateAsync(nameof(OssDataService) + nameof(GetAllWxUserMemory), async () =>
            {
                return await GetAllWxUser();
            });
        }

        #region WXUser
        /// <summary>
        /// 获取WXUser列表
        /// </summary>
        /// <returns></returns>
        public async Task<wx_user> GetWXUserByEmail(string email)
        {
            var result = await Get<List<wx_user>>($"/api/Wxusers/email/{email}");
            return result?.FirstOrDefault();
        }
        #endregion

        #endregion

        #region 员工信息
        /// <summary>
        /// 获取所有的员工
        /// </summary>
        /// <returns></returns>
        public async Task<List<Member>> GetMembers()
        {
            return await Get<List<Member>>("/api/Members");
        }

        /// <summary>
        /// 获取所有的销售人员
        /// </summary>
        /// <returns></returns>
        public async Task<List<Member>> GetAllMembersMemory()
        {
            return await _bxMemoryService.GetOrCreateAsync(nameof(OssDataService) + nameof(GetAllMembersMemory), async () =>
            {
                return await GetMembers();
            });
        }
    
        
        /// <summary>
        /// 获取用户Id
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<long> GetUserIdMemory(string email)
        {
            var members = await GetAllMembersMemory();
            var currentMember = members.FirstOrDefault(x => x.email == email);
            return currentMember != null ? currentMember.id : 0;
        }

        #endregion

        #region 销售人员信息
        /// <summary>
        /// 根据条件读取销售人员信息
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DevResponseDto<SaleGroupInfo>> GetSaleGroupData(DataSourceLoadOptions options)
        {
            var str = StringUtil.ModelToUriParam(options, "/api/PipeSaleGroupInfo/lists");
            return await Get<DevResponseDto<SaleGroupInfo>>(str);
        }

        /// <summary>
        /// 读取销售人员信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<SaleGroupInfo>> GetSaleGroupDataAll()
        {
            return await Get<List<SaleGroupInfo>>("/api/PipeSaleGroupInfo");
        }

        /// <summary>
        /// 删除销售人员
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> DelSaleGroupData(string key)
        {
            return await Delete<string>($"/api/PipeSaleGroupInfo/{key}");
        }


        /// <summary>
        /// 获取组织架构
        /// </summary>
        /// <returns></returns>
        public async Task<List<SaleGroupInfo>> GetSalesGroupContainsStop()
        {
            var result = await Get<List<SaleGroupInfo>>($"/api/PipeSaleGroupInfo");
            if (result != null && result.Count > 0) return result;
            return new List<SaleGroupInfo>();
        }


        /// <summary>
        /// 获取所有销售组
        /// </summary>
        /// <returns></returns>
        public async Task<List<SaleGroupInfo>> GetAllSaleGroupContainsStopMemory()
        {
            return await _bxMemoryService.GetOrCreateAsync(nameof(OssDataService) + nameof(GetAllSaleGroupContainsStopMemory), async () =>
            {
                return await GetSalesGroupContainsStop();
            });
        }

        /// <summary>
        /// 获取所有销售组
        /// </summary>
        /// <returns></returns>
        public async Task<List<SaleGroupInfo>> GetAllSaleGroupForListInterfaceMemory()
        {
            return await _bxMemoryService.GetOrCreateAsync(nameof(OssDataService) + nameof(GetAllSaleGroupForListInterfaceMemory), async () =>
            {
                var queryOptions = new DataSourceLoadOptionsBase() { RequireTotalCount = true };
                return (await GetSaleGroupData(queryOptions))?.data;
            });
        }

        #endregion

        #region 导出记录
        /// <summary>
        /// 获取导出记录数据通过DevExpress
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        public async Task<DevResponseDto<export_record>> GetexportrecordList(DataSourceLoadOptions loadOptions)
        {
            var requestStr = StringUtil.ModelToUriParam(loadOptions, "/api/ExportRecord/lists");
            return await Get<DevResponseDto<export_record>>(requestStr);
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="export_record"></param>
        /// <returns></returns>
        public async Task<export_record> Postexport_record(export_record export_record)
        {
            return await Post<export_record>("/api/ExportRecord", export_record);
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="export_record"></param>
        /// <returns></returns>
        public async Task<export_record> Putexport_record(export_record export_record)
        {
            return await Put<export_record>($"/api/ExportRecord/{export_record.ID}", export_record);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> Deleteexport_record(string key)
        {
            return await Delete<string>($"/api/ExportRecord/{key}");
        }
        #endregion

        #region 菜单获取

        /// <summary>
        /// 获取菜单配置数据(10条以内)
        /// </summary>
        /// <returns></returns>
        public async Task<DevResponseDto<OssMenuItem>> GetOssMenuItemList(DataSourceLoadOptionsBase options)
        {
            var str = StringUtil.ModelToUriParam(options, "/api/OssMenuItem/lists");
            return await Get<DevResponseDto<OssMenuItem>>(str);
        }

        /// <summary>
        /// 分页获取所有菜单配置数据
        /// </summary>
        /// <returns></returns>
        public async Task<List<OssMenuItem>> GetAllOssMenuItemList(DataSourceLoadOptionsBase options)
        {
            options.RequireTotalCount = true;
            options.Take = 10;
            options.Skip = 0;
            var reOssMenuItemList = new List<OssMenuItem>();
            var str = StringUtil.ModelToUriParam(options, "/api/OssMenuItem/lists");
            var reData = await Get<DevResponseDto<OssMenuItem>>(str);
            if(reData != null)
            {
                reOssMenuItemList.AddRange(reData.data);
                var totalCount = reData.totalCount;
                if (totalCount > options.Take) 
                {
                    for (var i = 1; i * options.Take < totalCount; i++) 
                    {
                        options.Skip = i * options.Take;
                        str = StringUtil.ModelToUriParam(options, "/api/OssMenuItem/lists");
                        reData = await Get<DevResponseDto<OssMenuItem>>(str);
                        if (reData != null) 
                        {
                            reOssMenuItemList.AddRange(reData.data);
                        }
                    }
                }
                
            }
            return reOssMenuItemList;
        }

        public void FillBaseField<T>(in T model, bool isNew = false)
        {
            try
            {
                var fields = model.GetType().GetProperties();
                foreach (var fieldInfo in fields)
                {
                    switch (fieldInfo.Name)
                    {
                        case "modify_date" or "updateAt" or "update_at" or "updated_at":
                            fieldInfo.SetValue(model,DateTime.Now);
                            break;
                        case "modify_member_id":
                            fieldInfo.SetValue(model,_customAuthenticationService.GetClaimValue("memberid"));
                            break;
                        case "modify_member_name":
                            fieldInfo.SetValue(model,_customAuthenticationService.GetClaimValue(ClaimTypes.Name));
                            break;
                        case "start_date" or "createAt" or "created_at" or "create_at":
                            if (isNew)
                            {
                                fieldInfo.SetValue(model,DateTime.Now);
                            }
                            break;
                        case "start_member_id":
                            if (isNew)
                            {
                                fieldInfo.SetValue(model,_customAuthenticationService.GetClaimValue("memberid"));
                            }
                            break;
                        case "start_member_name":
                            if (isNew)
                            {
                                fieldInfo.SetValue(model,_customAuthenticationService.GetClaimValue(ClaimTypes.Name));
                            }
                            break;
                    }
                    
                    
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion
    }
}
