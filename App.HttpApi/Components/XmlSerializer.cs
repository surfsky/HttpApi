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
    public class XmlSerializerException : Exception
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
    /// Xml序列及反序列化操作
    /// History:
    ///     2017-04-13 | Created  | Jackie Lee（天宇遊龍）http://www.cnblogs.com/dralee
    ///     2019-10-22 | 改为非泛型版本，更为通用，很多情况我们并不知道要序列化的对象的类型 | surfsky.cnblogs.com
    /// Todo:
    ///     反序列化未测试未优化
    ///     检测和避免无限循环引用
    ///     更精确的控制需要构建一个 XmlDocument 对象，最后再根据格式参数生成 xml 文本
    /// </summary>
    public class XmlSerializer
    {
        private string _rootTag;
        private ElementType _elemType;
        private Type _elementType;
        private bool _useCData = true;


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
                if (!name.IsNullOrEmpty())
                    sb.AppendFormat("<{0}/>", name);
                return;
            }

            // 正常处理
            var type = obj.GetType();
            if (type.IsNullable())
                type = type.GetNullableDataType();
            if (name.IsNullOrEmpty())
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


        //-------------------------------------------------
        // XML转对象
        //-------------------------------------------------
        /// <summary>准备解析引擎</summary>
        private void PrepareEngine(Type type)
        {
            if (type.Name.Contains("AnonymousType"))
            {
                _elemType = ElementType.Generic;
                _elementType = type.GenericTypeArguments.FirstOrDefault();
                _rootTag = "AnonymousType";
            }
            else if (type.IsGenericType)
            {
                _elemType = ElementType.Generic;
                _elementType = type.GenericTypeArguments.FirstOrDefault();
                _rootTag = _elementType.Name + "s";
            }
            else if (type.IsType(typeof(IDictionary)))
            {
                _elemType = ElementType.Array;
                _elementType = type.GetTypeInfo().GetElementType();
                _rootTag = "Dictionary";
            }
            else if (type.IsArray)
            {
                _elemType = ElementType.Array;
                _elementType = type.GetTypeInfo().GetElementType();
                _rootTag = _elementType.Name + "s";
            }
            else
            {
                _rootTag = type.Name;
            }
        }

        /// <summary>Xml字符串序列化为对象</summary>
        public T FromXml<T>(string xml)
        {
            int index;
            if (xml.Trim().StartsWith("<?xml") && (index = xml.IndexOf("?>")) != -1)
            {
                xml = xml.Substring(index + 2).Trim('\r', '\n', ' ');
            }
            try
            {
                PrepareEngine(typeof(T));
                switch (_elemType)
                {
                    case ElementType.Generic:
                        return VisitXmlGeneric<T>(xml);
                    case ElementType.Array:
                        return VisitXmlArray<T>(xml);
                    default:
                        return VisitXmlObject<T>(xml);
                }
            }
            catch (Exception ex)
            {
                throw new XmlSerializerException($"反序列化对象信息异常:{ex.Message}", ex);
            }
        }

        /// <summary>访问xml中对象集合</summary>
        private T VisitXmlGeneric<T>(string xml)
        {
            T collection = Activator.CreateInstance<T>();
            List<string> xmlArr = GetTagContents(xml, _rootTag, "", _useCData);
            foreach (var itemXml in xmlArr)
                AddElement(collection, itemXml, obj => {Add(collection, obj);});
            return collection;
        }

        /// <summary>访问xml中对象集合</summary>
        private T VisitXmlArray<T>(string xml)
        {
            List<string> xmlArr = GetTagContents(xml, _rootTag, "", _useCData);
            Array array = Array.CreateInstance(_elementType, xmlArr.Count);
            T collection = (T)Convert.ChangeType(array, typeof(T));
            int index = 0;
            foreach (var itemXml in xmlArr)
            {
                AddElement(collection, itemXml, obj =>
                {
                    SetValue(collection, obj, index++);
                });
            }
            return collection;
        }

        /// <summary>添加元素到集合</summary>
        /// <param name="collection">集合</param>
        /// <param name="itemXml">元素xml</param>
        /// <param name="addItem">集合项添加操作</param>
        private void AddElement<T>(T collection, string itemXml, Action<object> addItem)
        {
            var obj = Activator.CreateInstance(_elementType);
            VisitXml($"<{_rootTag}>{itemXml}</{_rootTag}>", obj, _elementType.GetProperties(BindingFlags.Instance | BindingFlags.Public));
            addItem(obj);
        }

        /// <summary>访问xml对象</summary>
        private T VisitXmlObject<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml) || !xml.StartsWith($"<{_rootTag}>"))
            {
                throw new XmlSerializerException($"反序列化对象信息异常:指定xml内容与指定对象类型{typeof(T)}不匹配");
            }
            T packet = Activator.CreateInstance<T>();
            VisitXml(xml, packet, typeof(T).GetProperties());
            return packet;
        }

        /// <summary>添加元素到集合中</summary>
        private void Add<T>(T collection, object obj)
        {
            var methodInfo = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("Add"));
            if (methodInfo == null)
                throw new XmlSerializerException($"反序列化集合xml内容失败，目标{typeof(T).FullName}非集合类型");

            var instance = Expression.Constant(collection);
            var param = Expression.Constant(obj);
            var addExpression = Expression.Call(instance, methodInfo, param);
            var add = Expression.Lambda<Action>(addExpression).Compile();
            add.Invoke();
        }

        /// <summary>添加元素到集合中</summary>
        private void SetValue<T>(T collection, object obj, int index)
        {
            var methodInfo = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("SetValue"));
            if (methodInfo == null)
                throw new XmlSerializerException($"反序列化集合xml内容失败，目标{typeof(T).FullName}非集合类型");

            var instance = Expression.Constant(collection);
            var param1 = Expression.Constant(obj);
            var param2 = Expression.Constant(index);
            var addExpression = Expression.Call(instance, methodInfo, param1, param2);
            var setValue = Expression.Lambda<Action>(addExpression).Compile();
            setValue.Invoke();
        }

        /// <summary>对象序列化为xml</summary>
        private void VisitXml(string xml, object obj, PropertyInfo[] fields)
        {
            foreach (var field in fields)
            {
                Type subType = field.PropertyType;
                if (!subType.FullName.StartsWith("System.") && !IsEnumType(subType))
                {
                    object subObj = Activator.CreateInstance(subType);// field.GetValue(obj);
                    var subFields = subType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    field.SetValue(obj, subObj);
                    if (subFields.Count() > 0)
                        VisitXml(xml, subObj, subFields);
                    else
                        field.SetValue(subObj, GetTagContent(xml, field.Name.ToLowCamel(), "", _useCData));
                }
                else
                {
                    var value = GetTagContent(xml, field.Name.ToLowCamel(), "", _useCData);
                    if (subType != typeof(string))
                    {
                        if (IsEnumType(subType))
                            field.SetValue(obj, Enum.Parse(subType, value));
                        else
                            field.SetValue(obj, Convert.ChangeType(value, subType));
                    }
                    else
                    {
                        field.SetValue(obj, value);
                    }
                }
            }
        }

        //-------------------------------------------
        // 辅助方法
        //-------------------------------------------
        /// <summary>是否为枚举类型</summary>
        private bool IsEnumType(Type type)
        {
            return type.IsEnum;
        }

        /// <summary>获取字符中指定标签的值</summary>  
        /// <param name="content">字符串</param>  
        /// <param name="tagName">标签</param>  
        /// <param name="attrib">属性名</param>  
        /// <returns>属性</returns>  
        public static string GetTagContent(string content, string tagName, string attrib, bool needCData)
        {
            string valueStr = needCData ? "<!\\[CDATA\\[(.*)\\]\\]>" : "([\\s\\S]*?)";
            string tmpStr = string.IsNullOrEmpty(attrib) ? $"<{tagName}>{valueStr}</{tagName}>" :
                $"<{tagName}\\s*{attrib}\\s*=\\s*.*?>{valueStr}</{tagName}>";
            Match match = Regex.Match(content, tmpStr, RegexOptions.IgnoreCase);

            string result = match.Groups[1].Value;
            //Match math = Regex.Match(result, @"\<\!\[CDATA\[(?<([\s\S]*?)>[^\]]*)\]\]\>", RegexOptions.IgnoreCase);
            return result;
        }

        /// <summary>获取字符中指定标签的值</summary>  
        /// <param name="content">字符串</param>  
        /// <param name="tagName">标签</param>  
        /// <param name="attrib">属性名</param>  
        /// <returns>属性</returns>  
        public static List<string> GetTagContents(string content, string tagName, string attrib, bool needCData)
        {
            string valueStr = needCData ? "<!\\[CDATA\\[(.*)\\]\\]>" : "([\\s\\S]*?)";
            string tmpStr = string.IsNullOrEmpty(attrib) ? $"<{tagName}>{valueStr}</{tagName}>" :
                $"<{tagName}\\s*{attrib}\\s*=\\s*.*?>{valueStr}</{tagName}>";
            MatchCollection matchs = Regex.Matches(content, tmpStr, RegexOptions.IgnoreCase);

            var result = new List<string>();
            foreach (Match match in matchs)
                result.Add(match.Groups[1].Value);
            return result;
        }
    }
}
