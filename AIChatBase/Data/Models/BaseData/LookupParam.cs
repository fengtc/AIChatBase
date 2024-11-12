namespace AIChatBase.Data.Models.BaseData;

public class LookupParam
{
    public string label { get; set; }

    public object value { get; set; }
}

public class LookupTypeParam<T>
{
    public string label { get; set; }

    public T value { get; set; }
}