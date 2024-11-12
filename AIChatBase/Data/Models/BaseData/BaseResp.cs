namespace AIChatBase.Data.Models.BaseData;

public class BaseResp<T>
{
    public string? msg { get; set; }

    public int code { get; set; }

    public T? data { get; set; }
}