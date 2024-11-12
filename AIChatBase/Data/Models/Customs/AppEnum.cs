using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatBase.Data.Models.Customs
{
    public class AppEnum
    {
    }

    /// <summary>
    /// 用户访问类型
    /// </summary>
    public enum UserAgentEnum
    {
        /// <summary>
        /// 手机端企业微信
        /// </summary>
        COM_WX_MOBILE = 0,
        /// <summary>
        /// PC端企业微信
        /// </summary>
        COM_WX_PC = 1,
        /// <summary>
        /// 手机端微信
        /// </summary>
        WX_MOBILE = 2,
        /// <summary>
        /// PC端微信
        /// </summary>
        WX_PC = 3,
        /// <summary>
        /// 其他
        /// </summary>
        OTHER = 4
    }

    /// <summary>
    /// 转换成字符串的类型
    /// </summary>
    [Flags]
    public enum NumberConverterShip
    {
        /// <summary>
        /// 长整数
        /// </summary>
        Int64 = 1,

        /// <summary>
        /// 无符号长整数
        /// </summary>
        UInt64 = 2,

        /// <summary>
        /// 浮点数
        /// </summary>
        Single = 4,

        /// <summary>
        /// 双精度浮点数
        /// </summary>
        Double = 8,

        /// <summary>
        /// 大数字
        /// </summary>
        Decimal = 16
    }
}
