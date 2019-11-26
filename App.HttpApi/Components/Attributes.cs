using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.UI;

namespace App.HttpApi
{
    /// <summary>
    /// 脚本特性，用于控制输出 js 脚本时的一些的命名及缓存
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ScriptAttribute : Attribute
    {
        public string NameSpace { get; set; }
        public string ClassName { get; set; }
        public int CacheDuration { get; set; }
    }

    /// <summary>
    /// 资源特性（尚未启用）
    /// 类似UIAttribute，可以给枚举字段增加注释信息，信息来自Resource
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class RAttribute : Attribute
    {
        public string Name { get; set; }

        public RAttribute(string name)
        {
            this.Name = name;
        }

        public string GetText()
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            var manager = new System.Resources.ResourceManager("App.HttpApi.Properties.Resources", this.GetType().Assembly);
            return manager.GetString(this.Name, culture);
        }
    }

    /// <summary>
    /// 历史版本信息
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
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
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class HttpParamAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Remark { get; set; }

        public HttpParamAttribute(string name, string description)
            : this(name, description, "", "", "")
        {
        }
        internal HttpParamAttribute(string name, string description, string type, string remark, string defaultValue)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Remark = remark;
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

        /// <summary>是否记录日志</summary>
        public bool Log { get; set; } = false;

        //---------------------------------------------------
        // 访问权限控制
        //---------------------------------------------------
        /// <summary>是否校验访问 IP</summary>
        public bool AuthIP { get; set; } = false;

        /// <summary>是否校验授权码</summary>
        public bool AuthToken { get; set; } = false;

        /// <summary>是否校验登录(User.IsAuthenticated)</summary>
        public bool AuthLogin { get; set; } = false;

        /// <summary>可访问的用户（用逗号隔开）</summary>
        public string AuthUsers { get; set; }

        /// <summary>可访问的角色（用逗号隔开）</summary>
        public string AuthRoles { get; set; }

        /// <summary>是否上传文件</summary>
        public bool PostFile { get; set; } = false;


        /// <summary>访问动作列表</summary>
        public List<string> VerbList
        {
            get
            {
                if (AuthVerbs.IsEmpty())
                    return new List<string>();
                else
                    return AuthVerbs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToLower();
            }
        }

        //---------------------------------------------------
        // 构造函数
        //---------------------------------------------------
        public HttpApiAttribute() { }
        public HttpApiAttribute(string description, bool authLogin=false, int cacheSeconds=0)
        {
            this.Description = description;
            this.AuthLogin = authLogin;
            this.CacheSeconds = cacheSeconds;
        }
    }

}
