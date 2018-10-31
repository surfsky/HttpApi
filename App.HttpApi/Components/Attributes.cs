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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ParamAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Info { get; set; }

        public ParamAttribute(string name, string description)
            : this(name, description, "", "", "")
        {
        }
        internal ParamAttribute(string name, string description, string type, string info, string defaultValue)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Info = info;
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
        public Type ReturnType { get; set; }

        /// <summary>响应类型</summary>
        public ResponseType Type { get; set; } = ResponseType.Auto;

        /// <summary>描述信息</summary>
        public string Description { get; set; }

        /// <summary>示例</summary>
        public string Example { get; set; }

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

        /// <summary>封装条件</summary>
        public string WrapCondition { get; set; }

        /// <summary>允许的访问动作（Get/Post)</summary>
        public string AuthVerbs { get; set; }

        /// <summary>状态（Testing, Published, Deprecated)</summary>
        public ApiStatus Status { get; set; }

        //---------------------------------------------------
        // 访问权限控制
        //---------------------------------------------------
        /// <summary>是否校验访问 IP</summary>
        public bool AuthIP { get; set; } = false;

        /// <summary>是否校验访问安全码(存在Cookie[HttpApiSecurityCode]中)</summary>
        public bool AuthSecurityCode { get; set; } = false;

        /// <summary>是否校验登录(User.IsAuthenticated)</summary>
        public bool AuthLogin { get; set; } = false;

        /// <summary>可访问的用户（用逗号隔开）</summary>
        public string AuthUsers { get; set; }

        /// <summary>可访问的角色（用逗号隔开）</summary>
        public string AuthRoles { get; set; }



        /// <summary>访问动作列表</summary>
        public List<string> VerbList
        {
            get
            {
                if (AuthVerbs.IsNullOrEmpty())
                    return new List<string>();
                else
                    return AuthVerbs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToLower();
            }
        }

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
