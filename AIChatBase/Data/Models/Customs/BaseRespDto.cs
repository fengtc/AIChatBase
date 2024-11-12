namespace AIChatBase.Data.Models.Customs;

/// <summary>
/// 返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseRespDto<T>
{
    public int errcode { get; set; }

    public string errmsg { get; set; }

    public T data { get; set; }
}