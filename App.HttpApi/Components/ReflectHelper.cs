using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Web;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
//using App.Core;

namespace App.HttpApi
{
    /// <summary>
    /// 反射相关辅助方法
    /// </summary>
    internal static class ReflectHelper
    {
        //------------------------------------------------
        // 数据集相关
        //------------------------------------------------
        // 获取数据集版本号
        public static Version AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public static string AssemblyPath
        {
            get { return Assembly.GetExecutingAssembly().Location; }
        }

        public static string AssemblyDirectory
        {
            get { return new FileInfo(AssemblyPath).DirectoryName; }
        }

        //------------------------------------------------
        // 类型相关
        //------------------------------------------------
        /// <summary>是否是某个类型（或子类型）</summary>
        public static bool IsType(this Type raw, Type match)
        {
            return (raw == match) ? true : raw.IsSubclassOf(match);
        }

        /// <summary>是否属于某个类型</summary>
        public static bool IsType(this Type type, string typeName)
        {
            if (type.ToString() == typeName)
                return true;
            if (type.ToString() == "System.Object")
                return false;
            return IsType(type.BaseType, typeName);
        }


        /// <summary>是否是泛型类型</summary>
        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        /// <summary>是否是可空类型</summary>
        public static bool IsNullable(this Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>获取可空类型中的值类型</summary>
        public static Type GetNullableDataType(this Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                return type.GetGenericArguments()[0];
            return type;
        }

        /// <summary>获取泛型中的数据类型</summary>
        public static Type GetGenericDataType(this Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericArguments()[0];
            return type;
        }


        //-------------------------------------------------
        // 获取类型信息
        //-------------------------------------------------
        /// <summary>获取（可空类型的）真实类型</summary>
        public static Type GetRealType(this Type type)
        {
            if (type.IsNullable())
                return GetRealType(type.GetNullableDataType());
            return type;
        }

        /// <summary>获取类型字符串（可处理可空类型）</summary>
        public static string GetTypeString(this Type type, bool shortName = true)
        {
            if (type.IsNullable())
            {
                type = type.GetNullableDataType();
                return GetTypeString(type) + "?";
            }
            if (type.IsValueType)
                return type.Name.ToString();
            return shortName ? type.Name.ToString() : type.FullName.ToString();
        }

        /// <summary>获取类型的概述信息（可解析枚举类型）</summary>
        public static string GetTypeSummary(this Type type)
        {
            if (type.IsNullable())
                type = type.GetNullableDataType();

            var sb = new StringBuilder();
            if (type.IsEnum)
            {
                foreach (var item in Enum.GetValues(type))
                    sb.AppendFormat("{0}-{1}({2}); ", (int)item, item.ToString(), item.GetDescription());
            }
            return sb.ToString();
        }

        /// <summary>获取枚举值的文本说明。RoleType.Admin.GetDescription()</summary>
        public static string GetDescription(this object enumValue)
        {
            if (enumValue == null)
                return "";
            MemberInfo info = GetEnumField(enumValue);
            return GetDescription(info);
        }

        /// <summary>获取字段说明（来自 AppCore.UIAttribute 或 DescriptionAttribute）</summary>
        /// <remarks>此处用反射的方式获取属性值，以废除对 App.Core 的依赖</remarks>
        static string GetDescription(this MemberInfo info)
        {
            if (info != null)
            {
                foreach (Attribute attr in info.GetCustomAttributes())
                {
                    if (attr.GetType().Name == "UIAttribute")
                        return attr.GetPropertyValue("Title") as string;
                    if (attr is DescriptionAttribute)
                        return attr.GetPropertyValue("Description") as string;
                }
                return info.Name;
            }
            return "";
        }

        /// <summary>获取对象的属性值。也可考虑用dynamic实现。</summary>
        /// <param name="propertyName">属性名。可考虑用nameof()表达式来实现强类型。</param>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            Type type = obj.GetType();
            PropertyInfo pi = type.GetProperty(propertyName);
            return pi.GetValue(obj);
        }

        /// <summary>获取枚举值对应的字段</summary>
        static FieldInfo GetEnumField(this object enumValue)
        {
            if (enumValue == null) return null;
            var enumType = enumValue.GetType();
            return enumType.GetField(enumValue.ToString());
        }

        //-------------------------------------------------
        // 获取类别属性
        //-------------------------------------------------
        // 获取类别特性
        public static T GetAttribute<T>(Type type) where T : Attribute
        {
            T[] arr = (T[])type.GetCustomAttributes(typeof(T), true);
            return (arr.Length == 0) ? null : arr[0];
        }

