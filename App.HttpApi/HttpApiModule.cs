using System;
using System.Collections.Generic;
using System.Web;

namespace App.HttpApi
{
    /// <summary>
    /// HttpApiModule。
    /// </summary>
    /// <example>
    /// <system.webServer>
    ///   <modules>
    ///     <add name = "HttpApiModule" type="App.HttpApi.HttpApiModule" />
    ///   </modules>
    /// <system.webServer>
    /// </example>
    /// <remark>
    /// 抽空测试，访问xxxx.aspx/method 时若Content-Type:application/json，看是否会被aspnet自带webservice模块截获，无法调用服务器端方法。
    /// </remark>
    public class HttpApiModule : IHttpModule
    {
        public void Dispose(){}
        public void Init(HttpApplication application)
        {
            application.PostResolveRequestCache += Application_PostResolveRequestCache;
            application.AuthenticateRequest += Application_AuthenticateRequest;
        }


        // 指定HttpApi处理器去处理
        private void Application_PostResolveRequestCache(object sender, EventArgs e)
        {
            var uri = HttpContext.Current.Request.Url;
            var url = uri.AbsolutePath.ToString().ToLower();

            if (url.Contains("httpapi/"))
            {
                // 以 /HttpApi/Type/Method 方式调用
                HttpContext.Current.RemapHandler(new HttpApiHandler());
            }
            else
            {
                // 以 Page.aspx/Method 方式调用
                int m = url.LastIndexOf(".");
                int n = url.LastIndexOf("/");
                if (m != -1 && n != -1 && m < n)
                {
                    url = url.Substring(0, n);
                    var ext = url.Substring(m);
                    var exts = new List<string>() { ".aspx", ".ashx"};
                    if (exts.Contains(ext))
                        HttpContext.Current.RemapHandler(new HttpApiHandler());
                }
            }
        }

        // 读取登录验票
        private void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //App.Core.AuthHelper.LoadPrincipalFromCookie();
        }
    }
}
