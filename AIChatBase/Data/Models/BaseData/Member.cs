using Newtonsoft.Json;
using ParateraNetUtil.Utils.Values;

namespace AIChatBase.Data.Models.BaseData;

public class Member
{
    [JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
    public long id { get; set; }
    public string name { get; set; }
    public string code { get; set; }
    public string mobile { get; set; }
    public string email { get; set; }
    public string departmenT_NAME { get; set; }
    public string company { get; set; }
    public string status { get; set; }
    public string isenable { get; set; }

    public string orG_DEPARTMENT_ID { get; set; }

    public string exT_ATTR_2 { get; set; }

    public string exT_ATTR_1 { get; set; }

    public int iS_DELETED { get; set; }
}
