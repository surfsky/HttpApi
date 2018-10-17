using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.UI;

namespace App.HttpApi
{
    /// <summary>
    /// WebMethod脚本特性，用于控制输出 js 脚本时的一些的命名及缓存
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ScriptAttribute : Attribute
    {
        public string NameSpace { get; set; }
        public string ClassName { get; set; }
        public int CacheDuration { get; set; }
    }

    /// <summary>
    /// 历史版本信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class HistoryAttribute : Attribute
    {
        public string Date { get; set; }
        public string User { get; set; }
        public string Info { get; set; }

        public HistoryAttribute(string date, string user, string info)
        {
            this.Date = date;
            this.User = user;
            this.Info = info;
        }
    }

    /// <summary>
    /// 参数信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ParameterAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }

        public ParameterAttribute(string name, string description, string defaultValue="")
        {
            this.Name = name;
            this.Description = description;
            this.DefaultValue = defaultValue;
        }
    }


    /// <summary>
    /// HttpApi特性，拥有该特性的方法都可以提供WebAPI服务
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpApiAttribute : Attribute
    {
        /// <summary>返回值类型</summary>
        public ResponseType Type { get; set; } = ResponseType.Auto;

        /// <summary>描述信息</summary>
        public string Description { get; set; }

        /// <summary>备注</summary>
        public string Remark { get; set; }

        /// <summary>缓存的秒数。默认为0，即没有任何缓存。</summary>
        public int CacheSeconds { get; set; } = 0;

        /// <summary>缓存位置（默认服务器和客户端都缓存）</summary>
        public HttpCacheability CacheLocation { get; set; } = HttpCacheability.ServerAndPrivate;

        /// <summary>导出文件的MIME类别</summary>
        public string MimeType { get; set; }

        /// <summary>导出文件名</summary>
        public string FileName { get; set; }

        /// <summary>是否对文本类型（Json, Text, Xml, ImageBase64)的数据进行 DataResult 封装</summary>
        public bool Wrap { get; set; } = false;

        /// <summary>封装成功后显示的信息</summary>
        public string WrapInfo { get; set; }

        /// <summary>允许的访问动作（Get/Post)</summary>
        public string Verbs { get; set; }

        /// <summary>状态（Testing, Published, Deprecated)</summary>
        public ApiStatus Status { get; set; }

        //---------------------------------------------------
        // 访问权限控制
        //---------------------------------------------------
        /// <summary>可访问的用户（用逗号隔开）</summary>
        public string AuthUsers { get; set; }

        /// <summary>可访问的角色（用逗号隔开）</summary>
        public string AuthRoles { get; set; }

        /// <summary>是否校验登录(User.IsAuthenticated)</summary>
        public bool AuthLogin { get; set; } = false;


        //---------------------------------------------------
        // 构造函数
        //---------------------------------------------------
        public HttpApiAttribute() { }
        public HttpApiAttribute(string description)
        {
            this.Description = description;
        }
    }

}
