using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Security.Principal;

namespace App.HttpApi
{
    /// <summary>
    /// 表单鉴权辅助函数（将用户、角色等信息用加密字符串保存在cookie中）。
    /// （1）CreateCookieTicket 创建验票，并将用户角色过期时间等信息加密保存在cookie中。
    /// （2）LoadCookieTicket 从cookie解析验票并设置当前登录人信息。
    /// （3）Logout 注销
    /// </summary>
    /// <remarks>参照此原理应该可以弄个基于querystring或者hidden的验票方案</remarks>
    public class HttpApiAuth
    {
        //-----------------------------------------------
        // 创建 Cookie 验票
        //-----------------------------------------------
        /// <summary>创建并设置用户验票Cookie</summary>
        /// <param name="userId">用户</param>
        /// <param name="roles">角色属猪</param>
        /// <param name="expiration">验票到期时间</param>
        public static IPrincipal Login(string user, string[] roles, DateTime expiration)
        {
            return CreateCookieTicket(user, roles, "", FormsAuthentication.FormsCookieName, expiration);
        }
        public static IPrincipal CreateCookieTicket(string user, string[] roles, string domain, string cookieName, DateTime expiration)
        {
            // ticket
            FormsAuthenticationTicket ticket = CreateTicket(user, roles, expiration);
            string ticketString = FormsAuthentication.Encrypt(ticket);
            // cookie
            HttpCookie cookie = new HttpCookie(cookieName, ticketString);
            cookie.Expires = expiration;
            cookie.Domain = domain;
            HttpContext.Current.Response.Cookies.Add(cookie);
            // return
            HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(ticket), roles);
            return HttpContext.Current.User;
        }


        //-----------------------------------------------
        // 读取 Cookie 验票
        //-----------------------------------------------
        /// <summary>从cookie中读取验票并设置当前用户</summary>
        public static IPrincipal LoadCookiePrincipal()
        {
            return LoadCookiePrincipal(FormsAuthentication.FormsCookieName);
        }
        internal static IPrincipal LoadCookiePrincipal(string cookieName)
        {
            string user;
            string[] roles;
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = ParseTicketString(authCookie.Value, out user, out roles);
                HttpContext.Current.User = new GenericPrincipal(new FormsIdentity(authTicket), roles);
                return HttpContext.Current.User;
            }
            return null;
        }

        /// <summary>
        /// 获取cookie验票用户
        /// </summary>
        public static string GetCookieTicketUser()
        {
            return GetCookieTicketUser(FormsAuthentication.FormsCookieName);
        }
        public static string GetCookieTicketUser(string cookieName)
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (authCookie != null)
            {
                string user;
                string[] roles;
                FormsAuthenticationTicket authTicket = ParseTicketString(authCookie.Value, out user, out roles);
                return user;
            }
            return null;
        }

        /// <summary>
        /// 获取cookie验票用户拥有的角色
        /// </summary>
        public static string[] GetcookieTicketUserRoles()
        {
            return GetCookieTicketUserRoles(FormsAuthentication.FormsCookieName);
        }
        public static string[] GetCookieTicketUserRoles(string cookieName)
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[cookieName];
            if (authCookie != null)
            {
                string user;
                string[] roles;
                FormsAuthenticationTicket authTicket = ParseTicketString(authCookie.Value, out user, out roles);
                return roles;
            }
            return null;
        }

        //-----------------------------------------------
        // 注销处理
        //-----------------------------------------------
        /// <summary>
        /// 注销。销毁验票
        /// </summary>
        public static void Logout()
        {
            Logout(FormsAuthentication.FormsCookieName);
        }
        public static void Logout(string cookieName)
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Request.Cookies[cookieName].Expires = System.DateTime.Now;
            HttpContext.Current.User = null;
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session.Abandon();
        }

        public static void RediretToLoginPage()
        {
            FormsAuthentication.RedirectToLoginPage();
        }


        //-----------------------------------------------
        // 验票字符串处理
        //-----------------------------------------------
        /// <summary>
        /// 创建验票字符串
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="roles">角色列表</param>
        /// <param name="expiration">过期时间</param>
        /// <returns>加密后的验票字符串</returns>
        public static string CreateTicketString(string user, string[] roles, DateTime expiration)
        {
            FormsAuthenticationTicket authTicket = CreateTicket(user, roles, expiration);
            return FormsAuthentication.Encrypt(authTicket);
        }
        public static FormsAuthenticationTicket CreateTicket(string user, string[] roles, DateTime expiration)
        {
            // 将角色数组转化为字符串
            string userData = "";
            if (roles != null)
                foreach (string role in roles)
                    userData += role + ";";

            // 创建验票并加密之
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                1,                          // 版本
                user,                       // 用户名
                DateTime.Now,               // 创建时间
                expiration,                 // 到期时间
                false,                      // 非永久
                userData                    // 用户数据
                );
            return authTicket;
        }

        /// <summary>
        /// 解析验票字符串，获取用户和角色信息
        /// </summary>
        /// <param name="ticket">验票字符串</param>
        /// <param name="user">用户名</param>
        /// <param name="roles">角色列表</param>
        /// <returns>表单验证票据对象</returns>
        public static FormsAuthenticationTicket ParseTicketString(string ticketString, out string user, out string[] roles)
        {
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(ticketString);
            user = authTicket.Name;
            roles = authTicket.UserData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return authTicket;
        }

    }
}
