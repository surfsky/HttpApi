using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
//using System.Web.Caching;
//using Newtonsoft.Json.Converters;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.FileProviders.Physical;

namespace App.HttpApi
{
    /// <summary>
    /// 缓存辅助类
    /// 参考 https://www.cnblogs.com/wyy1234/p/10519681.html#_label1_0
    /// </summary>
    internal class CacheHelper
    {
        public static MemoryCache Cache = new MemoryCache(new MemoryCacheOptions() { });

        /// <summary>从缓存中获取数据。若该缓存失效，则自动从创建对象并塞入缓存</summary>
        public static T GetCache<T>(string key, DateTime? expiredTime, Func<T> func) where T : class
        {
            if (!Cache.TryGetValue(key, out object o))
            {
                o = func();
                if (expiredTime == null)
                    Cache.Set(key, o);
                else
                    Cache.Set(key, o, expiredTime.Value - DateTime.Now);
            }
            return o as T;
        }




    }
}
