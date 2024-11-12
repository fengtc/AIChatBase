using System;
using System.Collections;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevExpress.Blazor;
using DevExpress.Blazor.Internal;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;


namespace AIChatBase.Data.Services.DevExpressExtensions
{
    //
    // 摘要:
    //     Allows you to bind the DevExpress.Blazor.DxGrid to a large IQueryable<T> data
    //     collection.
    //
    // 类型参数:
    //   T:
    //     The data type.
    public class CustomGridDevExtremeDataSource<T> : GridCustomDataSource
    {
        internal class HttpHelper
        {
            private static readonly JsonSerializerOptions LoadOptionsSerializerOptions;

            private static readonly JsonSerializerOptions LoadResultSerializerOptions;

            static HttpHelper()
            {
                LoadOptionsSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                LoadResultSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                LoadResultSerializerOptions.Converters.Add(new LoadResultJsonConverter<T>());
            }

            public static Task<LoadResult> LoadGroupsAsync(Type fieldType, Func<Uri, CancellationToken, Task<Stream>> httpRequestFunc, Uri url, DataSourceLoadOptionsBase loadOptions, CancellationToken cancellationToken)
            {
                Type typeFromHandle = typeof(HttpHelper);
                MethodInfo method = typeFromHandle.GetMethod("LoadGroupsInternallyAsync", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo methodInfo = method.MakeGenericMethod(fieldType);
                object[] parameters = new object[4] { httpRequestFunc, url, loadOptions, cancellationToken };
                return methodInfo.Invoke(null, parameters) as Task<LoadResult>;
            }

            public static async Task<LoadResult> LoadAsync(Func<Uri, CancellationToken, Task<Stream>> httpRequestFunc, Uri url, DataSourceLoadOptionsBase loadOptions, CancellationToken cancellationToken)
            {
                bool isSummaryQuery = loadOptions.IsSummaryQuery;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                InitializeFilterQueryParams(loadOptions, dictionary);
                if (loadOptions.IsCountQuery)
                {
                    dictionary["isCountQuery"] = "true";
                }
                else if (isSummaryQuery)
                {
                    dictionary["isSummaryQuery"] = "true";
                    dictionary["totalSummary"] = JsonSerializer.Serialize(loadOptions.TotalSummary, LoadOptionsSerializerOptions);
                    dictionary["take"] = "1";
                }
                else
                {
                    InitializeSelectQueryParams(loadOptions, dictionary);
                }

                using Stream stream = await httpRequestFunc(BuildQueryUri(url, dictionary), cancellationToken);
                LoadResult loadResult = await JsonSerializer.DeserializeAsync<LoadResult>(stream, LoadResultSerializerOptions, cancellationToken);
                if (isSummaryQuery)
                {
                    loadResult.data = null;
                }

                return loadResult;
            }

            private static async Task<LoadResult> LoadGroupsInternallyAsync<TGroupKey>(Func<Uri, CancellationToken, Task<Stream>> httpRequestFunc, Uri url, DataSourceLoadOptionsBase loadOptions, CancellationToken cancellationToken)
            {
                if (loadOptions.Group == null)
                {
                    throw new ArgumentException($"The {loadOptions.Group} option must be set up for load options", "loadOptions");
                }

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["group"] = JsonSerializer.Serialize(loadOptions.Group, LoadOptionsSerializerOptions);
                if (loadOptions.RequireGroupCount)
                {
                    dictionary["requireGroupCount"] = "true";
                }

                if (loadOptions.GroupSummary != null)
                {
                    dictionary["groupSummary"] = JsonSerializer.Serialize(loadOptions.GroupSummary, LoadOptionsSerializerOptions);
                }

                InitializeFilterQueryParams(loadOptions, dictionary);
                InitializeSelectQueryParams(loadOptions, dictionary);
                using Stream stream = await httpRequestFunc(BuildQueryUri(url, dictionary), cancellationToken);
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                jsonSerializerOptions.Converters.Add(new LoadResultJsonConverter<TypedKeyGroup<TGroupKey>>());
                return await JsonSerializer.DeserializeAsync<LoadResult>(stream, jsonSerializerOptions, cancellationToken);
            }

            private static void InitializeFilterQueryParams(DataSourceLoadOptionsBase loadOptions, Dictionary<string, string> queryParams)
            {
                if (loadOptions.Filter != null)
                {
                    queryParams["filter"] = JsonSerializer.Serialize(loadOptions.Filter);
                }
            }

            private static void InitializeSelectQueryParams(DataSourceLoadOptionsBase loadOptions, Dictionary<string, string> queryParams)
            {
                if (loadOptions.Skip > 0)
                {
                    queryParams["skip"] = Convert.ToString(loadOptions.Skip);
                }

                if (loadOptions.Take > 0)
                {
                    queryParams["take"] = Convert.ToString(loadOptions.Take);
                }

                if (loadOptions.Sort != null)
                {
                    queryParams["sort"] = JsonSerializer.Serialize(loadOptions.Sort, LoadOptionsSerializerOptions);
                }

                if (loadOptions.Select != null)
                {
                    queryParams["select"] = JsonSerializer.Serialize(loadOptions.Select, LoadOptionsSerializerOptions);
                }
            }

            private static Uri BuildQueryUri(Uri uri, Dictionary<string, string> queryParams)
            {
                UriBuilder uriBuilder = new UriBuilder(uri);
                uriBuilder.Query = uriBuilder.Query + (uriBuilder.Query.StartsWith("?") ? "&" : "?") + string.Join("&", queryParams.Select((p) => p.Key + "=" + WebUtility.UrlEncode(p.Value)));
                return uriBuilder.Uri;
            }
        }

