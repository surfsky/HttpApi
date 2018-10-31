using System;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Reflection;

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
        public bool IsReusable { get { return false; } }
        private const int _cacheMinutes = 600;

        // 处理 HttpApi 请求
        public void ProcessRequest(HttpContext context)
        {
             HandlerHttpApiRequest(context);
        }


        //----------------------------------------------
        // 解析和获取处理器
        //----------------------------------------------
        public static void HandlerHttpApiRequest(HttpContext context)
        {
            // 根据请求路径获取类型名：去掉扩展名；去前缀；用点运算符；
            // 获取处理器对象（从程序集创建或从缓存获取），处理web方法调用请求。
            var typeName = HttpApiHelper.GetRequestTypeName();
            var handler = TryGetHandlerFromCache(typeName);
            if (handler == null)
                handler = TryCreateHandlerFromAssemblies(typeName);
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