namespace AIChatBase.Data.Models.Customs;

/// <summary>
/// dev返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class DevResponseDto<T>
{
    public List<T> data { get; set; }

    public int totalCount { get; set; }

    public int groupCount { get; set; }

    public List<int> summary { get; set; }
}

public class DevSummaryResponseDto<T>
{
    public List<DevSummaryResponseItem<T>> data { get; set; }

    public int totalCount { get; set; }

    public int groupCount { get; set; }
}

public class DevSummaryResponseItem<T>
{
    public string key { get; set; }

    public List<T>? items { get; set; }

    public int count { get; set; }

    public List<decimal> summary { get; set; }
}