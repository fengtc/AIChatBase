namespace AIChatBase.Data.Models.Customs;

public class IdentifyResponse<T>
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int errcode { get; set; }
    /// <summary>
    /// 错误信息
    /// </summary>
    public string errmsg { get; set; }

    /// <summary>
    /// data
    /// </summary>
    public T data { get; set; }
}
