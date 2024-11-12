using System.Collections.Generic;

namespace AIChatBase.Data.Models.Customs
{
    /// <summary>
    /// 数据库提交事务
    /// </summary>
    public class TransicationDataInput
    {
        public List<TransicationDataDetail> TransicationDataDetails { get; set; }
    }

    /// <summary>
    /// 提交数据详情
    /// </summary>
    public class TransicationDataDetail
    {
        /// <summary>
        /// 数据库表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 数据库数据
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 是否发送消息(默认发送)
        /// </summary>
        public bool IsSend { get; set; } = true;
        /// <summary>
        /// 数据库提交类型
        /// </summary>
        public string AddOrUpdateOrDelete { get; set; }
    }
}
