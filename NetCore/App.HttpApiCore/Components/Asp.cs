using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace App.HttpApi
{

    /// <summary>
    /// Asp.net 相关辅助方法
    /// </summary>
    internal static class Asp
    {
        public static HttpContext Current => _contextAccessor.HttpContext;
        private static IHttpContextAccessor _contextAccessor;
        public static void Configure(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public static string GetAbsoluteUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

        /// <summary>获取客户端真实IP</summary>
        public static string GetClientIP()
        {
            if (Asp.Current != null)
            {
                return Current.Connection.RemoteIpAddress.ToString();
                /*
                var request = Asp.Current.Request;
                return (request.ServerVariables["HTTP_VIA"] != null)
                    ? request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString()   // 使用代理，尝试去找原始地址
                    : request.ServerVariables["REMOTE_ADDR"].ToString()            // 
                    ;
                //return request.UserHostAddress;
                */
            }
            return "";
        }

        //---------------------------------------------
        // 访问权限控制
        //---------------------------------------------
        // 当前用户是否已经登录
        public static bool IsLogin()
        {
            System.Security.Principal.IPrincipal p = Asp.Current.User;
            if (p == null || p.Identity == null) return false;
            return p.Identity.IsAuthenticated;
        }

        // 当前用户是否在限定用户列表中
        public static bool IsInUsers(string[] names)
        {
            System.Security.Principal.IPrincipal p = Asp.Current.User;
            if (p == null || p.Identity == null)
                return false;

            string name = p.Identity.Name;
            if (string.IsNullOrEmpty(name))
                return false;

            return ((System.Collections.IList)names).Contains(name);
        }

        // 当前用户是否在限定角色列表中
        public static bool IsInRoles(string[] roles)
        {
            System.Security.Principal.IPrincipal p = Asp.Current.User;
            if (p == null)
                return false;

            foreach (string role in roles)
                if (p.IsInRole(role))
                    return true;
            return false;
        }
    }
}
