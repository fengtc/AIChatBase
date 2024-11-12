namespace AIChatBase.Data.Models.Customs
{
    public class WXToken
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
        /// token
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public int expires_in { get; set; }
    }
}
