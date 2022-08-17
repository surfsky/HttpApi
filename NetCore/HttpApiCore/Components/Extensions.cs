using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.HttpApi
{
    /// <summary>
    /// 一些常用的扩展
    /// </summary>
    internal static class Extensions
    {
        /// <summary>字符串是否为空</summary>
        public static bool IsEmpty(this string txt)
        {
            return String.IsNullOrEmpty(txt);
        }

        /// <summary>对象是否为空或为空字符串</summary>
        public static bool IsEmpty(this object o)
        {
            return (o == null) ? true : o.ToString().IsEmpty();
        }


        /// <summary>字符串是否为空</summary>
        public static bool IsNotEmpty(this string txt)
        {
            return !String.IsNullOrEmpty(txt);
        }


        /// <summary>将可空对象转化为字符串</summary>
        public static string ToText(this object o, string format = "{0}")
        {
            //return o == null ? "" : o.ToString();
            return string.Format(format, o);
        }

        /// <summary>将可空bool对象转化为字符串</summary>
        public static string ToText(this bool? o, string trueText = "true", string falseText = "false")
        {
            return o == null
                ? ""
                : (o.Value ? trueText : falseText)
                ;
        }

        /// <summary>转化为小写字符串列表</summary>
        public static List<string> ToLower(this IEnumerable source)
        {
            var result = new List<string>();
            foreach (var item in source)
                result.Add(item.ToString().ToLower());
            return result;
        }
    }
}
