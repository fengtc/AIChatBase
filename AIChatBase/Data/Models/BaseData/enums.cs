using Newtonsoft.Json;
using ParateraNetUtil.Utils.Values;

namespace AIChatBase.Data.Models.BaseData;

public class enums
{
    [JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
    public long id { get; set; }
    [JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
    public long? reF_ENUMID { get; set; }
    public string showvalue { get; set; }
    public string enumvalue { get; set; }
    public int? sortnumber { get; set; }
    public int? state { get; set; }
    public int? outpuT_SWITCH { get; set; }
    public long? orG_ACCOUNT_ID { get; set; }
    public long? parenT_ID { get; set; }
    public long? rooT_ID { get; set; }
    public int? leveL_NUM { get; set; }
    public string description { get; set; }
    public string ifuse { get; set; }
    public int? i18N { get; set; }
    public long? exT1 { get; set; }
}
