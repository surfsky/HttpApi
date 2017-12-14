using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Web.SessionState;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters.Binary;
using App.HttpApi.Components;

namespace App.HttpApi
{
    /// <summary>
    /// 响应编码器
    /// </summary>
    public class ResponseEncoder
    {
        protected ResponseType DataType {get; set;}
        protected string MimeType { get; set; }
        protected string FileName { get; set; }
        public int CacheSeconds { get; set; } = 0;
        public HttpCacheability CacheLocation { get; set; }

        // 构造器
        public ResponseEncoder(ResponseType dataType, string mimeType=null, string fileName=null, int cacheSeconds=0, HttpCacheability cacheLocation=HttpCacheability.ServerAndPrivate)
        {
            this.DataType = dataType;
            this.MimeType = mimeType;
            this.FileName = fileName;
            this.CacheSeconds = cacheSeconds;
            this.CacheLocation = cacheLocation;
        }


        /// <summary>
        /// 将输出对象写到输出流中
        /// </summary>
        public void Write(object obj)
        {
            if (string.IsNullOrEmpty(this.MimeType))
                this.MimeType = GetDefaultMimeType(this.DataType);

            switch (this.DataType)
            {
                case ResponseType.JavaScript:  WriteText(SerializeHelper.ToText(obj));        break;
                case ResponseType.Text:        WriteText(SerializeHelper.ToText(obj));        break;
                case ResponseType.JSON:        WriteText(SerializeHelper.ToJson(obj));          break;
                case ResponseType.HTML:        WriteText(SerializeHelper.ToText(obj));        break;
                case ResponseType.XML:         WriteText(SerializeHelper.ToXml(obj));           break;
                case ResponseType.ImageBase64: WriteText(SerializeHelper.ToImageBase64(obj));   break;
                case ResponseType.Image:       WriteBinary(SerializeHelper.ToImageBytes(obj));  break;
                case ResponseType.BinaryFile:  WriteBinary(SerializeHelper.ToBinary(obj));      break;
                default:                       WriteText(SerializeHelper.ToText(obj));        break;
            }
        }

        //----------------------------------------------------
        // utils
        //----------------------------------------------------
        // 获取默认的mimetype
        string GetDefaultMimeType(ResponseType dataType)
        {
            switch (dataType)
            {
                case ResponseType.Text:        return @"text/plain";
                case ResponseType.JSON:        return @"application/json";
                case ResponseType.HTML:        return @"text/html";
                case ResponseType.XML:         return @"application/xml";
                case ResponseType.JavaScript:  return @"text/javascript";
                case ResponseType.Image:       return @"image/png";
                case ResponseType.ImageBase64: return @"text/plain";
                case ResponseType.BinaryFile:  return @"application/octet-stream";
                default:                       return @"text/plain";
            }
        }

        // 输出文本
        public void WriteText(string text)
        {
            var response = HttpContext.Current.Response;
            SetCache(response, this.CacheSeconds, this.CacheLocation, "*");
            response.ContentEncoding = HttpContext.Current.Request.ContentEncoding;
            response.ContentType = this.MimeType;
            if (!string.IsNullOrEmpty(this.FileName))
                response.AddHeader("Content-Disposition", "attachment; filename=" + this.FileName);
            response.Write(text);
        }

        // 输出二进制文件
        public void WriteBinary(byte[] bytes)
        {
            var response = HttpContext.Current.Response;
            SetCache(response, this.CacheSeconds, this.CacheLocation, "*");
            response.ClearContent();
            response.ContentType = this.MimeType;
            if (!string.IsNullOrEmpty(this.FileName))
                response.AddHeader("Content-Disposition", "attachment; filename=" + this.FileName);
            response.BinaryWrite(bytes);
        }

        /// <summary>设置页面缓存</summary>
        /// <param name="context">网页上下文</param>
        /// <param name="cacheSeconds">缓存秒数</param>
        /// <param name="varyByParam">缓存参数名称</param>
        public static void SetCache(HttpResponse response, int cacheSeconds, HttpCacheability cacheLocation = HttpCacheability.ServerAndPrivate, string varyByParam = "*")
        {
            if (cacheSeconds == 0)
                return;

            TimeSpan ts = new TimeSpan(0, 0, 0, cacheSeconds);
            HttpCachePolicy cachePolicy = response.Cache;
            cachePolicy.SetCacheability(cacheLocation);
            cachePolicy.VaryByParams[varyByParam] = true;
            cachePolicy.SetExpires(DateTime.Now.Add(ts));
            cachePolicy.SetMaxAge(ts);
            cachePolicy.SetValidUntilExpires(true);
        }
    }
}
