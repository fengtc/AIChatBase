using System.ComponentModel;

namespace AIChatBase.Data.Models.BaseData;

public class SaleGroupInfo : BaseModel
{
    [DisplayName("销售名称")]
    public string SaleName { get; set; }
    [DisplayName("销售memberId")]
    public string SaleMemberId { get; set; }
    [DisplayName("销售微信Id")]
    public string SaleWxWorkId { get; set; }
    [DisplayName("销售邮箱")]
    public string SaleEmail { get; set; }
    [DisplayName("销售组别")]
    public string SaleDepartment { get; set; }
    [DisplayName("销售团队")]
    public string SaleTeamName { get; set; }

}