using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
//using App.Core;


namespace App.HttpApi
{
    /// <summary>
    /// 枚举值相关信息
    /// </summary>
    public class EnumInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public string Group { get; set; }

        public string Info
        {
            get
            {
                return this.Group.IsEmpty()
                    ? string.Format("{0}({1})", this.Value, this.Name)
                    : string.Format("{0}({1}/{2})", this.Value, this.Group, this.Name)
                    ;
            }
        }
    }

    /// <summary>
    /// 枚举相关辅助方法（扩展方法）
    /// 尝试去获取 DescriptionAttribute, UIAttribute 的值作为枚举名称，都没有的话才用原Enum名。
    /// Historey: 
    ///     2017-10-31 Init
    ///     2017-11-01 尝试改为泛型版本失败，泛型不支持枚举约束，但类型转化时又必须指明是类类型还是值类型
    ///     以后再尝试，可用T : struct 来约束
    /// </summary>
    /// <example>
    /// public enum OrderStatus
    /// {
    ///     [Description("新建")]  New;
    ///     [UI("完成")]           Finished;
    /// }
    /// var items = typeof(OrderStatus).ToList();
    /// </example>
    internal static class EnumHelper
    {
        /// <summary>判断一个对象是否是枚举类型</summary>
        public static bool IsEnum(this object value)
        {
            return value?.GetType().BaseType == typeof(Enum);
        }

        /// <summary>判断一个类型是否是枚举类型</summary>
        public static bool IsEnum(this Type type)
        {
            return type?.BaseType == typeof(Enum);
        }

        /// <summary>获取枚举的值列表</summary>
        public static List<T> GetEnums<T>(this Type enumType) where T : struct
        {
            //return Enum.GetValues(enumType).CastEnum<T>();
            var values = new List<T>();
            foreach (var value in Enum.GetValues(enumType))
                values.Add((T)value);
            return values;
        }

        
    }

}