        private class TypedKeyGroup<TKey> : Group
        {
            public new TKey key
            {
                get
                {
                    return (TKey)base.key;
                }
                set
                {
                    base.key = value;
                }
            }
        }

        internal class LoadResultJsonConverter<TResultDataItem> : JsonConverter<LoadResult>
        {
            public override LoadResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                LoadResult loadResult = new LoadResult();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        continue;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string @string = reader.GetString();
                        reader.Read();
                        switch (@string)
                        {
                            case "totalCount":
                                loadResult.totalCount = reader.GetInt32();
                                continue;
                            case "groupCount":
                                loadResult.groupCount = reader.GetInt32();
                                continue;
                            case "summary":
                                if (reader.TokenType != JsonTokenType.Null)
                                {
                                    loadResult.summary = Array.ConvertAll(JsonSerializer.Deserialize<JsonElement[]>(ref reader, options), GridUtils.UnwrapJsonElement);
                                }

                                continue;
                            case "data":
                                if (reader.TokenType != JsonTokenType.Null)
                                {
                                    loadResult.data = JsonSerializer.Deserialize<List<TResultDataItem>>(ref reader, options);
                                }

                                continue;
                        }
                    }

                    throw new NotSupportedException("HTTP response has an unexpected JSON structure.");
                }

                return loadResult;
            }

