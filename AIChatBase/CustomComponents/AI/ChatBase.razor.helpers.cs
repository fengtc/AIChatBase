using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.ComponentModel;

namespace AIChatBase.CustomComponents.AI
{
    public partial class ChatBase
    {

        /// <summary>
        /// 时间辅助类
        /// </summary>
        public static class DateTimeHelper
        {

            /// <summary>
            /// 转化为UTC时间
            /// </summary>
            /// <param name="dateTime"></param>
            /// <returns></returns>
            public static DateTime GetDateTime(DateTime dateTime)
            {
                return dateTime.ToUniversalTime();
            }

            /// <summary>
            /// 从UTC转化本地时间(默认加8小时)
            /// </summary>
            /// <param name="utcDateTime"></param>
            /// <param name="format"></param>
            /// <returns></returns>
            public static string GetLocationDateTimeStr(IConfiguration config, DateTime? utcDateTime, string format = "yyyy-MM-dd HH:mm:ss")
            {
                return utcDateTime is null ? null : GetLocalDateTime(config, utcDateTime.Value).ToString(format);
            }

            public static DateTime GetLocalDateTime(IConfiguration configuration, DateTime utcDateTime)
            {
                return utcDateTime.ToLocalTime();
            }
        }

        /// <summary>
        /// 时间日期序列化
        /// </summary>
        public sealed class DateTimeConverter : IsoDateTimeConverter
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="format"></param>
            public DateTimeConverter(string format) : base()
            {
                base.DateTimeFormat = format;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// 大数据json序列化重写
        /// </summary>
        public sealed class NumberConverter : JsonConverter
        {
            /// <summary>
            /// 转换成字符串的类型
            /// </summary>
            private readonly NumberConverterShip _ship;

            /// <summary>
            /// 大数据json序列化重写实例化
            /// </summary>
            public NumberConverter()
            {
                _ship = (NumberConverterShip)0xFF;
            }

            /// <summary>
            /// 大数据json序列化重写实例化
            /// </summary>
            /// <param name="ship">转换成字符串的类型</param>
            public NumberConverter(NumberConverterShip ship)
            {
                _ship = ship;
            }

            /// <inheritdoc />
            /// <summary>
            /// 确定此实例是否可以转换指定的对象类型。
            /// </summary>
            /// <param name="objectType">对象的类型。</param>
            /// <returns>如果此实例可以转换指定的对象类型，则为：<c>true</c>，否则为：<c>false</c></returns>
            public override bool CanConvert(Type objectType)
            {
                var typecode = Type.GetTypeCode(objectType.Name.Equals("Nullable`1") ? objectType.GetGenericArguments().First() : objectType);
                switch (typecode)
                {
                    case TypeCode.Decimal:
                        return (_ship & NumberConverterShip.Decimal) == NumberConverterShip.Decimal;
                    case TypeCode.Double:
                        return (_ship & NumberConverterShip.Double) == NumberConverterShip.Double;
                    case TypeCode.Int64:
                        return (_ship & NumberConverterShip.Int64) == NumberConverterShip.Int64;
                    case TypeCode.UInt64:
                        return (_ship & NumberConverterShip.UInt64) == NumberConverterShip.UInt64;
                    case TypeCode.Single:
                        return (_ship & NumberConverterShip.Single) == NumberConverterShip.Single;
                    default: return false;
                }
            }

            /// <inheritdoc />
            /// <summary>
            /// 读取对象的JSON表示。
            /// </summary>
            /// <param name="reader">从 <see cref="T:Newtonsoft.Json.JsonReader" /> 中读取。</param>
            /// <param name="objectType">对象的类型。</param>
            /// <param name="existingValue">正在读取的对象的现有值。</param>
            /// <param name="serializer">调用的序列化器实例。</param>
            /// <returns>对象值。</returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return AsType(reader.Value != null ? reader.Value.ToString() : "", objectType);
            }

            /// <summary>
            /// 字符串格式数据转其他类型数据
            /// </summary>
            /// <param name="input">输入的字符串</param>
            /// <param name="destinationType">目标格式</param>
            /// <returns>转换结果</returns>
            public static object AsType(string input, Type destinationType)
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(destinationType);
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        return converter.ConvertFrom(null, null, input);
                    }

                    converter = TypeDescriptor.GetConverter(typeof(string));
                    if (converter.CanConvertTo(destinationType))
                    {
                        return converter.ConvertTo(null, null, input, destinationType);
                    }
                }
                catch
                {
                    return null;
                }
                return null;
            }

            /// <inheritdoc />
            /// <summary>
            /// 写入对象的JSON表示形式。
            /// </summary>
            /// <param name="writer">要写入的 <see cref="T:Newtonsoft.Json.JsonWriter" /> 。</param>
            /// <param name="value">要写入对象值</param>
            /// <param name="serializer">调用的序列化器实例。</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    var objectType = value.GetType();
                    var typeCode = Type.GetTypeCode(objectType.Name.Equals("Nullable`1") ? objectType.GetGenericArguments().First() : objectType);
                    switch (typeCode)
                    {
                        case TypeCode.Decimal:
                            writer.WriteValue(((decimal)value).ToString("f6"));
                            break;
                        case TypeCode.Double:
                            writer.WriteValue(((double)value).ToString("f4"));
                            break;
                        case TypeCode.Single:
                            writer.WriteValue(((float)value).ToString("f2"));
                            break;
                        default:
                            writer.WriteValue(value.ToString());
                            break;
                    }
                }
            }
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

        public static class StrSubstitution
        {
            public static string FormatPhone(string value)
            {
                if (string.IsNullOrEmpty(value))
                { return value; }
                else
                {
                    //if (value.Length < 6)
                    //{
                    //    return value.Substring(0, value.Length - 3) + "***";
                    //}
                    //else
                    //{
                    //    return value.Substring(0, value.Length - 5) + "*****";
                    //}
                    if (value.Length > 6)
                    {
                        return value.Substring(0, value.Length - 5) + "*****";
                    }
                    else
                    {
                        return value;
                    }
                }

            }
            public static string FormatEmail(string value)
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var tagIndex = value.IndexOf("@");
                        if (tagIndex == -1)
                        {
                            return FormatPhone(value);
                        }
                        else
                        {
                            if (value.Contains("@paratera.com") || value.Contains("@blsc.cn"))
                            {
                                return value;
                            }
                            else
                            {
                                return value.Substring(0, tagIndex - 1) + "**@*******";
                            }

                        }
                    }
                    else
                    {
                        return value;
                    }
                }
                catch (Exception ex)
                {
                    return "";
                }
            }

            public static string FormatAmount(object value)
            {
                if (value != null && string.IsNullOrEmpty(value.ToString()))
                    return "0";
                else
                    return Convert.ToDecimal(value).ToString("#,##0.00");
            }
            /// <summary>
            /// 写个校验是否手机号码的方法
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static bool IsMobilePhone(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
                else
                {
                    return System.Text.RegularExpressions.Regex.IsMatch(value, @"^1[3-9]\d{9}$");
                }
            }
        }
    }
}
