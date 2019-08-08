using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App
{
    /// <summary>
    /// 身份验票
    /// </summary>
    public class Token
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string TimeStamp { get; set; }
        public DateTime ExpireDt { get; set; }

        public Token(string appKey, string appSecret, string timeStamp, DateTime expireDt)
        {
            this.AppKey = AppKey;
            this.AppSecret = AppSecret;
            this.TimeStamp = timeStamp;
            this.ExpireDt = expireDt;
        }
    }
}