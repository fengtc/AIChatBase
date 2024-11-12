using DevExtreme.AspNet.Data;
using Newtonsoft.Json;
using ParateraNetUtil.Utils.Values;
using System.ComponentModel;
using AIChatBase.Data.Models.Customs;
using AIChatBase.Data.Services.DevExpressExtensions;

namespace AIChatBase.Data.Services.OssData
{
    public partial class OssDataService
    {
        [DisplayName("获取数据源")]
        public async Task<CustomGridDevExtremeDataSource<T>> GetTEntityDataSource<T>(string TEntityApiName)
        {
            var dataSource = await GetCustomGridDevExtremeDataSource<T>($"/api/{TEntityApiName}/lists");
            return dataSource;
        }

        /// <summary>
        /// 获取列表数据
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DevResponseDto<T>> GetTEntityList<T>(DataSourceLoadOptionsBase options, string TEntityApiName)
        {
            var str = StringUtil.ModelToUriParam(options, $"/api/{TEntityApiName}/lists");
            return await Get<DevResponseDto<T>>(str);
        }

        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<T> PostTEntity<T>(T model, string TEntityApiName)
        {
            return await Post<T>($"/api/{TEntityApiName}", model);
        }

        /// <summary>
        /// 修改记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<T> PutTEntity<T>(string key, T model, string TEntityApiName)
        {
            return await Put<T>($"/api/{TEntityApiName}/{key}", model);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteTEntity(string key, string TEntityApiName)
        {
            await Delete($"/api/{TEntityApiName}/{key}");
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
            var dataSource = await GetCustomGridDevExtremeDataSource<T>($"/api/v2/List/oaContext/{type?.Name}");
            return dataSource;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DevResponseDto<T>> GetTEntityListV2<T>(DataSourceLoadOptionsBase options)
        {
            Type type = typeof(T);
            var str = StringUtil.ModelToUriParam(options, $"/api/v2/List/oaContext/{type?.Name}");
            return await Get<DevResponseDto<T>>(str);
        }

        /// <summary>
        /// 获取列表List
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<List<T>> GetTEntityDataListV2<T>(DataSourceLoadOptionsBase options)
        {
            Type type = typeof(T);
            var str = StringUtil.ModelToUriParam(options, $"/api/v2/List/oaContext/{type?.Name}");
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
                var response = await client.PostAsync($"/api/v2/List/oaContext/{type?.Name}", content);

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
            return await Post<T>($"/api/v2/Create/oaContext/{type?.Name}", model);
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
            return await Put<T>($"/api/v2/Update/oaContext/{type?.Name}/{key}", model);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteTEntityV2<T>(string key)
        {
            Type type = typeof(T);
            await Delete($"/api/v2/Delete/oaContext/{type?.Name}/{key}");
        }
        #endregion
    }
}
