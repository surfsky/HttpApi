using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

/*
HttpApplication 生命周期

01.对请求进行验证，将检查浏览器发送的信息，并确定其是否包含潜在恶意标记。有关更多信息，请参见 ValidateRequest 和脚本侵入概述。
02.如果已在 Web.config 文件的 UrlMappingsSection 节中配置了任何 URL，则执行 URL 映射。
03.引发 BeginRequest 事件。
04.引发 AuthenticateRequest 事件。
05.引发 PostAuthenticateRequest 事件。
06.引发 AuthorizeRequest 事件。
07.引发 PostAuthorizeRequest 事件。
08.引发 ResolveRequestCache 事件。
09.引发 PostResolveRequestCache 事件。<--- 可在此进行 HttpContext.Current.RemapHandler
10.根据所请求文件扩展名，选择实现 IHttpHandler 的类，对请求进行处理。
11.引发 PostMapRequestHandler 事件。
12.引发 AcquireRequestState 事件。    <--- 可获取Session信息
13.引发 PostAcquireRequestState 事件。
14.引发 PreRequestHandlerExecute 事件。
15.为该请求调用合适的 IHttpHandler 类的 ProcessRequest 方法（或异步版 BeginProcessRequest）。例如，如果该请求针对某页，则当前的页实例将处理该请求。
16.引发 PostRequestHandlerExecute 事件。
17.引发 ReleaseRequestState 事件。
18.引发 PostReleaseRequestState 事件。
19.如果定义了 Filter 属性，则执行响应筛选。
20.引发 UpdateRequestCache 事件。
21.引发 PostUpdateRequestCache 事件。
22.引发 EndRequest 事件。
*/
namespace App.HttpApi
{
    /// <summary>HttpApiModule</summary>
    /// <example>
    /// <system.webServer>
    ///   <modules>
    ///     <add name = "HttpApiModule" type="App.HttpApi.HttpApiModule" />
    ///   </modules>
    /// <system.webServer>
    /// </example>
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
