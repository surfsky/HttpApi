using Newtonsoft.Json;
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
        // 序列化控制移到配置里面去
        /*
        settings.MissingMemberHandling = MissingMemberHandling.Ignore;
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.Formatting = Formatting.Indented;
        */

        [ConfigurationProperty("jsonIndented", DefaultValue = Formatting.Indented)]
        public Formatting JsonIndented
        {
            get { return (Formatting)this["jsonIndented"]; }
            set { this["jsonIndented"] = value; }
        }

        [ConfigurationProperty("jsonEnumFormatting", DefaultValue = EnumFomatting.Text)]
        public EnumFomatting JsonEnumFormatting
        {
            get { return (EnumFomatting)this["jsonEnumFormatting"]; }
            set { this["jsonEnumFormatting"] = value; }
        }

        [ConfigurationProperty("jsonDateTimeFormat", DefaultValue = "yyyy-MM-dd HH:mm:ss")]
        public string JsonDateTimeFormat
        {
            get { return (string)this["jsonDateTimeFormat"]; }
            set { this["jsonDateTimeFormat"] = value; }
        }
        

        [ConfigurationProperty("authIPs")]
        public string AuthIPs
        {
            get { return (string)this["authIPs"]; }
            set { this["authIPs"] = value; }
        }


        [ConfigurationProperty("errorResponse", DefaultValue=ErrorResponse.DataResult)]
        public ErrorResponse ErrorResponse
        {
            get { return (ErrorResponse)this["errorResponse"]; }
            set { this["errorResponse"] = value; }
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
                    _instance = (HttpApiConfig)ConfigurationManager.GetSection("httpApi");
                    if (_instance == null)
                    {
                        _instance = new HttpApiConfig();
                        _instance.JsonIndented = Formatting.Indented;
                        _instance.JsonEnumFormatting = EnumFomatting.Text;
                    }
                }
                return _instance;
            }
        }
    }
}
