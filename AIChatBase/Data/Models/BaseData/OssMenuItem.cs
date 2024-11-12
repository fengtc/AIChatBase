using Newtonsoft.Json;
using ParateraNetUtil.Utils.Values;

namespace AIChatBase.Data.Models.BaseData
{
    /// <summary>
    /// 自定义菜单 
    /// </summary>
    public class OssMenuItem
    {
        [JsonConverter(typeof(NumberConverter), NumberConverterShip.Int64)]
        public long id { get; set; }

        #region

        public int? state { get; set; }
        public string start_member_id { get; set; }
        public string start_member_name { get; set; }
        public DateTime? start_date { get; set; }
        public string approve_member_id { get; set; }
        public string approve_member_name { get; set; }
        public DateTime? approve_date { get; set; }
        public int? finishedflag { get; set; }
        public int? ratifyflag { get; set; }
        public string ratify_member_id { get; set; }
        public string ratify_member_name { get; set; }
        public DateTime? ratify_date { get; set; }
        public int? sort { get; set; }
        public string modify_member_id { get; set; }
        public string modify_member_name { get; set; }
        public DateTime? modify_date { get; set; }
        public int? confirmflag { get; set; }
        public string confirm_member_id { get; set; }
        public string confirm_member_name { get; set; }
        public DateTime? confirm_date { get; set; }

        #endregion

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 菜单服务
        /// </summary>
        public string serviceName { get; set; }
        /// <summary>
        /// 菜单图标
        /// </summary>
        public string icon { get; set; }
        /// <summary>
        /// 菜单路径
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 排序序号
        /// </summary>
        public int? order { get; set; }
        /// <summary>
        /// 上级菜单
        /// </summary>
        public long? fatherId { get; set; }
        /// <summary>
        /// 菜单角色
        /// </summary>
        public string roleId { get; set; }

        /// <summary>
        /// 是否启用（0否/1是）
        /// </summary>
        public int? visible { get; set; }
    }
}
