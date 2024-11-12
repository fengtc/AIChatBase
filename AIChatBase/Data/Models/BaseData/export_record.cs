using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatBase.Data.Models.BaseData
{
    /// <summary>
    /// 导出记录
    /// </summary>
    public class export_record
    {
        /// <summary>
        /// ID
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string start_member_id { get; set; }
        /// <summary>
        /// 创建人中文
        /// </summary>
        public string start_member_name { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? start_date { get; set; }
        /// <summary>
        /// 服务名称，命名规则：finance-web（财务管理）
        /// </summary>
        public string service_name { get; set; }
        /// <summary>
        /// 模块名称，如：财务管理-销售合同信息单
        /// </summary>
        public string module_name { get; set; }
        /// <summary>
        /// 过滤条件，存储dev filter中的条件
        /// </summary>
        public string export_filter { get; set; }
        /// <summary>
        ///  导出文件名，如：费用包导入模块数据明细.xlsx
        /// </summary>
        public string excel_name { get; set; }
        /// <summary>
        /// 导出条数，如果是所有则写“所有（xx条），部分（xx条）
        /// </summary>
        public string export_count { get; set; }
        /// <summary>
        /// 导出条数
        /// </summary>
        public int record_count { get; set; }
        /// <summary>
        /// 备注，当前导出操作的一些描述
        /// </summary>
        public string remarks { get; set; }
    }
}
