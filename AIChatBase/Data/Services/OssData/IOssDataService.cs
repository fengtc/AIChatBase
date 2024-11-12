using DevExpress.Blazor.Internal;
using DevExtreme.AspNet.Data;
using AIChatBase.Data.Models.BaseData;
using AIChatBase.Data.Models.Customs;

namespace AIChatBase.Data.Services.OssData
{
    public partial interface IOssDataService
    {
        Task<string> Deleteexport_record(string key);
        Task<string> DelSaleGroupData(string key);
        Task<List<enums>> GetAllEnumsMemory();
        Task<List<Member>> GetAllMembersMemory();
        Task<List<SaleGroupInfo>> GetAllSaleGroupContainsStopMemory();
        Task<List<SaleGroupInfo>> GetAllSaleGroupForListInterfaceMemory();
        Task<List<wx_user>> GetAllWxUser();
        Task<List<wx_user>> GetAllWxUserMemory();

        Task<wx_user> GetWXUserByEmail(string email);
        Task<List<enums>> GetEnums();
        Task<DevResponseDto<export_record>> GetexportrecordList(DataSourceLoadOptions loadOptions);
        Task<List<Member>> GetMembers();
        Task<DevResponseDto<SaleGroupInfo>> GetSaleGroupData(DataSourceLoadOptions options);
        Task<List<SaleGroupInfo>> GetSaleGroupDataAll();
        Task<List<SaleGroupInfo>> GetSalesGroupContainsStop();
        Task<List<units>> GetUnits();
        Task<List<units>> GetUnitsMemory(bool containsStop = true);
        Task<long> GetUserIdMemory(string email);
        Task<wx_user> GetWXUserById(string Id);

        Task<List<wx_user>> GetWXUserByMemberId(string memberId);
        Task<export_record> Postexport_record(export_record export_record);
        Task<export_record> Putexport_record(export_record export_record);
        Task<DevResponseDto<OssMenuItem>> GetOssMenuItemList(DataSourceLoadOptionsBase options);
        Task<List<OssMenuItem>> GetAllOssMenuItemList(DataSourceLoadOptionsBase options);

        void FillBaseField<T>(in T model,bool isNew);
    }
}
