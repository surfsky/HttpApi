using System;
using System.Collections.Generic;
using System.Web;

namespace App.HttpApi
{
    /*
    <system.webServer>
      <modules>
        <add name="HttpApiModule" type="App.HttpApi.HttpApiModule" />
      </modules>
    /<system.webServer>
     * 
     */
    /// <summary>
    /// HttpApiModule
    /// </summary>
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
            var u = HttpContext.Current.Request.Url.AbsolutePath;
            var url = u.ToString().ToLower();
            var u2 = new App.Core.Url(url);

            if (url.Contains("httpapi/"))
            {
                // 以 /HttpApi/Type/Method 方式调用
                HttpContext.Current.RemapHandler(new HttpApiHandler());
            }
            /*
            else
            {
                // 以 Page.aspx/Method 或 Handler.ashx/Method 方式调用（未完成）
                int m = url.LastIndexOf(".");
                int n = url.LastIndexOf("/");
                if (m != -1 && n != -1 && m < n)
                {
                    url = url.Substring(0, n);
                    var ext = url.Substring(m);
                    var exts = new List<string>() { ".aspx", "ashx" };
                    if (exts.Contains(ext))
                        HttpContext.Current.RemapHandler(new HttpApiHandler()); // 指定处理器
                }
            }
            */
        }


        // 读取登录验票
        private void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //App.Core.AuthHelper.LoadPrincipalFromCookie();
        }
    }
}
