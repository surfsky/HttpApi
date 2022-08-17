using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web;
//using AppPlat.HttpApi.Properties;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using HttpApiCore.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace App.HttpApi
{
    /*
    "httpApi" : {
        authIPs = "",
        errorResponse="DataResult",
        jsonEnumFormatting="Text",
        wrap="",
        jsonIndented="Indented",
        jsonDateTimeFormat="yyyy-MM-dd"
    }
    */
    /// <summary>
    /// HttpApi 配置
    /// </summary>
    public class HttpApiConfig
    {
        public bool FormatLowCamel          { get; set; } = false;
        public Formatting FormatIndented    { get; set; } = Formatting.Indented;
        public EnumFomatting FormatEnum     { get; set; } = EnumFomatting.Text;
        public string FormatDateTime        { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public string FormatLongNumber      { get; set; } = "Int64,UInt64,Decimal";
        public ErrorResponse ErrorResponse  { get; set; } = ErrorResponse.APIResult;
        public string TypePrefix            { get; set; } = "AppPlat.API.";
        public string Language              { get; set; } = "en";
        public bool? Wrap                   { get; set; } = null;
        public int? MaxDepth                { get; set; } = null;
        public int? BanMinutes              { get; set; } = null;

        /// <summary>Json Serializer Settings</summary>
        public JsonSerializerSettings JsonSetting { get; set; }


        //public static void Configure(IHttpContextAccessor contextAccessor)
        //{
        //    Asp.Configure(contextAccessor);
        //}

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
                    //var cfg = Configuration.GetSection<HttpApiConfig>("httpApi");
                    _instance = new HttpApiConfig();
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
