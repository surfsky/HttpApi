using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace App.HttpApi
{
    /// <summary>
    /// 类拥有的 API 清单
    /// </summary>
    public class TypeAPI
    {
        public string Description { get; set; }
        public object[] Histories { get; set; }
        public List<API> Apis { get; set; }
    }

    /// <summary>
    /// API信息（可考虑继承或与 HttpApiAttribute 合并）
    /// </summary>
    public class API
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
        public string ReturnType { get; set; }
        public int CacheDuration { get; set; }
        public bool AuthIP { get; set; }
        public bool AuthToken { get; set; }
        public bool AuthLogin { get; set; }
        public int AuthTraffic { get; set; }
        public string AuthUsers { get; set; }
        public string AuthRoles { get; set; }
        public string AuthVerbs { get; set; }
        public ApiStatus Status { get; set; }
        public string Remark { get; set; }
        public string Url { get; set; }
        public string UrlTest { get; set; }
        public bool Log { get; set; }
        public bool PostFile { get; set; }

        public List<HttpParamAttribute> Params { get; set; }

        [NonSerialized]
        public MethodInfo Method;

        [NonSerialized]
        public ResponseType RType;
    }
}
