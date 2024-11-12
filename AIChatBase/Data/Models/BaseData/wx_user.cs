namespace AIChatBase.Data.Models.BaseData;

/// <summary>
/// 微信用户
/// </summary>
public class wx_user
{
    public string? userid { get; set; }
    public string name { get; set; }
    public string position { get; set; }
    public string mobile { get; set; }
    public string gender { get; set; }
    public string email { get; set; }
    public string avatar { get; set; }
    public int status { get; set; }
    public int enable { get; set; }
    public int isleader { get; set; }
    public int hide_mobile { get; set; }
    public string english_name { get; set; }
    public string telephone { get; set; }
    public int main_department { get; set; }
    public string qr_code { get; set; }
    public string alias { get; set; }
    public string address { get; set; }
    public string thumb_avatar { get; set; }
    public string oss_userid { get; set; }
    public long? oa_memberid { get; set; }
    public List<user_role> user_roles { get; set; }
    public List<user_claim> user_claims { get; set; }
}
