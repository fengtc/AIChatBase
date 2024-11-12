using Newtonsoft.Json;
using ParateraNetUtil.Utils.Values;

namespace AIChatBase.Data.Models.BaseData;

/// <summary>
/// 部门信息
/// </summary>
public class units
{
    [JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
    public long id { get; set; }
    public string name { get; set; }

    /// <summary>
    /// 当前部门的根节点id
    /// </summary>
    [JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
    public long orG_ACCOUNT_ID { get; set; }

    /// <summary>
    /// 状态 1可用 0不可用 
    /// </summary>
    public int? status { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public int? sorT_ID { get; set; }
    /// <summary>
    /// 是否展示 1：展示 0：不展示
    /// </summary>
    public int? iS_ENABLE { get; set; }

    /// <summary>
    /// 当前节点
    /// </summary>
    public string path { get; set; }
}
