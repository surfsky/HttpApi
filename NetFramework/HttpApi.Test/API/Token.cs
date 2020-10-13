using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.Core;

namespace App
{
    /// <summary>
    /// 身份验票
    /// </summary>
    public class Token
    {
        public string AppKey { get; set; }
        public string TimeStamp { get; set; }
        public DateTime ExpireDt { get; set; }

        /// <summary>默认构造函数</summary>
        public Token(string appKey, string timeStamp, DateTime expireDt)
        {
            this.AppKey = appKey;
            this.TimeStamp = timeStamp;
            this.ExpireDt = expireDt;
        }

        /// <summary>创建验票字符串</summary>
        public static string Create(string appKey, string appSecret, int minutes)
        {
            // TODO: 检测appKey和AppSecret有效性
            // 创建验票字符串
            var now = DateTime.Now;
            var o = new Token(appKey, now.ToTimeStamp(), now.AddMinutes(minutes));
            return o.ToJson().DesEncrypt("12345678");
        }

        /// <summary>检测验票字符串</summary>
        public static Token Check(string tokenText)
        {
            var o = tokenText.DesDecrypt("12345678").ParseJson<Token>();
            if (o != null && o.ExpireDt > DateTime.Now)
                return o;
            return null;
        }
    }
}