using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Web;
using System.Web.Caching;

namespace App.HttpApi
{
    /// <summary>
    /// 缓存辅助类
    /// </summary>
    internal class CacheHelper
    {
        //-------------------------------------------------
        // 缓存相关
        //-------------------------------------------------
        // 数据缓存辅助方法
        public static T GetCachedObject<T>(string key, Func<T> func) where T : class
        {
            return GetCachedObject<T>(key, System.Web.Caching.Cache.NoAbsoluteExpiration, func);
        }
        public static T GetCachedObject<T>(string key, DateTime expiredTime, Func<T> func) where T : class
        {
            var cache = HttpRuntime.Cache; // 等价于 HttpContext.Current.Cache
            if (cache[key] == null) 
            {
                T o = func();
                if (o != null)
                    cache.Insert(key, o, null, expiredTime, Cache.NoSlidingExpiration); // 如果存在则会覆盖; 使用绝对时间过期策略；
            }
            return cache[key] as T;
        }

        /// <summary>
        /// 设置缓存策略（使用context.Response.Cache来缓存输出）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attr"></param>
        public static void SetCachePolicy(HttpContext context, int cacheDuration, bool varyByParams = true)
        {
            if (cacheDuration > 0)
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Server);
                context.Response.Cache.SetExpires(DateTime.Now.AddSeconds((double)cacheDuration));
                context.Response.Cache.SetSlidingExpiration(false);
                context.Response.Cache.SetValidUntilExpires(true);
                if (varyByParams)
                    context.Response.Cache.VaryByParams["*"] = true;
                else
                    context.Response.Cache.VaryByParams.IgnoreParams = true;
            }
            else
            {
                context.Response.Cache.SetNoServerCaching();
                context.Response.Cache.SetMaxAge(TimeSpan.Zero);
            }
        }


    }
}
