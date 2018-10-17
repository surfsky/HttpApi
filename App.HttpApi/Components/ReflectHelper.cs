using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Web;
using System.ComponentModel;

namespace App.HttpApi
{
    /// <summary>
    /// 反射相关辅助方法
    /// </summary>
    internal class ReflectHelper
    {
        // 获取DescriptionAttribute
        public static string GetDescription(Type type)
        {
            var objs = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length > 0)
                return (objs[0] as DescriptionAttribute).Description;
            else
                return type.FullName;
        }

        // 获取历史信息特性元数据
        public static object[] GetHistories(Type type)
        {
            var objs = type.GetCustomAttributes(typeof(HistoryAttribute), false);
            return objs;
        }

        // 获取类型文件的缓存时间
        public static int GetCacheDuration(Type type)
        {
            ScriptAttribute attr = ReflectHelper.GetScriptAttribute(type);
            return (attr != null) ? attr.CacheDuration : 0;
        }

        //-------------------------------------------------
        // 反射、方法调用相关
        //-------------------------------------------------
        // 获取WebMethodNamespaceAttribute
        public static ScriptAttribute GetScriptAttribute(Type type)
        {
            return GetAttribute<ScriptAttribute>(type);
        }


        /// <summary>取得HttpApiAttribute</summary>
        public static HttpApiAttribute GetHttpApiAttribute(MethodInfo info)
        {
            if (info == null) return null;
            HttpApiAttribute attr = GetAttribute<HttpApiAttribute>(info);
            return attr;
        }

        // 获取类别特性
        public static T GetAttribute<T>(Type type) where T : Attribute
        {
            T[] arr = (T[])type.GetCustomAttributes(typeof(T), true);
            return (arr.Length == 0) ? null : arr[0];
        }

        // 获取方法特性
        public static T GetAttribute<T>(MethodInfo info) where T : Attribute
        {
            T[] arr = (T[])info.GetCustomAttributes(typeof(T), true);
            return arr.Length > 0 ? arr[0] : null;
        }

        // 获取方法
        public static MethodInfo GetMethod(Type type, string methodName)
        {
            MethodInfo info = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);
            return info;
        }

        // 调用方法
        public static object InvokeMethod(object obj, MethodInfo info, Dictionary<string, object> args)
        {
            // 获取需要的参数
            object[] parameters = GetParameters(info, args);

            // 用Invoke调用方法
            return info.Invoke(obj, parameters);
        }


        /// <summary>
        /// 构造匹配方法的参数值列表（若方法名重名怎么处理？）
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="args">参数名-值字典</param>
        /// <returns>排序后的参数值数组</returns>
        public static object[] GetParameters(MethodInfo info, Dictionary<string, object> args)
        {
            List<object> array = new List<object>();
            if (info != null)
            {
                ParameterInfo[] pis = info.GetParameters();
                foreach (ParameterInfo pi in pis)
                {
                    // 如果输入参数不足，尝试取方法的默认参数
                    if (!args.Keys.Contains(pi.Name))
                    {
                        if (pi.DefaultValue != null)
                            array.Add(pi.DefaultValue);
                        continue;
                    }

                    // 找到匹配的输入参数
                    object obj = args[pi.Name];
                    object obj2 = null;

                    // 字典转化为对象
                    if (obj is Dictionary<string, object>)
                        obj2 = DicToObj(obj as Dictionary<string, object>, pi.ParameterType);

                    // 其它类型转换（先尝试用简单的Convert转换，若不行再用json转换）
                    else if (null != obj)
                    {
                        try
                        {
                            obj2 = Convert.ChangeType(obj, pi.ParameterType);
                        }
                        catch
                        {
                            obj2 = Newtonsoft.Json.JsonConvert.DeserializeObject(obj.ToString(), pi.ParameterType);
                        }
                    }

                    //
                    array.Add(obj2);
                }
            }
            return array.ToArray();
        }

        // 将字典转化为指定对象
        public static object DicToObj(Dictionary<string, object> dic, Type type)
        {
            object o = type.Assembly.CreateInstance(type.FullName);
            foreach (PropertyInfo p in type.GetProperties())
            {
                string name = p.Name;
                if (dic.ContainsKey(name))
                {
                    Type propertyType = p.PropertyType;
                    object propertyValue = dic[name];
                    if (propertyValue is Dictionary<string, object>)
                        propertyValue = DicToObj(propertyValue as Dictionary<string, object>, propertyType);
                    else
                        propertyValue = ToBasicObject(propertyValue, propertyType);
                    p.SetValue(o, propertyValue, null);
                }
            }
            return o;
        }

        public static object ToBasicObject(object o, Type type)
        {
            if (type.IsSubclassOf(typeof(Enum)))
                return Enum.Parse(type, o.ToString());

            switch (type.FullName)
            {
                case "System.DateTime":
                    return Convert.ToDateTime(o);
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return Convert.ToInt64(o);
                case "System.Boolean":
                    return Convert.ToBoolean(o);
                case "System.Char":
                    return Convert.ToChar(o);
                case "System.Decimal":
                case "System.Double":
                case "System.Single":
                    return Convert.ToDouble(o);
                default:
                    return o;
            }
        }


    }
}
