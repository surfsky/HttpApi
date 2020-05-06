//using System.Web.Script.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace App.HttpApi
{
    /// <summary>请求解码器基类</summary>
    public abstract class RequestDecoder
    {
        protected HttpContext _context;
        protected RequestDecoder(HttpContext context)
        {
            this._context = context;
        }

        /// <summary>创建解码器（尝试根据ContentType来构造解析器，但往往不准确，客户端没那么乖）</summary>
        public static RequestDecoder CreateInstance(HttpContext context)
        {
            // 若客户端明确发送了ContentType，则调用相应的 Decoder
            string contentType = context.Request.ContentType?.ToLower();
            if (!string.IsNullOrEmpty(contentType))
            {
                if (contentType.IndexOf("application/json") >= 0)
                    return new JsonDecoder(context);
                if (contentType.IndexOf("application/x-www-form-urlencoded") >= 0)
                    return new JsonDecoder(context);
                if (contentType.IndexOf("multipart/") >= 0)
                    return new MultipartFormDecoder(context);
                if (contentType.IndexOf("application/xml") >= 0)
                    return new JsonDecoder(context);
                //if (contentType.IndexOf("application/contract") >= 0)
                //    throw new NotImplementedException();
            }

            // 若是 POST 请求方式用 JsonDecoder
            if (context.Request.Method == "POST")
                return new JsonDecoder(context);

            // 否则尝试分析querystring，若有则用url解析器
            if (context.Request.Query.Count > 0)
                return new UrlDecoder(context);

            // 默认用 UrlDecoder
            return new UrlDecoder(context);
        }

        /// <summary>取得方法名（以url最后一部分作为方法名。如：..\Handler1.ashx\GetData）</summary>
        public virtual string MethodName
        {
            get
            {
                //int n = this._context.Request.Url.Segments.Length;
                //return this._context.Request.Url.Segments[n - 1];
                //int n = this._context.Request.Path..Segments.Length;
                //return this._context.Request.Url.Segments[n - 1];
                var u = new Url(_context.Request.Path.ToString());
                return u.FileName;
            }
        }

        /// <summary>解析请求参数</summary>
        public abstract Dictionary<string, object> ParseArguments();
    }




    ///-----------------------------------------------
    /// URL
    ///-----------------------------------------------
    /// <summary>URL 解码器</summary>
    internal class UrlDecoder : RequestDecoder
    {
        internal UrlDecoder(HttpContext context)
            : base(context)
        {
        }

        public override Dictionary<string, object> ParseArguments()
        {
            var data = new Dictionary<string, object>();
            var qs = this._context.Request.QueryString.Value;
            var dict = qs.ParseDict();
            foreach (var key in  dict.Keys)
                data.Add(key, dict[key]);

            return data;
        }

        public override string MethodName
        {
            get
            {

                //int n = this._context.Request.Url.Segments.Length;
                //string methodName = this._context.Request.Url.Segments[n - 1];
                //if (methodName.ToLower().LastIndexOf(".ashx") >= 0)
                //    return "js";  // 缺省函数名称
                //else
                //    return methodName;

                var u = new Url(_context.Request.Path.ToString());
                return (u.FileExtesion == ".ashx") ? "js" : u.FileName;
            }
        }
    }

    ///-----------------------------------------------
    /// JSON POST
    ///-----------------------------------------------
    /// <summary>JSON 解码器</summary>
    internal class JsonDecoder : RequestDecoder
    {
        internal JsonDecoder(HttpContext context)
            : base(context)
        {
            if (this._context.Request.Method.ToUpper() == "GET")
                throw new NotSupportedException("不支持GET请求");
        }

        /// <summary>解析参数</summary>
        public override Dictionary<string, object> ParseArguments()
        {
            // 解析 Post 上来的字符串
            /*
            var stream = this._context.Request.Body;
            var buffer = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buffer, 0, (int)stream.Length);
            var enc = GetRequestEncoding(_context.Request);
            var str = enc.GetString(buffer);

            // 将字符串解析为字典（不知道能否解析复杂类，可能要根据方法的参数类型来强制转换）
            //object obj = Newtonsoft.Json.JsonConvert.DeserializeObject(str);  // 该方法会将字符串解析成 匿名对象
            //object obj = new JavaScriptSerializer().DeserializeObject(str); // 该方法会将字符串解析成 Dictionary
            object obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
            var dict = obj as Dictionary<string, object>;
            */

            // 自己解析比较麻烦，直接用Asp内置的机制来简化参数解析操作
            var dict = new Dictionary<string, object>();
            foreach (var item in _context.Request.Form)
                dict.Add(item.Key, item.Value);

            // 附加上 QueryString（有些请求既有Post部分，又有 QueryString 部分，需要补全）
            foreach (var q in _context.Request.Query)
                dict.Add(q.Key, q.Value);
            return dict;
        }

        private Encoding GetRequestEncoding(HttpRequest request)
        {
            var requestContentType = request.ContentType;
            var requestMediaType = requestContentType == null ? default(MediaType) : new MediaType(requestContentType);
            var requestEncoding = requestMediaType.Encoding;
            if (requestEncoding == null)
                requestEncoding = Encoding.UTF8;

            return requestEncoding;
        }

    }


    ///-----------------------------------------------
    /// <summary> Multipart Form 解码器（带附件）</summary>
    internal class MultipartFormDecoder : RequestDecoder
    {
        internal MultipartFormDecoder(HttpContext context)
            : base(context)
        {
            if (this._context.Request.Method.ToUpper() == "GET")
                throw new NotSupportedException("不支持GET请求");
        }

        /// <summary>解析参数</summary>
        public override Dictionary<string, object> ParseArguments()
        {
            // 自己解析比较麻烦，直接用Asp内置的机制来简化参数解析操作
            var dict = new Dictionary<string, object>();
            foreach (var item in _context.Request.Form)
                dict.Add(item.Key, item.Value);

            // 附加上 QueryString（有些请求既有Post部分，又有 QueryString 部分，需要补全）
            foreach (var item in _context.Request.Query)
                dict.Add(item.Key, item.Value);
            return dict;
        }
    }
}