            public override void Write(Utf8JsonWriter writer, LoadResult value, JsonSerializerOptions options)
            {
                throw new NotSupportedException();
            }
        }

        //
        // 摘要:
        //     Customizes load options before the grid loads data.
        //
        // 值:
        //     A delegate method that configures load options.
        public Action<DataSourceLoadOptionsBase> CustomizeLoadOptions { get; set; }

        #region 自定义处理参数
        /// <summary>
        /// 自定义返回最大总条数
        /// </summary>
        public int CustomizeTotalCount { get; set; } = -1;

        public delegate Task<IList> CustomGridDataHandler(object sender, CustomGridDataEventArgs e);

        /// <summary>
        /// 自定义返回数据处理事件
        /// </summary>
        public event CustomGridDataHandler CustomGridDataEvent;

        #endregion

        //internal override Type ItemType => typeof(T);

        private IQueryable<T> Source { get; }

        private Uri Url { get; }

        private Func<Uri, CancellationToken, Task<Stream>> HttpRequestFunc { get; }

        //
        // 摘要:
        //     Initializes a new instance of the DevExpress.Blazor.CustomGridDevExtremeDataSource`1
        //     class with the specified data collection.
        //
        // 参数:
        //   source:
        //     A data collection.
        public CustomGridDevExtremeDataSource(IQueryable<T> source)
        {
            Source = source;
        }

        //
        // 摘要:
        //     Returns the number of data source items that match specified options.
        //
        // 参数:
        //   options:
        //     An object that contains options that data source items should match.
        //
        //   cancellationToken:
        //     An object that propagates a cancellation notification.
        //
        // 返回结果:
        //     An asynchronous operation that returns the number of items.
        public override async Task<int> GetItemCountAsync(GridCustomDataSourceCountOptions options, CancellationToken cancellationToken)
        {
            DataSourceLoadOptionsBase dataSourceLoadOptionsBase = new DataSourceLoadOptionsBase
            {
                IsCountQuery = true,
                Filter = ConvertFilter(options.FilterCriteria)
            };
            CustomizeLoadOptionsInternal(dataSourceLoadOptionsBase);
            var totalCount = (await LoadAsync(dataSourceLoadOptionsBase, cancellationToken)).totalCount;
            if (CustomizeTotalCount > 0 && totalCount > CustomizeTotalCount)
            {
                return CustomizeTotalCount;
            }
            return totalCount;
        }

        //
        // 摘要:
        //     Returns a collection of data source items selected according to specified options.
        //
        //
        // 参数:
        //   options:
        //     An object that contains options.
        //
        //   cancellationToken:
        //     An object that propagates a cancellation notification.
        //
        // 返回结果:
        //     An asynchronous operation that returns an item collection.
        public override async Task<IList> GetItemsAsync(GridCustomDataSourceItemsOptions options, CancellationToken cancellationToken)
        {
            DataSourceLoadOptionsBase dataSourceLoadOptionsBase = new DataSourceLoadOptionsBase
            {
                Filter = ConvertFilter(options.FilterCriteria),
                Sort = ConvertSort(options.SortInfo),
                Skip = options.StartIndex,
                Take = options.Count
            };
            CustomizeLoadOptionsInternal(dataSourceLoadOptionsBase);
            LoadResult loadResult = await LoadAsync(dataSourceLoadOptionsBase, cancellationToken);
            IList list = loadResult.data as IList;
            if (list != null)
            {
                #region 自定义数据返回
                if (CustomGridDataEvent is not null)
                {
                    CustomGridDataEventArgs customGridData = new CustomGridDataEventArgs();
                    customGridData.Data = list;
                    list = await CustomGridDataEvent(this, customGridData);
                }
                #endregion
                return list;
            }

            return loadResult.data.Cast<object>().ToList();
        }

        //
        // 摘要:
        //     Returns total summary values calculated according to specified options.
        //
        // 参数:
        //   options:
        //     An object that contains options according to which total summary values should
        //     be calculated.
        //
        //   cancellationToken:
        //     An object that propagates a cancellation notification.
        //
        // 返回结果:
        //     An asynchronous operation that returns a collection of total summary values.
        public override async Task<IList> GetTotalSummaryAsync(GridCustomDataSourceTotalSummaryOptions options, CancellationToken cancellationToken)
        {
            DataSourceLoadOptionsBase dataSourceLoadOptionsBase = new DataSourceLoadOptionsBase
            {
                IsSummaryQuery = true,
                Filter = ConvertFilter(options.FilterCriteria),
                TotalSummary = ConvertSummary(options.SummaryInfo)
            };
            CustomizeLoadOptionsInternal(dataSourceLoadOptionsBase);
            return (await LoadAsync(dataSourceLoadOptionsBase, cancellationToken)).summary;
        }

        //
        // 摘要:
        //     Returns an array of unique values contained in the specified column.
        //
        // 参数:
        //   options:
        //     Options that data source items should match.
        //
        //   cancellationToken:
        //     An object that propagates a cancellation notification.
        //
        // 返回结果:
        //     An asynchronous operation that returns unique column values.
        public override async Task<object[]> GetUniqueValuesAsync(GridCustomDataSourceUniqueValuesOptions options, CancellationToken cancellationToken)
        {
            DataSourceLoadOptionsBase dataSourceLoadOptionsBase = new DataSourceLoadOptionsBase();
            dataSourceLoadOptionsBase.Take = -1;
            dataSourceLoadOptionsBase.Filter = ConvertFilter(options.FilterCriteria);
            dataSourceLoadOptionsBase.Group = new GroupingInfo[1]
            {
            new GroupingInfo
            {
                Selector = options.FieldName,
                IsExpanded = false
            }
            };
            DataSourceLoadOptionsBase dataSourceLoadOptionsBase2 = dataSourceLoadOptionsBase;
            CustomizeLoadOptionsInternal(dataSourceLoadOptionsBase2);
            LoadResult loadResult = Source != null ? await DataSourceLoader.LoadAsync(Source, dataSourceLoadOptionsBase2, cancellationToken) : await HttpHelper.LoadGroupsAsync(options.FieldType, HttpRequestFunc, Url, dataSourceLoadOptionsBase2, cancellationToken);
            return (from Group g in loadResult.data
                    select g.key).ToArray();
        }

        //
        // 摘要:
        //     Returns information about groups that are generated based on specified options.
        //
        //
        // 参数:
        //   options:
        //     An object that contains options according to which the grouping operation should
        //     be applied.
        //
        //   cancellationToken:
        //     An object that propagates a cancellation notification.
        //
        // 返回结果:
        //     An asynchronous operation that returns information about groups.
        public override async Task<IList<GridCustomDataSourceGroupInfo>> GetGroupInfoAsync(GridCustomDataSourceGroupingOptions options, CancellationToken cancellationToken)
        {
            DataSourceLoadOptionsBase dataSourceLoadOptionsBase = new DataSourceLoadOptionsBase();
            dataSourceLoadOptionsBase.Take = -1;
            dataSourceLoadOptionsBase.Filter = ConvertFilter(options.FilterCriteria);
            dataSourceLoadOptionsBase.Group = new GroupingInfo[1]
            {
            new GroupingInfo
            {
                Selector = options.FieldName,
                IsExpanded = false,
                Desc = options.DescendingSortOrder
            }
            };
            dataSourceLoadOptionsBase.RequireGroupCount = true;
            dataSourceLoadOptionsBase.GroupSummary = ConvertSummary(options.SummaryInfo);
            DataSourceLoadOptionsBase dataSourceLoadOptionsBase2 = dataSourceLoadOptionsBase;
            CustomizeLoadOptionsInternal(dataSourceLoadOptionsBase2);
            LoadResult loadResult = Source != null ? await DataSourceLoader.LoadAsync(Source, dataSourceLoadOptionsBase2, cancellationToken) : await HttpHelper.LoadGroupsAsync(options.FieldType, HttpRequestFunc, Url, dataSourceLoadOptionsBase2, cancellationToken);
            return (from Group g in loadResult.data
                    select new GridCustomDataSourceGroupInfo
                    {
                        Value = g.key,
                        DataItemCount = g.count.Value,
                        SummaryValues = g.summary
                    }).ToList();
        }

        private static SortingInfo[] ConvertSort(IReadOnlyList<GridCustomDataSourceSortInfo> sortInfo)
        {
            if (sortInfo == null || sortInfo.Count < 1)
            {
                return null;
            }

            return sortInfo.Select((i) => new SortingInfo
            {
                Selector = i.FieldName,
                Desc = i.DescendingSortOrder
            }).ToArray();
        }

        private static IList ConvertFilter(CriteriaOperator filterCriteria)
        {
            if (filterCriteria.ReferenceEqualsNull())
            {
                return null;
            }

            return CriteriaToDevExtremeArrayConverter.Convert(filterCriteria);
        }

        private static SummaryInfo[] ConvertSummary(IReadOnlyList<GridCustomDataSourceSummaryInfo> summaryInfo)
        {
            if (summaryInfo == null || summaryInfo.Count < 1)
            {
                return null;
            }

            return summaryInfo.Select((i) => new SummaryInfo
            {
                Selector = i.FieldName,
                SummaryType = ConvertSummaryType(i.SummaryType)
            }).ToArray();
        }

        private static string ConvertSummaryType(GridSummaryItemType type)
        {
            return type switch
            {
                GridSummaryItemType.Count => "count",
                GridSummaryItemType.Min => "min",
                GridSummaryItemType.Max => "max",
                GridSummaryItemType.Sum => "sum",
                GridSummaryItemType.Avg => "avg",
                _ => throw new NotSupportedException(),
            };
        }

        private void CustomizeLoadOptionsInternal(DataSourceLoadOptionsBase options)
        {
            options.AllowAsyncOverSync = true;
            CustomizeLoadOptions?.Invoke(options);
        }

        private async Task<LoadResult> LoadAsync(DataSourceLoadOptionsBase loadOptions, CancellationToken cancellationToken)
        {
            if (Source != null)
            {
                return await DataSourceLoader.LoadAsync(Source, loadOptions, cancellationToken);
            }

            return await HttpHelper.LoadAsync(HttpRequestFunc, Url, loadOptions, cancellationToken);
        }

        //
        // 摘要:
        //     Initializes a new instance of the DevExpress.Blazor.CustomGridDevExtremeDataSource`1
        //     class with specified URL and function that generates an HTTP request.
        //
        // 参数:
        //   httpRequestFunc:
        //     An asynchronous function that generates an HTTP request to a service with the
        //     specified URL.
        //
        //   url:
        //     A URL to the service’s controller action that processes HTTP requests.
        public CustomGridDevExtremeDataSource(Func<Uri, CancellationToken, Task<Stream>> httpRequestFunc, Uri url)
        {
            HttpRequestFunc = httpRequestFunc;
            Url = url;
        }

        //
        // 摘要:
        //     Initializes a new instance of the DevExpress.Blazor.CustomGridDevExtremeDataSource`1
        //     class with the specified HTTP client and URL.
        //
        // 参数:
        //   httpClient:
        //     An object that sends HTTP requests and receives HTTP responses from a service
        //     identified by the URI.
        //
        //   url:
        //     A URL to the service’s controller action that processes HTTP requests.
        public CustomGridDevExtremeDataSource(HttpClient httpClient, Uri url)
            : this(httpClient.GetStreamAsync, url)
        {
        }
    }
}