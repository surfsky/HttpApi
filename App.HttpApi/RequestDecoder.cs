/****************************************************************************
 *
 * 功能描述：    Web请求参数解码器
 * 作    者：    wzcheng
 * 修改日期：    2010/04/16,2010/04/21
 *  
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace App.HttpApi
{
    /// <summary>
    /// 抽象的解码器
    /// </summary>
    public abstract class RequestDecoder
    {
        protected HttpContext _context;
        protected RequestDecoder(HttpContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// 创建一个解码器（尝试根据ContentType来构造解析器，但往往不准确，客户端没那么乖）
        /// </summary>
        /// <returns></returns>
        public static RequestDecoder CreateInstance(HttpContext context)
        {
            // 若客户端明确发送了ContentType，则调用相应的 Decoder
            string contentType = context.Request.ContentType.ToLower();
            if (!string.IsNullOrEmpty(contentType))
            {
                if (contentType.IndexOf("application/json") >= 0)
                    return new JsonDecoder(context);
                if (contentType.IndexOf("application/x-www-form-urlencoded") >= 0)
                    return new SimpleUrlDecoder(context);
                if (contentType.IndexOf("application/xml") >= 0)
                    return new JsonDecoder(context);
                //if (contentType.IndexOf("application/contract") >= 0)
                //    throw new NotImplementedException();
            }

            // 否则尝试分析querystring，若有则用url解析器
            if (context.Request.QueryString.Count > 0)
                return new SimpleUrlDecoder(context);

            // 若是 POST 请求方式用 JsonDecoder
            if (context.Request.RequestType == "POST")
                return new JsonDecoder(context);

            // 默认用SimpleUrlDecoder
            return new SimpleUrlDecoder(context);
        }

        /// <summary>
        /// 取得逻辑的方法名（以url最后一部分作为方法名。如：..\Handler1.ashx\GetData）
        /// </summary>
        public virtual string MethodName
        {
            get
            {
                int segLen = this._context.Request.Url.Segments.Length;
                string methodName = this._context.Request.Url.Segments[segLen - 1];
                return methodName;
            }
        }

        /// <summary>
        /// 反序列化话请求中的数据
        /// </summary>
        public abstract Dictionary<string, object> ParseArguments();
    }


    ///-----------------------------------------------
    /// <summary>
    /// JSON方式的解码器
    /// </summary>
    internal class JsonDecoder : RequestDecoder
    {
        internal JsonDecoder(HttpContext context)
            : base(context)
        {
            if (this._context.Request.HttpMethod.ToUpper() == "GET")
                throw new NotSupportedException("不支持GET请求");
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> ParseArguments()
        {
            // 获取原始输入字符串
            Stream inStr = this._context.Request.InputStream;
            inStr.Position = 0;
            byte[] buffer = new byte[inStr.Length];
            inStr.Read(buffer, 0, (int)inStr.Length);
            Encoding en = this._context.Request.ContentEncoding;
            string str = en.GetString(buffer);

            // 将字符串解析为字典（不知道能否解析复杂类，可能要根据方法的参数类型来强制转换）
            //object obj = Newtonsoft.Json.JsonConvert.DeserializeObject(str);  // 该方法会将字符串解析成 匿名对象
            object obj = new JavaScriptSerializer().DeserializeObject(str); // 该方法会将字符串解析成 Dictory
            Dictionary<string, object> result = obj as Dictionary<string, object>;

            // 字典附加上querystring
            NameValueCollection queryStr = this._context.Request.QueryString;
            foreach (string name in queryStr)
                result.Add(name, queryStr[name]);
            return result;
        }
    }

    ///-----------------------------------------------
    /// <summary>
    /// 简单格式的URL解码器
    /// </summary>
    internal class SimpleUrlDecoder : RequestDecoder
    {
        internal SimpleUrlDecoder(HttpContext context)
            : base(context)
        {
        }

        public override Dictionary<string, object> ParseArguments()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            NameValueCollection queryStr = this._context.Request.Params;
            //NameValueCollection queryStr = (_context.Request.HttpMethod == "GET") ? this._context.Request.QueryString : this._context.Request.Form;

            foreach (string name in queryStr)
            {
                if (name == "ALL_HTTP")
                    break;
                if (name == null)
                    continue;
                else
                    data.Add(name, queryStr[name]);
            }
            return data;
        }

        public override string MethodName
        {
            get
            {
                int segLen = this._context.Request.Url.Segments.Length;
                string methodName = this._context.Request.Url.Segments[segLen - 1];
                if (methodName.ToLower().LastIndexOf(".ashx") >= 0)
                    return "js";  // 缺省函数名称
                else
                    return methodName;
            }
        }
    }
}
