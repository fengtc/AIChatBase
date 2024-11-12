using DevExtreme.AspNet.Data;

namespace AIChatBase.Data.Models.BaseData
{
    public class LoadDataRequest
    {
        public string ContextName { get; set; }
        public string EntityName { get; set; }
        public string LoadOptions { get; set; }
        public string[] Includes { get; set; }
    }
}
