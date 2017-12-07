﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Reflection;
using System.Text.RegularExpressions;
using App.HttpApi;

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
        HttpRequest Request { get { return HttpContext.Current.Request; } }
        Cache Cache{ get { return HttpContext.Current.Cache; } }
        public bool IsReusable { get { return false; } }
        private const int _cacheMinutes = 2;

        // 处理这种方式的调用
        public void ProcessRequest(HttpContext context)
        {
            string path = Request.FilePath;

            // 去掉扩展名
            int n = path.LastIndexOf(".");
            path = path.Substring(0, n);

            // 去掉前缀
            string pre = "/HttpApi.";
            if (path.StartsWith("/HttpApi.") || path.StartsWith("/HttpApi-") || path.StartsWith("/HttpApi_"))
                path = path.Substring(pre.Length);

            // 调用
            Call(path, context);
        }

        // 尝试遍历程序集创建对象，并处理web方法调用请求。
        // 注意几个 ASP.NET 动态编译生成的程序集：
        // App_Web_*  : aspx页面生成的程序集
        // App_Code_* : app_code下面的代码生成的程序集
        private void Call(string typeName, HttpContext context)
        {
            // 尝试从缓存中恢复对象并处理请求
            typeName = typeName.Replace('-', '.').Replace('_', '.');
            object handler = TryGetHandlerFromCache(typeName);
            if (handler != null)
            {
                App.HttpApi.HttpApiHelper.ProcessRequest(context, handler);
                DisposeIfNeed(handler);
                return;
            }

            // 找不到着遍历程序集去找这个类
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                // 过滤掉系统自带的程序集
                string name = assembly.FullName;
                if (name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib"))
                    continue;

                // 尝试创建对象，且处理Web方法调用请求
                handler = assembly.CreateInstance(typeName, true);
                if (handler != null)
                {
                    App.HttpApi.HttpApiHelper.ProcessRequest(context, handler);
                    DisposeIfNeed(handler);
                    SaveHandlerInCache(typeName, assembly, handler);
                    break;
                }
            }
        }

        //----------------------------------------------
        // 缓存处理
        //----------------------------------------------
        // 如果对象实现了IDisposal接口，则马上释放资源
        private static void DisposeIfNeed(object handler)
        {
            if (handler is IDisposable)
                (handler as IDisposable).Dispose();
        }

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
        private object TryGetHandlerFromCache(string typeName)
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


    }
}