        // 获取成员特性
        public static T GetAttribute<T>(PropertyInfo property) where T : Attribute
        {
            T[] arr = (T[])property.GetCustomAttributes(typeof(T), true);
            return (arr.Length == 0) ? null : arr[0];
        }


        // 获取方法特性
        public static T GetAttribute<T>(MethodInfo info) where T : Attribute
        {
            T[] arr = (T[])info.GetCustomAttributes(typeof(T), true);
            return arr.Length > 0 ? arr[0] : null;
        }

        //-------------------------------------------------
        // Attribute 相关
        //-------------------------------------------------
        /// <summary>获取DescriptionAttribute</summary>
        public static string GetDescription(this Type type)
        {
            var objs = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length > 0)
                return (objs[0] as DescriptionAttribute).Description;
            else
                return type.FullName;
        }

        /// <summary>获取历史信息特性元数据</summary>
        public static object[] GetHistories(this Type type)
        {
            var objs = type.GetCustomAttributes(typeof(HistoryAttribute), false);
            return objs;
        }

        /// <summary>获取参数信息元数据</summary>
        public static List<ParamAttribute> GetParamAttributes(this MethodInfo method)
        {
            var objs = method.GetCustomAttributes(typeof(ParamAttribute), false);
            List<ParamAttribute> p = new List<ParamAttribute>();
            foreach (object obj in objs)
                p.Add(obj as ParamAttribute);
            return p;
        }

        /// <summary>获取类型文件的缓存时间</summary>
        public static int GetCacheDuration(this Type type)
        {
            ScriptAttribute attr = ReflectHelper.GetScriptAttribute(type);
            return (attr != null) ? attr.CacheDuration : 0;
        }

        // 获取WebMethodNamespaceAttribute
        public static ScriptAttribute GetScriptAttribute(this Type type)
        {
            return GetAttribute<ScriptAttribute>(type);
        }


        /// <summary>取得HttpApiAttribute</summary>
        public static HttpApiAttribute GetHttpApiAttribute(this MethodInfo info)
        {
            if (info == null) return null;
            HttpApiAttribute attr = GetAttribute<HttpApiAttribute>(info);
            return attr;
        }


        //-------------------------------------------------
        // 方法调用相关
        //-------------------------------------------------
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


        /// <summary>构造匹配方法的参数值列表（若方法名重名怎么处理？）</summary>
        /// <param name="methodName">方法名</param>
        /// <param name="args">参数名-值字典</param>
        /// <returns>排序后的参数值数组</returns>
        public static object[] GetParameters(MethodInfo info, Dictionary<string, object> args)
        {
            List<object> array = new List<object>();
            if (info == null)
                return array.ToArray();

            // 遍历方法参数，找到匹配的输入参数
            foreach (var pi in info.GetParameters())
            {
                // 未找到匹配参数，尝试取方法的默认参数
                if (!args.Keys.Contains(pi.Name))
                {
                    if (pi.HasDefaultValue)
                        array.Add(pi.DefaultValue);
                    else if (pi.ParameterType.IsNullable())
                        array.Add(null);
                    continue;
                }

                // 找到匹配的输入参数
                var o = args[pi.Name];
                var type = pi.ParameterType;
                var realType = type.GetRealType();

                // 如果值为空字符串，且不是字符串类型，尝试取方法的默认参数
                if (o == "" && type != typeof(string) && pi.HasDefaultValue)
                {
                    array.Add(pi.DefaultValue);
                    continue;
                }

                // 如果值为空字符串，且为可空类型，直接设置为null
                if (o == "" && type.IsNullable())
                {
                    array.Add(null);
                    continue;
                }

                // 转换数据类型
                var value = Cast(o, type);
                array.Add(value);
            }
            return array.ToArray();
        }

        // 转换数据类型
        private static object Cast(object o, Type type)
        {
            if (type.IsNullable())
                return Cast(o, type.GetRealType());

            // 字典转化为对象
            if (o is Dictionary<string, object>)
                return DicToObj(o as Dictionary<string, object>, type);
            else if (o != null)
            {
                if (o is string && o != "" && type.IsEnum)          // 处理枚举文本类型
                    return Enum.Parse(type, o.ToString(), true);
                else if (type == typeof(string))                    // 处理字符串类型
                    return o.ToString();
                else if (type.IsValueType)                          // 处理简单值类型
                    return Convert.ChangeType(o, type);             
                else
                    return Newtonsoft.Json.JsonConvert.DeserializeObject(o.ToString(), type);  // 处理对象类型。该方法可以解析可空类型，但无法解析简单数据类型
            }

            return o;
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
            return o.ToString().ParseBasicType(type);
            /*
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
            */
        }


    }
}
