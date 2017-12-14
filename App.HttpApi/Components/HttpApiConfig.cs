using App.HttpApi.Components;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace App.HttpApi
{
    /// <summary>
    /// Web.Config 中的配置节
    /// </summary>
    public class HttpApiConfig : ConfigurationSection
    {
        /*
        // 成功信息移到方法特性里面去，无需全局配置
        [ConfigurationProperty("wrapInfo")]
        public string WrapInfo
        {
            get { return (string)this["wrapInfo"]; }
            set { this["wrapInfo"] = value; }
        }

        // 失败信息各不同，不用统一
        [ConfigurationProperty("failInfo")]
        public string FailInfo
        {
            get { return (string)this["failInfo"]; }
            set { this["failInfo"] = value; }
        }
        */

        [ConfigurationProperty("authIPs")]
        public string AuthIPs
        {
            get { return (string)this["authIPs"]; }
            set { this["authIPs"] = value; }
        }


        [ConfigurationProperty("errorResponse", DefaultValue="DataResult")]
        public ErrorResponse ErrorResponse
        {
            get { return (ErrorResponse)this["errorResponse"]; }
            set { this["errorResponse"] = value; }
        }

        [ConfigurationProperty("enumResponse", DefaultValue = "Text")]
        public EnumResponse EnumResponse
        {
            get { return (EnumResponse)this["enumResponse"]; }
            set { this["enumResponse"] = value; }
        }

        [ConfigurationProperty("wrap")]
        public bool? Wrap
        {
            get { return (bool?)this["wrap"]; }
            set { this["wrap"] = value; }
        }

        // 单例
        private static HttpApiConfig _instance = null;
        public static HttpApiConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                     var section = (HttpApiConfig)ConfigurationManager.GetSection("httpApi");
                    _instance = section ?? new HttpApiConfig();
                }
                return _instance;
            }
        }
    }
}
