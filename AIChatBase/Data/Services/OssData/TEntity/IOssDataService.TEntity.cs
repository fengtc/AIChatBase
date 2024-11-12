using DevExpress.Blazor.Internal;
using DevExtreme.AspNet.Data;
using ParateraNetUtil.Utils.Values;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Data.Services.DevExpressExtensions;

namespace AIChatBase.Data.Services.OssData
{
    public partial interface IOssDataService
    {
        Task<CustomGridDevExtremeDataSource<T>> GetTEntityDataSource<T>(string TEntityApiName);

        Task<DevResponseDto<T>> GetTEntityList<T>(DataSourceLoadOptionsBase options, string TEntityApiName);

        Task<T> PostTEntity<T>(T model, string TEntityApiName);

        Task<T> PutTEntity<T>(string key, T model, string TEntityApiName);

        Task DeleteTEntity(string key, string TEntityApiName);

        Task<CustomGridDevExtremeDataSource<T>> GetTEntityDataSourceV2<T>();

        Task<DevResponseDto<T>> GetTEntityListV2<T>(DataSourceLoadOptionsBase options);

        Task<DevResponseDto<T>> PostGetTEntityV2<T>(DataSourceLoadOptionsBase options);
        Task<List<T>> GetTEntityDataListV2<T>(DataSourceLoadOptionsBase options);

        Task<T> PostTEntityV2<T>(T model);

        Task<T> PutTEntityV2<T>(string key, T model);

        Task DeleteTEntityV2<T>(string key);
    }
}
