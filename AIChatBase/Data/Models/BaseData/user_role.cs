namespace AIChatBase.Data.Models.BaseData;

public class user_role
{
    public string userid { get; set; }
    public string roleid { get; set; }
    public string role_name { get; set; }
    public List<user_claim> user_role_claims { get; set; }
}
