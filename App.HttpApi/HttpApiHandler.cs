using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

namespace App.HttpApi
{
    /// <summary>
    /// Web方法调用，可调用任何类（包括动态编译的）中标记了[HttpApi]特性标签的方法。
    /// </summary>
    /// <example>
    /// (1)注册: 
    ///     &lt;httpHandlers&gt;
    ///         &lt;add verb=&quot;*&quot; path=&quot;HttpApi-*.axd&quot; type=&quot;App.HttpApi.WebMethodCallerHandler&quot;/&gt;
    ///     &lt;/httpHandlers&gt;
    /// (2)编写类：
    ///     using System;
    ///     using System.Collections.Generic;
    ///     using System.Web;
    ///     using App.HttpApi;
    ///     namespace App
    ///     {
    ///         public class MyClass
    ///         {
    ///             [HttpApi(Type = ResponseDataType.Text)]
    ///             public string HelloWorld(string info)
    ///             {
    ///                 System.Threading.Thread.Sleep(200);
    ///                 return "hello world " + info;
    ///             }
    ///             
    ///             [HttpApi(Type = ResponseDataType.JSON)]
    ///             public static object GetStaticObject()
    ///             {
    ///                 return new { h = "3", a = "1", b = "2", c = "3" };
    ///             }
    ///         }
    ///     }
    /// (2)使用: 
    ///     查看js：  HttpApi.App.MyClass.axd/js
    ///     调用函数：HttpApi.App.MyClass.axd/HelloWorld?info=xxx
    /// </example>
    public class HttpApiHandler : IHttpHandler, IRequiresSessionState
    {
        HttpRequest Request    { get { return HttpContext.Current.Request; } }
        Cache Cache            { get { return HttpContext.Current.Cache; } }
        public bool IsReusable { get { return false; } }
        private const int _cacheMinutes = 2;

        //----------------------------------------------
        // 入口
        //----------------------------------------------
        // 处理 HttpApi 请求
        public void ProcessRequest(HttpContext context)
        {
            // 根据请求路径获取类型名：去掉扩展名；去前缀；用点运算符；
            string path = Request.FilePath;
            int n = path.LastIndexOf(".");
            path = path.Substring(0, n);
            if (path.StartsWith("/HttpApi.") || path.StartsWith("/HttpApi-") || path.StartsWith("/HttpApi_") || path.StartsWith("/HttpApi/"))
                path = path.Substring(9);
            var typeName = path.Replace('-', '.').Replace('_', '.');

            // 获取处理器对象（从程序集创建或从缓存获取），处理web方法调用请求。
            object handler = TryGetHandlerFromCache(typeName);
            if (handler == null)
                handler = TryCreateHandlerFromAssemblies(typeName);
            if (handler != null)
            {
                HttpApiHelper.ProcessRequest(context, handler);
                DisposeIfNeed(handler);
                return;
            }
        }


        //----------------------------------------------
        // 辅助方法
        //----------------------------------------------
        // 加到缓存中去（若实现了IDisposal接口，则保存assembly，否则保存对象）
        void SaveHandlerInCache(string typeName, Assembly assembly, object handler)
        {
            string cacheName = "HttpApi-" + typeName;
            object cacheObj = (handler is IDisposable) ? assembly : handler;
            Cache.Add(cacheName, cacheObj, 
                null,
                Cache.NoAbsoluteExpiration, new TimeSpan(0, _cacheMinutes, 0),
                CacheItemPriority.Default,
                null);
        }

        // 尝试从缓存中获取处理器
        object TryGetHandlerFromCache(string typeName)
        {
            string cacheName = "HttpApi-" + typeName;
            object o = Cache[cacheName];
            if (o == null)
                return null;
            else
            {
                if (o is Assembly)
                    return (o as Assembly).CreateInstance(typeName, true);
                else
                    return o;
            }
        }


        // 注意几个 ASP.NET 动态编译生成的程序集：
        // App_Web_*  : aspx页面生成的程序集
        // App_Code_* : app_code下面的代码生成的程序集
        /// <summary>尝试根据类型名称，从当前程序集中创建对象</summary>
        object TryCreateHandlerFromAssemblies(string typeName)
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
        private static void DisposeIfNeed(object handler)
        {
            if (handler is IDisposable)
                (handler as IDisposable).Dispose();
        }
    }
}