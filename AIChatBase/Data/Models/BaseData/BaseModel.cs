using Newtonsoft.Json;
using ParateraNetUtil.Utils.Values;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIChatBase.Data.Models.BaseData;

public class BaseModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
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