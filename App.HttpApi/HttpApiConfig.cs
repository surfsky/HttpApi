using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web;

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

        [ConfigurationProperty("apiTypePrefix", DefaultValue = "App.DAL.Db")]
        public string ApiTypePrefix
        {
            get { return (string)this["apiTypePrefix"]; }
            set { this["apiTypePrefix"] = value; }
        }

        [ConfigurationProperty("wrap")]
        public bool? Wrap
        {
            get { return (bool?)this["wrap"]; }
            set { this["wrap"] = value; }
        }

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
                    _instance = (HttpApiConfig)ConfigurationManager.GetSection("httpApi");
                    if (_instance == null)
                    {
                        _instance = new HttpApiConfig();
                        _instance.FormatIndented = Formatting.Indented;
                        _instance.FormatEnum = EnumFomatting.Text;
                    }
                }
                return _instance;
            }
        }

        //--------------------------------------------------
        // HttpApi访问事件，请在Global中设置
        //--------------------------------------------------
        public delegate void VisitHandler(HttpContext context, MethodInfo method, HttpApiAttribute attr, Dictionary<string, object> inputs);
        public delegate void AuthHandler(HttpContext context, MethodInfo method, HttpApiAttribute attr, string token);
        public delegate void EndHandler(HttpContext context);
        public delegate void ExceptionHandler(MethodInfo method, Exception ex);

        /// <summary>访问事件（有异常请直接抛出 HttpApiException 异常）</summary>
        public event VisitHandler OnVisit;

        /// <summary>鉴权事件（有异常请直接抛出 HttpApiException 异常）</summary>
        public event AuthHandler OnAuth;

        /// <summary>结束事件（有异常请直接抛出 HttpApiException 异常）</summary>
        public event EndHandler OnEnd;

        /// <summary>异常处理</summary>
        public event ExceptionHandler OnException;


        //--------------------------------------------
        // 包裹方法
        //--------------------------------------------
        public void DoVisit(HttpContext context, MethodInfo method, HttpApiAttribute attr, Dictionary<string, object> inputs)
        {
            if (this.OnVisit != null)
                this.OnVisit(context, method, attr, inputs);
        }

        /// <summary>授权事件</summary>
        public void DoAuth(HttpContext context, MethodInfo method, HttpApiAttribute attr, string token)
        {
            if (this.OnAuth != null)
                this.OnAuth(context, method, attr, token);
        }

        /// <summary>结束</summary>
        public void DoEnd(HttpContext context)
        {
            if (this.OnEnd != null)
                this.OnEnd(context);
        }

        /// <summary>异常处理</summary>
        /// <returns>若有自定义异常处理程序，则返回true；否则返回false</returns>
        public void DoException(MethodInfo method, Exception ex)
        {
            if (this.OnException != null)
                this.OnException(method, ex);
        }
    }
}
