using System;
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
            string url = HttpContext.Current.Request.Url.ToString().ToLower();
            if (url.Contains("httpapi/"))
                HttpContext.Current.RemapHandler(new HttpApiHandler()); // 指定处理器
        }


        // 读取登录验票
        private void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //App.Core.AuthHelper.LoadPrincipalFromCookie();
        }
    }
}
