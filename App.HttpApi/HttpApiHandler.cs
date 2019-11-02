using System;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Reflection;
using System.Web.UI;
using System.Collections.Generic;

namespace App.HttpApi
{
    /// <summary>
    /// 页面请求处理器解析器。可获取页面请求对应的处理类。
    /// （抄的 Asp.net 源码）
    /// </summary>
    internal class WebHandlerParser : SimpleWebHandlerParser
    {
        private WebHandlerParser(HttpContext context, string virtualPath, string physicalPath)
            : base(context, virtualPath, physicalPath)
        {
        }
        internal static Type GetCompiledType(string virtualPath, string physicalPath, HttpContext context)
        {
            var parser = new WebHandlerParser(context, virtualPath, physicalPath);
            return parser.GetCompiledTypeFromCache();
        }
        protected override string DefaultDirectiveName
        {
            get { return "webhandler"; }
        }
    }

    /// <summary>
    /// Web方法调用，可调用任何类（包括动态编译的）中标记了[HttpApi]特性标签的方法。
    /// </summary>
    /// <example>
    /// (1)注册 HttpApiModule: 
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
    ///     查看js：  HttpApi/App.MyClass/js
    ///     查看api： HttpApi/App.MyClass/api
    ///     调用函数：HttpApi/App.MyClass/HelloWorld?info=xxx
    /// </example>
    public class HttpApiHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }
        private const int _cacheMinutes = 600;

        // 处理 HttpApi 请求
        public void ProcessRequest(HttpContext context)
        {
            //HttpContext.Current.Session["HttpApiSession"] = "HelloWorld";
            HandlerHttpApiRequest(context);
        }


        //----------------------------------------------
        // 解析和获取处理器
        //----------------------------------------------
        public static void HandlerHttpApiRequest(HttpContext context)
        {
            var req = context.Request;
            var uri = req.Url;
            var url = uri.AbsolutePath.ToLower();
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
            else
            {
                // 以 Page.aspx/Method 方式调用
                int n = url.LastIndexOf("/");
                url = url.Substring(0, n);
                Type type = WebHandlerParser.GetCompiledType(url, url, context);
                handler = Activator.CreateInstance(type);
            }


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
            HttpContext.Current.Cache.Add(cacheName, cacheObj,
                null,
                Cache.NoAbsoluteExpiration,
                new TimeSpan(0, _cacheMinutes, 0),
                CacheItemPriority.Default,
                null);
        }

        // 尝试从缓存中获取处理器
        static object TryGetHandlerFromCache(string typeName)
        {
            string cacheName = "HttpApi-" + typeName;
            object o = HttpContext.Current.Cache[cacheName];
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