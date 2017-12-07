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
    public class APIInfos
    {
        public string Desc { get; set; }
        public object[] Histories { get; set; }
        public List<APIInfo> Apis { get; set; }
    }

    /// <summary>
    /// API信息
    /// </summary>
    public class APIInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int CacheDuration { get; set; }
        public bool AuthLogin { get; set; }
        public string AuthUsers { get; set; }
        public string AuthRoles { get; set; }
        public string Remark { get; set; }
        public string Url { get; set; }

        [NonSerialized]
        public MethodInfo Method;

        [NonSerialized]
        public ResponseType RspType;
    }
}
