using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace App.HttpApi
{
    /// <summary>XML序列化异常</summary>
    internal class XmlSerializerException : Exception
    {
        public XmlSerializerException() { }
        public XmlSerializerException(string message) : base(message){}
        public XmlSerializerException(string message, Exception innerException) : base(message, innerException){}
    }

    /// <summary>元素类型</summary>
    internal enum ElementType
    {
        Object, Array, Generic
    }

    /// <summary>
    /// Xml序列及反序列化操作（以后请改用AppPlat.Core.Xmlizer）
    /// History:
    ///     2017-04-13 | Created  | Jackie Lee（天宇遊龍）http://www.cnblogs.com/dralee
    ///     2019-10-22 | 改为非泛型版本，更为通用，很多情况我们并不知道要序列化的对象的类型 | surfsky.cnblogs.com
    /// Todo:
    ///     检测和避免无限循环引用
    ///     更精确的控制需要构建一个 XmlDocument 对象，最后再根据格式参数生成 xml 文本
    /// </summary>
    internal class XmlSerializer
    {
        public bool FormatLowCamel { get; set; } = false;
        public EnumFomatting FormatEnum { get; set; } = EnumFomatting.Text;
        public string FormatDateTime { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public bool FormatIndent { get; set; } = false;

        //-------------------------------------------------
        // 构造析构
        //-------------------------------------------------
        /// <summary>Xml序列化</summary>
        /// <param name="xmlHead">XML文件头<?xml ... ?></param>
        /// <param name="useCData">是否需要CDATA包裹数据</param>
        public XmlSerializer(bool formatLowCamel=false, EnumFomatting formatEnum=EnumFomatting.Text, string formatDateTime="yyyy-MM-dd HH:mm:ss", bool formatIndent=false)
        {
            this.FormatLowCamel = formatLowCamel;
            this.FormatEnum = formatEnum;
            this.FormatDateTime = formatDateTime;
            this.FormatIndent = formatIndent;
        }

        


        //-------------------------------------------------
        // 对象转 XML
        //-------------------------------------------------
        /// <summary>序列化报文为xml</summary>
        public string ToXml(object obj, Type type=null, string xmlHead = "<?xml version=\"1.0\" encoding=\"utf-8\"?>")
        {
            var sb = new StringBuilder();
            var rootName = GetNodeName(type ?? obj.GetType());
            if (!string.IsNullOrEmpty(xmlHead))
                sb.AppendFormat("{0}\r\n", xmlHead);
            VisitObject(sb, obj, rootName);
            return sb.ToString();
        }


        /// <summary>访问对象</summary>
        private void VisitObject(StringBuilder sb, object obj, string name)
        {
            if (this.FormatLowCamel)
                name = name?.ToLowCamel();

            // 对象为空处理
            if (obj == null)
            {
                if (!name.IsEmpty())
                    sb.AppendFormat("<{0}/>", name);
                return;
            }

            // 正常处理
            var type = obj.GetType();
            if (type.IsNullable())
                type = type.GetNullableDataType();
            if (name.IsEmpty())
                name = GetNodeName(type);

            // 根据类型进行输出（还要判断可空类型）
            sb.AppendFormat("<{0}>", name);
            if (obj is string)
            {
                sb.Append(GetXmlSafeText(obj));
            }
            else if (obj is DateTime)
            {
                var dt = Convert.ToDateTime(obj);
                if (dt != new DateTime())
                    sb.AppendFormat(dt.ToString(this.FormatDateTime));
            }
            else if (type.IsEnum)
            {
                if (this.FormatEnum == EnumFomatting.Int)  sb.AppendFormat("{0:d}", obj);
                else                                       sb.AppendFormat("{0}", obj);
            }
            else if (obj is DataTable)
            {
                var table = obj as DataTable;
                var cols = table.Columns;
                foreach (DataRow row in table.Rows)
                {
                    sb.AppendFormat("<Row>");
                    foreach (DataColumn col in cols)
                    {
                        var columnName = col.ColumnName;
                        VisitObject(sb, row[columnName], columnName);
                    }
                    sb.AppendFormat("</Row>");
                }
            }
            else if (obj is IDictionary)
            {
                var dict = (obj as IDictionary);
                foreach (var key in dict.Keys)
                {
                    sb.AppendFormat("<Item Key=\"{0}\">", key);
                    VisitObject(sb, dict[key], "");
                    sb.AppendFormat("</Item>");
                }
            }
            else if (obj is IEnumerable)
            {
                foreach (var item in (obj as IEnumerable))
                    VisitObject(sb, item, "");
            }
            else if (type.IsValueType)
            {
                sb.AppendFormat("{0}", obj);
            }
            else
            {
                var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    if (ReflectHelper.GetAttribute<NonSerializedAttribute>(property) != null
                        || ReflectHelper.GetAttribute<JsonIgnoreAttribute>(property) != null
                        || ReflectHelper.GetAttribute<System.Xml.Serialization.XmlIgnoreAttribute>(property) != null
                        )
                        continue;

                    var subObj = property.GetValue(obj);
                    VisitObject(sb, subObj, property.Name);
                }
            }
            sb.AppendFormat("</{0}>", name);
        }


        /// <summary>获取节点名</summary>
        string GetNodeName(Type type)
        {
            if (type.Name.Contains("AnonymousType"))
                return "Item";
            if (type.GetInterface("IDictionary") != null)
                return "Dictionary";
            if (type.IsGenericType)
                return GetNodeName(type.GetGenericDataType()) + "s";
            if (type.IsArray)
                return GetNodeName(type.GetElementType()) + "s";
            return type.Name;
        }

        /// <summary>获取Xml安全文本</summary>
        static string GetXmlSafeText(object obj, bool useCDATA=true)
        {
            // "<" 字符和"&"字符对于XML来说是严格禁止使用的，可用转义符或CDATA解决
            var txt = obj.ToString();
            if (txt.IndexOfAny(new char[] { '<', '&' }) != -1)
            {
                if (useCDATA)
                    return string.Format("<![CDATA[ {0} ]]>", txt);
                else
                    return txt.Replace("<", "").Replace("&", "");
            }
            return txt;
        }
    }
}
