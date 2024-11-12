namespace AIChatBase.Data.Models.BaseData
{
    public class gridListResponse<T>
    {
        /// <summary>
        /// list数据
        /// </summary>
        public List<T> data { get; set; }
        /// <summary>
        /// 总条数
        /// </summary>
        public int totalCount { get; set; }

        public int groupCount { get; set; }

        public string summary { get; set; }
        public int currentPage { get; set; }

        /// <summary>
        /// list数据1
        /// </summary>
        public List<T> detail { get; set; }
    }
}
