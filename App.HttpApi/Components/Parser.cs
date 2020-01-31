using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace App.HttpApi
{
    /// <summary>
    /// 负责各种类型转换、列表类型转换
    /// ParseXXXX(string) 负责将字符串解析为对应的类型
    /// </summary>
    internal static class Parser
    {
        /// <summary>获取类型编码</summary>
        public static TypeCode GetTypeCode(this Type type)
        {
            return Type.GetTypeCode(type);
        }

        //--------------------------------------------------
        // 将文本解析为值或对象
        //--------------------------------------------------
        /// <summary>将文本解析为数字及衍生类型(枚举、布尔、日期）</summary>
        public static T ParseBasicType<T>(this string text) where T : struct
        {
            return (T)text.ParseBasicType(typeof(T));
        }
        /// <summary>将文本转化为数字及衍生类型(枚举、布尔、日期）</summary>
        /// <remarks>ParseBasicType, ParseSimpleType, ParseValue, ParseNumber</remarks>
        public static object ParseBasicType(this string text, Type type)
        {
            if (type == typeof(string))
                return text;

            if (type.IsNullable())
            {
                type = type.GetRealType();
                if (type == typeof(int))        return text.ParseInt();
                if (type == typeof(long))       return text.ParseLong();
                if (type == typeof(ulong))      return text.ParseULong();
                if (type == typeof(float))      return text.ParseFloat();
                if (type == typeof(double))     return text.ParseDouble();
                if (type == typeof(decimal))    return text.ParseDecimal();
                if (type == typeof(short))      return text.ParseShort();
                if (type == typeof(bool))       return text.ParseBool();
                if (type == typeof(DateTime))   return text.ParseDate();
                if (type.IsEnum())              return text.ParseEnum(type);
            }
            else
            {
                if (type == typeof(int))        return text.ParseInt().Value;
                if (type == typeof(long))       return text.ParseLong().Value;
                if (type == typeof(ulong))      return text.ParseULong().Value;
                if (type == typeof(float))      return text.ParseFloat().Value;
                if (type == typeof(double))     return text.ParseDouble().Value;
                if (type == typeof(decimal))    return text.ParseDecimal().Value;
                if (type == typeof(short))      return text.ParseShort().Value;
                if (type == typeof(bool))       return text.ParseBool().Value;
                if (type == typeof(DateTime))   return text.ParseDate().Value;
                if (type.IsEnum())              return text.ParseEnum(type);
            }
            return text;
        }


        /// <summary>Parse string to enum object</summary>
        /// <param name="text"></param>
        public static object ParseEnum(this string text, Type enumType)
        {
            try
            {
                return text.IsEmpty() ? null : Enum.Parse(enumType, text, true);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Parse string to enum? </summary>
        /// <param name="text">Enum text(name or value). Eg. "Male" or "0"</param>
        public static T? ParseEnum<T>(this string text) where T : struct
        {
            if (Enum.TryParse<T>(text, true, out T val))
                return val;
            return null;
        }


        /// <summary>解析枚举字符串列表（支持枚举名或值，如Male,Female 或 0,1）</summary>
        /// <param name="text">Enum texts, eg. "Male,Female" or "0,1"</param>
        public static List<T> ParseEnums<T>(this string text, char separator = ',') where T : struct
        {
            var enums = new List<T>();
            if (text.IsNotEmpty())
            {
                var items = text.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    var e = item.ParseEnum<T>();
                    if (e != null)
                        enums.Add(e.Value);
                }
            }
            return enums;
        }

        /// <summary>Parse string to DateTime?</summary>
        public static DateTime? ParseDate(this string text)
        {
            if (DateTime.TryParse(text, out DateTime val))
                return val;
            return null;
        }

        /// <summary>Parse string to decimal?</summary>
        public static decimal? ParseDecimal(this string text)
        {
            if (Decimal.TryParse(text, out Decimal val))
                return val;
            return null;
        }

        /// <summary>Parse string to double?</summary>
        public static double? ParseDouble(this string text)
        {
            if (Double.TryParse(text, out Double val))
                return val;
            return null;
        }

        /// <summary>Parse string to float?</summary>
        public static float? ParseFloat(this string text)
        {
            if (float.TryParse(text, out float val))
                return val;
            return null;
        }

        /// <summary>Parse string to int?</summary>
        public static int? ParseInt(this string text)
        {
            if (Int32.TryParse(text, out Int32 val))
                return val;
            return null;
        }

        /// <summary>Parse string to int64?</summary>
        public static long? ParseLong(this string text)
        {
            if (Int64.TryParse(text, out Int64 val))
                return val;
            return null;
        }

        /// <summary>Parse string to short?</summary>
        public static short? ParseShort(this string text)
        {
            if (Int16.TryParse(text, out Int16 val))
                return val;
            return null;
        }

        /// <summary>Parse string to ulong?</summary>
        public static ulong? ParseULong(this string text)
        {
            if (UInt64.TryParse(text, out UInt64 val))
                return val;
            return null;
        }

        /// <summary>Parse string to bool?</summary>
        public static bool? ParseBool(this string text)
        {
            if (bool.TryParse(text, out bool val))
                return val;
            return null;
        }

        /*
        /// <summary>Parse querystring to dict（eg. id=1&amp;name=Kevin）</summary>
        /// <param name="text">Querystring, eg. id=1&amp;name=Kevin</param>
        public static FreeDictionary<string, string> ParseDict(this string text)
        {
            var dict = new FreeDictionary<string, string>();
            var regex = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                var key = match.Result("$2");
                var value = match.Result("$3");
                dict.Add(key, value);
            }
            return dict;
        }
        */

    }
}