using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace App.HttpApi
{
    /// <summary>
    /// 错误时返回方式
    /// </summary>
    public enum ErrorResponse
    {
        HttpError = 0,
        DataResult = 1
    }

    [XmlInclude(typeof(ErrorResponse))]
    public class HttpApiConfig : ConfigurationSection
    {
        [ConfigurationProperty("successInfo")]
        public string SuccessInfo
        {
            get { return (string)this["successInfo"]; }
            set { this["successInfo"] = value; }
        }

        [ConfigurationProperty("failInfo")]
        public string FailInfo
        {
            get { return (string)this["failInfo"]; }
            set { this["failInfo"] = value; }
        }

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
