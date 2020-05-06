using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
//using System.Web.SessionState;


namespace App.HttpApi
{
    public static class HttpApiExtension
    {
        public static IApplicationBuilder UseHttpApi(this IApplicationBuilder app, IHttpContextAccessor accessor)
        {
            //return app.UseMiddleware<RequestCultureMiddleware>();
            //CultureHandler(app);
            //return app;
            Asp.Configure(accessor);
            return app.UseMiddleware<HttpApiMiddleware>();
        }
    }


    /// <summary>HttpApiModule</summary>
    /// <example>
    /// app.UseMiddleware<HttpApiMiddleware>();
    /// </example>
    public class HttpApiMiddleware
    {
        private const int _cacheMinutes = 600;
        private static MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        private readonly RequestDelegate _next;
        public HttpApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var url = httpContext.Request.Path.Value.ToLower();
            //var url = uri.AbsolutePath.ToString().ToLower();

            if (url.Contains("httpapi/"))
            {
                // 以 /HttpApi/Type/Method 方式调用
                HandlerHttpApiRequest(httpContext);
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
                    var exts = new List<string>() { ".aspx", ".ashx" };
                    if (exts.Contains(ext))
                        HandlerHttpApiRequest(httpContext);
                }
            }

            await _next.Invoke(httpContext);
        }

        //----------------------------------------------
        // 解析和获取处理器
        //----------------------------------------------
        public static void HandlerHttpApiRequest(HttpContext context)
        {
            var req = context.Request;
            var url = req.Path.Value.ToLower();
            object handler = null;

            // 获取处理器对象
            if (url.StartsWith("/httpapi/"))
            {
                // 以 /HttpApi/Type/Method 方式调用
                var typeName = HttpApiHelper.GetRequestTypeName();
                handler = TryGetHandlerFromCache(typeName);
                if (handler == null)
                    handler = TryCreateHandlerFromAssemblies(typeName);
            }
            /*
            else
            {
                // 以 Page.aspx/Method 方式调用
                int n = url.LastIndexOf("/");
                url = url.Substring(0, n);
                Type type = BuildManager.GetCompiledType(url);
                handler = Activator.CreateInstance(type);
            }
            */


            // 调用处理器方法
            if (handler != null)
            {
                HttpApiHelper.ProcessRequest(context, handler);
                DisposeIfNeed(handler);
            }
        }

        // 加到缓存中去（若实现了IDisposal接口，则保存assembly，否则保存对象）
        static void SaveHandlerInCache(string typeName, Assembly assembly, object handler)
        {
            string cacheName = "HttpApi-" + typeName;
            object cacheObj = (handler is IDisposable) ? assembly : handler;
            Cache.Set(
                cacheName, 
                cacheObj, 
                new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(_cacheMinutes)}
            );
        }

        // 尝试从缓存中获取处理器
        static object TryGetHandlerFromCache(string typeName)
        {
            string cacheName = "HttpApi-" + typeName;
            if (Cache.TryGetValue(cacheName, out object o))
            {
                if (o is Assembly)
                    return (o as Assembly).CreateInstance(typeName, true);
                else
                    return o;
            }
            return null;
        }



        // 注意几个 ASP.NET 动态编译生成的程序集：
        // App_Web_*  : aspx页面生成的程序集
        // App_Code_* : app_code下面的代码生成的程序集
        /// <summary>尝试根据类型名称，从当前程序集中创建对象</summary>
        static object TryCreateHandlerFromAssemblies(string typeName)
        {
            // 遍历程序集去找这个类
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                // 过滤掉系统自带的程序集
                string name = assembly.FullName;
                if (name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib"))
                    continue;

                // 尝试创建对象
                var obj = assembly.CreateInstance(typeName, true);
                if (obj != null)
                {
                    SaveHandlerInCache(typeName, assembly, obj);
                    return obj;
                }
            }
            return null;
        }

        // 如果对象实现了IDisposable接口，则马上释放资源
        static void DisposeIfNeed(object handler)
        {
            if (handler is IDisposable)
                (handler as IDisposable).Dispose();
        }

    }
}
