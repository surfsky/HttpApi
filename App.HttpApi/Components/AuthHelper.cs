using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Security.Principal;

namespace App.HttpApi
{
    /// <summary>存储用户名及角色列表的Principal</summary>
    internal class UserRolePrincipal : GenericPrincipal
    {
        public string[] Roles { get; set; }
        public UserRolePrincipal(IIdentity identity, string[] roles)
            : base(identity, roles)
        {
            this.Roles = roles;
        }
    }

    /// <summary>
    /// 表单鉴权辅助函数（将用户、角色等信息用加密字符串保存在cookie中）。
    /// （1）Login 创建验票，并将用户角色过期时间等信息加密保存在cookie中。
    /// （2）LoadPrincipal 从cookie解析验票并设置当前登录人信息。
    /// （3）Logout 注销
    /// </summary>
    internal class AuthHelper
    {
        /// <summary>是否登录</summary>
        public static bool IsLogin()
        {
            return (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated);
        }

        /// <summary>当前登录用户名</summary>
        public static string GetLoginUserName()
        {
            return IsLogin() ? HttpContext.Current.User.Identity.Name : "";
        }

        /// <summary>当前登录用户是否具有某个角色</summary>
        public static bool HasRole(string role)
        {
            if (IsLogin())
                return HttpContext.Current.User.IsInRole(role);
            return false;
        }



        //-----------------------------------------------
        // 登录
        //-----------------------------------------------
        /// <summary>登录（设置当前用户，并创建用户验票Cookie）。</summary>
        /// <param name="userId">用户</param>
        /// <param name="roles">角色名称列表</param>
        /// <param name="expiration">验票到期时间</param>
        /// <example>AuthHelper.Login("Admin", new string[] { "Admins" }, DateTime.Now.AddDays(1));</example>
        public static IPrincipal Login(string user, string[] roles, DateTime expiration)
        {
            Logout();
            return CreateCookieTicket(user, roles, "", FormsAuthentication.FormsCookieName, expiration);
        }
        public static IPrincipal CreateCookieTicket(string user, string[] roles, string domain, string cookieName, DateTime expiration)
        {
            // ticket
            var ticket = CreateTicket(user, roles, expiration);

            // cookie
            var ticketString = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(cookieName, ticketString);
            cookie.Expires = expiration;
            cookie.Domain = domain;
            HttpContext.Current.Response.Cookies.Add(cookie);

            // current user
            HttpContext.Current.User = new UserRolePrincipal(new FormsIdentity(ticket), roles);
            return HttpContext.Current.User;
        }


        //-----------------------------------------------
        // 读取 Cookie 验票
        //-----------------------------------------------
        /// <summary>从cookie中读取验票并设置当前用户</summary>
        public static IPrincipal LoadPrincipalFromCookie()
        {
            // 获取鉴权Cookie值
            string cookieName = FormsAuthentication.FormsCookieName;
            string cookieValue = CookieHelper.FindCookie(cookieName);

            // 解析Cookie
            if (cookieValue.IsNotEmpty())
            {
                FormsAuthenticationTicket authTicket = ParseTicket(cookieValue, out string user, out string[] roles);
                HttpContext.Current.User = new UserRolePrincipal(new FormsIdentity(authTicket), roles);
                return HttpContext.Current.User;
            }
            return null;
        }



        //-----------------------------------------------
        // 注销处理
        //-----------------------------------------------
        /// <summary>注销。销毁验票</summary>
        public static void Logout()
        {
            FormsAuthentication.SignOut();
            ClearAuthCookie();
            HttpContext.Current.User = null;
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session.Abandon();
        }

        private static void ClearAuthCookie()
        {
            string cookieName = FormsAuthentication.FormsCookieName;
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie != null)
                cookie.Expires = System.DateTime.Now;
        }

        public static void RediretToLoginPage()
        {
            FormsAuthentication.RedirectToLoginPage();
        }


        //-----------------------------------------------
        // 验票字符串处理
        //-----------------------------------------------
        /// <summary>创建验票字符串</summary>
        /// <param name="user">用户名</param>
        /// <param name="roles">角色列表</param>
        /// <param name="expiration">过期时间</param>
        private static FormsAuthenticationTicket CreateTicket(string user, string[] roles, DateTime expiration)
        {
            // 将角色数组转化为字符串
            string userData = "";
            if (roles != null)
                foreach (string role in roles)
                    userData += role + ";";

            // 创建验票并加密之
            return new FormsAuthenticationTicket(
                1,                          // 版本
                user,                       // 用户名
                DateTime.Now,               // 创建时间
                expiration,                 // 到期时间
                false,                      // 非永久
                userData                    // 用户数据
                );
        }

        /// <summary>解析验票字符串，获取用户和角色信息</summary>
        /// <param name="ticket">验票字符串</param>
        /// <param name="user">用户名</param>
        /// <param name="roles">角色列表</param>
        /// <returns>表单验证票据对象</returns>
        private static FormsAuthenticationTicket ParseTicket(string ticketString, out string user, out string[] roles)
        {
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(ticketString);
            user = authTicket.Name;
            roles = authTicket.UserData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return authTicket;
        }

        /// <summary>当前登录用户的角色列表</summary>
        public static List<string> GetRoles()
        {
            var roles = new List<string>();
            if (IsLogin())
            {
                FormsAuthenticationTicket ticket = ((FormsIdentity)HttpContext.Current.User.Identity).Ticket;
                string userData = ticket.UserData;
                foreach (string role in userData.Split(','))
                {
                    if (!String.IsNullOrEmpty(role))
                        roles.Add(role);
                }
            }
            return roles;
        }
    }
}
