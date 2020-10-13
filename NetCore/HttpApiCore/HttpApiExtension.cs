using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
//using System.Web.SessionState;


namespace App.HttpApi
{
    /// <summary>
    /// HttpApi 扩展方法
    /// </summary>
    public static class HttpApiExtension
    {
        /// <summary>使用 HttpApi 中间件（请确保已使用 services.AddHttpContextAccessor())</summary>
        public static IApplicationBuilder UseHttpApi(this IApplicationBuilder app, Action<HttpApiConfig> options)
        {
            options(HttpApiConfig.Instance);
            var accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            Asp.Configure(accessor);
            return app.UseMiddleware<HttpApiMiddleware>();
        }
    }
}
