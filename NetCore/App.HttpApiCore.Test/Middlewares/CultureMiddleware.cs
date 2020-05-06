using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace App.HttpApiCore
{
    /// <summary>
    /// 语言中间件注册
    /// </summary>
    public static class RequestCultureMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestCulture(this IApplicationBuilder app)
        {
            //return app.UseMiddleware<RequestCultureMiddleware>();
            CultureHandler(app);
            return app;
        }

        // 根据culture参数切换页面语言
        public static void CultureHandler(IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                var cultureQuery = context.Request.Query["culture"];
                if (!string.IsNullOrWhiteSpace(cultureQuery))
                {
                    var culture = new CultureInfo(cultureQuery);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                }
                // Call the next delegate/middleware in the pipeline
                return next();
            });
        }
    }

    /// <summary>
    /// 语言中间件
    /// </summary>
    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var cultureQuery = context.Request.Query["culture"];
            if (!string.IsNullOrWhiteSpace(cultureQuery))
            {
                var culture = new CultureInfo(cultureQuery);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            // Call the next delegate/middleware in the pipeline
            return this._next(context);
        }
    }

    /// <summary>
    /// 浏览器验证
    /// </summary>
    public class ValidateBrowserMiddleware
    {
        private readonly RequestDelegate _next;
        public ValidateBrowserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Headers["User-Agent"].Any(h => h.ToLower().Contains("trident")))
                httpContext.Response.StatusCode = 403;
            else
                await _next.Invoke(httpContext);
        }
    }

    /// <summary>
    /// 中间件基类
    /// </summary>
    public class BaseMiddleware
    {
        protected readonly RequestDelegate _next;
        public BaseMiddleware() { }
        public BaseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        protected virtual void Process(HttpContext httpContext) { }
        public async Task Invoke(HttpContext httpContext)
        {
            Process(httpContext);
        }
    }

    public class BrowerMiddleware : BaseMiddleware
    {
        protected override void Process(HttpContext httpContext)
        {
            var isie = httpContext.Request.Headers["User-Agent"].Any(v => v.ToLower().Contains("trident"));
            httpContext.Items["IEBrowser"] = isie;
            _next.Invoke(httpContext);
        }
    }

    /*
    // 传值
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;
        public MyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IMyScopedService svc)
        {
            svc.MyProperty = 1000;
            await _next(httpContext);
        }
    }
    */


    /// <summary>
    /// 使用 IMiddleware 接口的中间件
    /// 使用IMiddleware类型的中间件需要在容器中进行注册，否则抛异常
    /// </summary>
    /// <example>
    /// services.AddSingleton<MyMiddleware>()
    /// 
    /// app.UseMyMiddleware();
    /// </example>
    public class MyMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
        }
    }
    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MyMiddleware>();
        }
    }

}