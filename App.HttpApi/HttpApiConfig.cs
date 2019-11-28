using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web;
using App.HttpApi.Properties;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;


namespace App.HttpApi
{
    /*
    <configSections>
      <section name = "httpApi" type="App.HttpApi.HttpApiConfig, App.HttpApi"/>
    </configSections>
    <httpApi authIPs = "" errorResponse="DataResult" jsonEnumFormatting="Text" wrap="" jsonIndented="Indented" jsonDateTimeFormat="yyyy-MM-dd"/>
    */
    /// <summary>
    /// HttpApi 配置（可在Web.Config中配置）
    /// </summary>
    public class HttpApiConfig : ConfigurationSection
    {
        //--------------------------------------------------
        // 序列化控制配置
        //--------------------------------------------------
        [ConfigurationProperty("formatLowCamel", DefaultValue = false)]
        public bool FormatLowCamel
        {
            get { return (bool)this["formatLowCamel"]; }
            set { this["formatLowCamel"] = value; }
        }

        [ConfigurationProperty("formatIndented", DefaultValue = Formatting.Indented)]
        public Formatting FormatIndented
        {
            get { return (Formatting)this["formatIndented"]; }
            set { this["formatIndented"] = value; }
        }

        [ConfigurationProperty("formatEnum", DefaultValue = EnumFomatting.Text)]
        public EnumFomatting FormatEnum
        {
            get { return (EnumFomatting)this["formatEnum"]; }
            set { this["formatEnum"] = value; }
        }

        [ConfigurationProperty("formatDateTime", DefaultValue = "yyyy-MM-dd HH:mm:ss")]
        public string FormatDateTime
        {
            get { return (string)this["formatDateTime"]; }
            set { this["formatDateTime"] = value; }
        }

        [ConfigurationProperty("formatLongNumber", DefaultValue = "Int64,UInt64,Decimal")]
        public string FormatLongNumber
        {
            get { return (string)this["formatLongNumber"]; }
            set { this["formatLongNumber"] = value; }
        }

        [ConfigurationProperty("errorResponse", DefaultValue=ErrorResponse.APIResult)]
        public ErrorResponse ErrorResponse
        {
            get { return (ErrorResponse)this["errorResponse"]; }
            set { this["errorResponse"] = value; }
        }

        [ConfigurationProperty("typePrefix", DefaultValue = "App.API")]
        public string TypePrefix
        {
            get { return (string)this["typePrefix"]; }
            set { this["typePrefix"] = value; }
        }

        [ConfigurationProperty("wrap")]
        public bool? Wrap
        {
            get { return (bool?)this["wrap"]; }
            set { this["wrap"] = value; }
        }

        [ConfigurationProperty("maxDepth")]
        public int? MaxDepth
        {
            get { return (int?)this["maxDepth"]; }
            set { this["maxDepth"] = value; }
        }

        [ConfigurationProperty("language", DefaultValue="zh-CN")]
        public string Language
        {
            get { return (string)this["language"]; }
            set { this["language"] = value; }
        }

        [ConfigurationProperty("banMinutes", DefaultValue = "30")]
        public int? BanMinutes
        {
            get { return (int?)this["banMinutes"]; }
            set { this["banMinutes"] = value; }
        }

        /// <summary>Json Serializer Settings</summary>
        public JsonSerializerSettings JsonSetting { get; set; }


        //--------------------------------------------------
        // 单例
        //--------------------------------------------------
        private static HttpApiConfig _instance = null;
        public static HttpApiConfig Instance 
        {
            get 
            {
                if (_instance == null)
                {
                    // 尝试从配置节中恢复配置。若未找到配置节，则赋予默认值。
                    _instance = (HttpApiConfig)ConfigurationManager.GetSection("httpApi");
                    if (_instance == null)
                    {
                        _instance = new HttpApiConfig();
                        _instance.FormatIndented = Formatting.Indented;
                        _instance.FormatEnum = EnumFomatting.Text;
                        _instance.Language = "en";
                    }
                    _instance.JsonSetting = _instance.GetJsonSetting();

                    // 设置国际化支持
                    Resources.Culture = new System.Globalization.CultureInfo(_instance.Language);
                }
                return _instance;
            }
        }


        /// <summary>从配置中获取 Json 序列化信息</summary>
        public JsonSerializerSettings GetJsonSetting()
        {
            var settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.MaxDepth = this.MaxDepth;  // 没什么用，不是用于控制输出的json层次的，而是读取层次的

            // 小驼峰命名法
            if (this.FormatLowCamel)
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // 递进格式
            settings.Formatting = this.FormatIndented;

            // 时间格式
            var datetimeConverter = new IsoDateTimeConverter();
            datetimeConverter.DateTimeFormat = this.FormatDateTime;
            settings.Converters.Add(datetimeConverter);

            // 枚举格式
            if (this.FormatEnum == EnumFomatting.Text)
                settings.Converters.Add(new StringEnumConverter());

            // 长数字格式化（转化为字符串）
            var types = this.FormatLongNumber.ParseEnums<TypeCode>();
            settings.Converters.Add(new LongNumberToStringConverter(types));
            return settings;
        }


        //--------------------------------------------------
        // HttpApi访问事件，请在Global中设置
        //--------------------------------------------------
        public delegate void VisitHandler(HttpContext context, MethodInfo method, HttpApiAttribute attr, Dictionary<string, object> inputs);
        public delegate void AuthHandler(HttpContext context, MethodInfo method, HttpApiAttribute attr, string token);
        public delegate void EndHandler(HttpContext context);
        public delegate void ExceptionHandler(MethodInfo method, Exception ex);
        public delegate void BanHandler(string ip, string url);

        /// <summary>访问事件（有异常请直接抛出 HttpApiException 异常）</summary>
        public event VisitHandler OnVisit;

        /// <summary>鉴权事件（有异常请直接抛出 HttpApiException 异常）</summary>
        public event AuthHandler OnAuth;

        /// <summary>结束事件（有异常请直接抛出 HttpApiException 异常）</summary>
        public event EndHandler OnEnd;

        /// <summary>异常时间</summary>
        public event ExceptionHandler OnException;

        /// <summary>禁止事件</summary>
        public event BanHandler OnBan;


        //--------------------------------------------
        // 包裹方法
        //--------------------------------------------
        public void DoVisit(HttpContext context, MethodInfo method, HttpApiAttribute attr, Dictionary<string, object> inputs)
        {
            this.OnVisit?.Invoke(context, method, attr, inputs);
        }

        /// <summary>授权事件</summary>
        public void DoAuth(HttpContext context, MethodInfo method, HttpApiAttribute attr, string token)
        {
            this.OnAuth?.Invoke(context, method, attr, token);
        }

        /// <summary>结束</summary>
        public void DoEnd(HttpContext context)
        {
            this.OnEnd?.Invoke(context);
        }

        /// <summary>异常处理</summary>
        /// <returns>若有自定义异常处理程序，则返回true；否则返回false</returns>
        public void DoException(MethodInfo method, Exception ex)
        {
            this.OnException?.Invoke(method, ex);
        }

        /// <summary>禁止访问</summary>
        public void DoBan(string ip, string url)
        {
            this.OnBan?.Invoke(ip, url);
        }
    }
}
