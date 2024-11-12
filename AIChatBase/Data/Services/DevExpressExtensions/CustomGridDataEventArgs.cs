using System.Collections;

namespace AIChatBase.Data.Services.DevExpressExtensions
{
    /// <summary>
    /// 派生自EventArgs的类，用于传递数据
    /// </summary>
    public class CustomGridDataEventArgs : EventArgs
    {
        public IList Data { get; set; }                    //用于存储数据，当事件被调用时，可利用其进行传递数据。
    }
}
