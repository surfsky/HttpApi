using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace App.HttpApi
{
    /// <summary>
    /// Cookie 辅助处理方式
    /// </summary>
    internal class CookieHelper
    {
        /// <summary>查找Cookie值（可处理cookie名重复情况）</summary>
        public static string FindCookie(string cookieKey)
        {
            var pairs = GetCookies();
            foreach (var pair in pairs)
            {
                if (pair.Key == cookieKey)
                    return pair.Value;
            }
            return "";
        }

        /// <summary>将 cookie 字符串解析为键值对列表（键值可重复）</summary>
        public static List<KeyValuePair<string, string>> GetCookies()
        {
            var cookieText = Asp.Current.Request.Headers["Cookie"].FirstOrDefault();
            var pairs = new List<KeyValuePair<string, string>>();
            if (cookieText.IsNotEmpty())
            {
                var cookies = cookieText.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cookie in cookies)
                {
                    int n = cookie.IndexOf('=');
                    if (n != -1)
                    {
                        var key = cookie.Substring(0, n).Trim();
                        var value = cookie.Substring(n + 1).Trim();
                        pairs.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
            return pairs;
        }



        /// <summary>读cookie值</summary>
        /// <param name="name">名称</param>
        /// <returns>cookie值</returns>
        public static string GetCookie(string name)
        {
            if (Asp.Current.Request.Cookies != null)
                return Asp.Current.Request.Cookies[name];

            return null;
        }

        /// <summary>
        /// 设置cookie
        /// </summary>
        public static void SetCookie(string name, string value)
        {
            SetCookie(name, value, 20);
        }

        /// <summary>
        /// 设置cookie
        /// </summary>
        public static void SetCookie(string name, string value, int expires)
        {
            //HttpCookie cookie = new HttpCookie(name, value);
            //cookie.Expires = DateTime.Now.AddMinutes(expires);
            //Asp.Current.Response.Cookies.SetCookie(cookie);
            //Asp.Current.Response.Cookies.Delete(name);
            Asp.Current.Response.Cookies.Append(name, value, new CookieOptions() { Expires = DateTime.Now.AddMinutes(expires) });
        }

        public static void InsertCookie(string name, string value)
        {
            InsertCookie(name, value, 20);
        }

        public static void InsertCookie(string name, string value, int expires)
        {
            //HttpCookie cookie = new HttpCookie(name);
            //cookie.Expires = DateTime.Now.AddMinutes(expires);
            //Http.Current.Response.Cookies.Add(cookie);
            Asp.Current.Response.Cookies.Append(name, value, new CookieOptions() { Expires = DateTime.Now.AddMinutes(expires) });
        }

        public static void RemoveCookie(string name)
        {
            //HttpCookie cookie = Http.Current.Request.Cookies[name];
            //if (cookie != null)
            //    Http.Current.Response.Cookies.Remove(name);
            Asp.Current.Response.Cookies.Delete(name);
        }

        public static void ClearCookie()
        {
            //Http.Current.Request.Cookies.Clear();
            foreach (var key in Asp.Current.Request.Cookies.Keys)
                Asp.Current.Response.Cookies.Delete(key);
        }
    }
}
