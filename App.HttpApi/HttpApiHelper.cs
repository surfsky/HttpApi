using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Web.SessionState;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using App.Components;

namespace App.HttpApi
{
    /// <summary>
    /// HttpApi 的逻辑实现。
    /// </summary>
    internal class HttpApiHelper
    {
        //----------------------------------------------
        //----------------------------------------------
        /// <summary>处理 Web 方法调用请求</summary>
        /// <param name="context">Http上下文</param>
        /// <param name="handler">任何使用了[HttpApi]特性标签的对象，如Page、HttpHandler</param>
        public static void ProcessRequest(HttpContext context, object handler)
        {
            // 解析方法名和参数
            RequestDecoder decoder = RequestDecoder.CreateInstance(context);
            string methodName = decoder.MethodName;
            Dictionary<string, object> args = decoder.ParseArguments();

            // 获取类型和方法信息
            // 对于aspx页面，执行时编译出来的是类似ASP.xxx_aspx的类，这不是我们想要处理的类，追溯处理父类(IMPORTANT!)
            Type type = handler.GetType();
            if (type.FullName.StartsWith("ASP.") && type.BaseType != null)
                type = type.BaseType;
            MethodInfo method = ReflectHelper.GetMethod(type, methodName);
            HttpApiAttribute attr = ReflectHelper.GetHttpApiAttribute(method);

            // 找到预留方法直接处理掉
            // 检测方法可用性
            if (ProcessReservedMethod(context, type, methodName, args))
                return;
            if (!CheckMethodEnable(context, method, methodName, attr))
                return;

            // 普通方法调用
            try
            {
                // 获取需要的参数
                object[] parameters = ReflectHelper.GetParameters(method, args);
                string cacheKey = string.Format("{0}-{1}-{2}", handler, method.Name, SerializeHelper.ToJson(parameters));
                        
                // 获取方法调用结果并依情况缓存
                object result = (attr.CacheSeconds == 0) 
                    ? method.Invoke(handler, parameters)
                    : CacheHelper.GetCachedObject<object>(
                        cacheKey, 
                        DateTime.Now.AddSeconds(attr.CacheSeconds), 
                        () =>{return method.Invoke(handler, parameters);}
                        )
                    ;

                // 输出结果
                ResponseType dataType = attr.Type;
                if (args.ContainsKey("_type"))
                    dataType = (ResponseType)Enum.Parse(typeof(ResponseType), args["_type"].ToString(), true);
                if (dataType == ResponseType.Auto && result != null)
                    dataType = ParseDataType(result.GetType());
                var wrap = HttpApiConfig.Instance.Wrap ?? attr.Wrap;
                var wrapInfo = attr.WrapCondition;
                WriteResult(context, result, dataType, attr.MimeType, attr.FileName, attr.CacheSeconds, attr.CacheLocation, wrap, wrapInfo);
            }
            catch (Exception ex)
            {
                string result = string.Format("Api {0}() fail. Please check parameters. {1}", methodName, ex.Message);
                WriteError(context, 400, result);
            }
        }

        // 预处理保留方法（"js", "jq", "ext", "api", "apis"）
        static bool ProcessReservedMethod(HttpContext context, Type type, string methodName, Dictionary<string, object> args)
        {
            string methodNameLower = methodName.ToLower();
            int cacheDuration = ReflectHelper.GetCacheDuration(type);

            // 输出api接口展示页面（方法名后面跟了个_)
            if (methodNameLower.LastIndexOf("_") != -1)
            {
                var name = methodName.Substring(0, methodName.Length - 1);
                var typeapi = GetTypeApi(type);
                var api = FindApi(typeapi, name);
                var result = GetApiHtml(api);
                context.Response.Clear();
                context.Response.ContentType = "text/html";
                context.Response.Write(result.ToString());
            }
            else
            {
                // 保留方法名
                string[] arr = { "js", "jq", "ext", "api", "apis" };
                if (!((IList<string>)arr).Contains(methodNameLower))
                    return false;

                // 输出api接口页面
                if (methodNameLower == "api")
                {
                    var typeapi = GetTypeApi(type);
                    var result = GetTypeApiHtml(typeapi);
                    context.Response.Clear();
                    context.Response.ContentType = "text/html";
                    context.Response.Write(result.ToString());
                }
                // 输出api接口清单
                else if (methodNameLower == "apis")
                {
                    var typeapi = GetTypeApi(type);
                    context.Response.Clear();
                    WriteResult(context, typeapi, ResponseType.JSON);
                }
                // 输出js/jquery/ext脚本
                else
                {
                    string nameSpace = GetJsNamespace(type, args);
                    string className = GetJsClassName(type, args);
                    StringBuilder result = GetJs(type, nameSpace, className, cacheDuration, methodNameLower);
                    WriteResult(context, result, ResponseType.JavaScript);
                }
            }

            // 处理缓存
            CacheHelper.SetCachePolicy(context, cacheDuration);
            return true;
        }

        // 查找接口方法对象
        private static API FindApi(TypeAPI typeapi, string methodName)
        {
            foreach (var api in typeapi.Apis)
            {
                if (api.Name == methodName)
                    return api;
            }
            return null;
        }

        // 检测方法的可用性
        static bool CheckMethodEnable(HttpContext context, MethodInfo method, string methodName, HttpApiAttribute attr)
        {
            // 方法未找到或未公开
            if (method == null || attr == null)
            {
                object result = "Function " + methodName + " not found. Please check the [HttpApi] attribute.";
                WriteError(context, 404, result.ToString());
                return false;
            }

            // 校验访问方式
            if (!attr.Verbs.IsNullOrEmpty())
            {
                string[] verbs = attr.Verbs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (verbs == null || verbs.Length == 0)
                    return true;
                else
                {
                    string verb = context.Request.HttpMethod.ToLower();
                    if (!verbs.ToLower().Contains(verb))
                    {
                        WriteError(context, 400, "Allowed verbs: " + attr.Verbs);
                        return false;
                    }
                }
            }

            // 校验登录与否
            if (attr.AuthLogin)
            {
                if (!Asp.IsLogin())
                {
                    WriteError(context, 401, "Login auth fail");
                    return false;
                }
            }

            // 校验用户
            if (!string.IsNullOrEmpty(attr.AuthUsers))
            {
                if (!Asp.IsInUsers(attr.AuthUsers.Split(',', ';')))
                {
                    WriteError(context, 401, "User auth fail");
                    return false;
                }
            }

            // 校验角色
            if (!string.IsNullOrEmpty(attr.AuthRoles))
            {
                if (!Asp.IsInRoles(attr.AuthRoles.Split(',', ';')))
                {
                    WriteError(context, 401, "Role auth fail");
                    return false;
                }
            }

            return true;
        }


        //-----------------------------------------------------------
        // 输出调用结果
        //-----------------------------------------------------------
        // 输出数据
        static void WriteResult(
            HttpContext context, object result, 
            ResponseType dataType, string mimeType = null, string fileName = null, 
            int cacheSeconds = 0, HttpCacheability cacheLocation = HttpCacheability.NoCache, 
            bool wrap = false, string wrapInfo = null
            )
        {
            // 是否需要做 DataResult JSON 封装
            if (wrap && (dataType == ResponseType.JSON || dataType == ResponseType.ImageBase64 || dataType == ResponseType.Text || dataType == ResponseType.XML))
            {
                result = new DataResult("true", wrapInfo, result, null);
                if (dataType != ResponseType.XML)
                    dataType = ResponseType.JSON;
            }

            var encoder = new ResponseEncoder(dataType, mimeType, fileName, cacheSeconds, cacheLocation);
            encoder.Write(result);
        }


        // 输出错误（根据AppSettings）
        // 若为HttpError，浏览器会跳转到对应的错误页面
        // 若为DataResult，直接输出DataResult错误信息（默认）
        static void WriteError(HttpContext context, int errorCode, string info)
        {
            ErrorResponse errorResponse = HttpApiConfig.Instance.ErrorResponse;
            if (errorResponse ==  ErrorResponse.HttpError)
            {
                context.Response.StatusCode = errorCode;
                context.Response.StatusDescription = info;
                context.Response.End();
            }
            else
            {
                DataResult dr = new DataResult("false", info, errorCode, null);
                WriteResult(context, dr, ResponseType.JSON);
            }
        }



        // 推断输出类型（非 string, image 对象，默认类型为 json）
        static ResponseType ParseDataType(Type t)
        {
            if (IsType(t, typeof(string)))                return ResponseType.Text;
            if (IsType(t, typeof(StringBuilder)))         return ResponseType.Text;
            if (IsType(t, typeof(DateTime)))              return ResponseType.Text;
            if (IsType(t, typeof(System.Drawing.Image)))  return ResponseType.Image;
            if (IsType(t, typeof(System.Data.DataTable))) return ResponseType.JSON;
            if (IsType(t, typeof(System.Data.DataRow)))   return ResponseType.JSON;
            return ResponseType.JSON;
        }
        static ResponseType ParseDataType(ResponseType attrType, Type returnType)
        {
            if (attrType != ResponseType.Auto) 
                return attrType;
            return ParseDataType(returnType);
        }
        static bool IsType(Type raw, Type match)
        {
            return (raw == match) ? true : raw.IsSubclassOf(match);
        }



        //-----------------------------------------------------------
        // 获取接口清单
        //-----------------------------------------------------------
        // 获取接口清单
        static TypeAPI GetTypeApi(Type type)
        {
            var typeapi = new TypeAPI();
            var uri = HttpContext.Current.Request.Url;
            var filePath = HttpContext.Current.Request.FilePath;
            var rootUrl = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, filePath);

            // 获取接口列表
            var apis = new List<API>();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                HttpApiAttribute attr = ReflectHelper.GetHttpApiAttribute(method);
                if (attr != null)
                {
                    var api = new API()
                    {
                        Name = method.Name,
                        Description = attr.Description,
                        ReturnType = ParseDataType(attr.Type, method.ReturnType).ToString(),
                        CacheDuration = attr.CacheSeconds,
                        AuthLogin = attr.AuthLogin,
                        AuthUsers = attr.AuthUsers,
                        AuthRoles = attr.AuthRoles,
                        Verbs = attr.Verbs.IsNullOrEmpty() ? "" : attr.Verbs.ToUpper(),
                        Status = attr.Status,
                        Remark = attr.Remark,
                        Url = GetMethodDisplayUrl(rootUrl, method),
                        UrlTest = GetMethodTestUrl(rootUrl, method),
                        Params = GetMethodParams(method),
                        Method = method,
                        RType = attr.Type
                    };
                    apis.Add(api);
                }
            }
            

            //
            typeapi.Apis = apis.OrderBy(t => t.Name).ToList();
            typeapi.Description = ReflectHelper.GetDescription(type);
            typeapi.Histories = ReflectHelper.GetHistories(type);
            return typeapi;
        }

        private static List<ParamAttribute> GetMethodParams(MethodInfo method)
        {
            var items = new List<ParamAttribute>();
            var attrs = ReflectHelper.GetParamAttributes(method);
            var paras = method.GetParameters();
            foreach (var p in paras)
            {
                var attr = attrs.AsQueryable().FirstOrDefault(t => t.Name == p.Name);
                string desc = attr?.Description;
                items.Add(new ParamAttribute(
                    p.Name,
                    desc,
                    p.ParameterType.ToString(),
                    GetTypeInfo(p.ParameterType),
                    p.DefaultValue?.ToString()
                    ));
            }
            return items;
        }

        // 获取类型的概述信息
        private static string GetTypeInfo(Type type)
        {
            var sb = new StringBuilder();
            if (type.IsEnum)
            {
                foreach (var item in Enum.GetValues(type))
                {
                    sb.AppendFormat("{0}-{1}({2}); ", (int)item, item.ToString(), item.GetDescription());
                }
            }
            return sb.ToString();
        }



        /// <summary>
        /// 构造接口清单页面
        /// </summary>
        static StringBuilder GetTypeApiHtml(TypeAPI typeapi)
        {
            // 概述信息
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<h1>" + typeapi.Description + "</h1>");
            foreach (HistoryAttribute history in typeapi.Histories)
            {
                sb.AppendFormat("<small>{0}, {1}, {2}</small><br/>", history.Date, history.User, history.Info);
            }

            // 接口清单
            sb.AppendLine("<br/><table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendLine("<tr><td width='200'>接口名</td><td width='200'>说明</td><td width='70'>类型</td><td width='70'>缓存(秒)</td><td width='70'>限登录</td><td width='70'>限用户</td><td width='70'>限角色</td><td width='100'>访问方式</td><td width='100'>状态</td><td width='100'>备注</td><td>详情</td></tr>");
            foreach (var api in typeapi.Apis)
            {
                sb.AppendFormat("<tr><td>{0}&nbsp;</td><td>{1}&nbsp;</td><td>{2}&nbsp;</td><td>{3}&nbsp;</td><td>{4}&nbsp;</td><td>{5}&nbsp;</td><td>{6}&nbsp;</td><td>{7}&nbsp;</td><td>{8}&nbsp;</td><td>{9}&nbsp;</td><td><a target='_blank' href='{10}'>详情</a>&nbsp;</td></tr>"
                    , api.Name
                    , api.Description
                    , api.ReturnType
                    , api.CacheDuration
                    , api.AuthLogin
                    , api.AuthUsers
                    , api.AuthRoles
                    , api.Verbs
                    , api.Status
                    , api.Remark
                    , api.Url
                    );
            }
            return sb;
        }


        /// <summary>
        /// 构造接口清单页面
        /// </summary>
        static StringBuilder GetApiHtml(API api)
        {
            // 概述信息
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<h1>" + api.Name + "</h1>");
            sb.AppendLine("<h3>" + api.Description + "</h3>");

            // 属性
            sb.AppendLine("<h2>属性</h2>");
            sb.AppendLine("<br/><table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width='100'>名称    </td><td>值</td></tr>");
            sb.AppendFormat("<tr><td width='100'>返回类型</td><td>{0}&nbsp;</td></tr>", api.ReturnType);
            sb.AppendFormat("<tr><td width='100'>缓存(秒)</td><td>{0}&nbsp;</td></tr>", api.CacheDuration);
            sb.AppendFormat("<tr><td width='100'>限登陆  </td><td>{0}&nbsp;</td></tr>", api.AuthLogin);
            sb.AppendFormat("<tr><td width='100'>限用户  </td><td>{0}&nbsp;</td></tr>", api.AuthUsers);
            sb.AppendFormat("<tr><td width='100'>限角色  </td><td>{0}&nbsp;</td></tr>", api.AuthRoles);
            sb.AppendFormat("<tr><td width='100'>访问方式</td><td>{0}&nbsp;</td></tr>", api.Verbs);
            sb.AppendFormat("<tr><td width='100'>状态    </td><td>{0}&nbsp;</td></tr>", api.Status);
            sb.AppendFormat("<tr><td width='100'>备注    </td><td>{0}&nbsp;</td></tr>", api.Remark);
            sb.AppendFormat("<tr><td width='100'>测试URL </td><td>{0}&nbsp;</td></tr>", api.UrlTest);
            sb.AppendLine("</tr></table>");

            // 参数
            sb.AppendLine("<h2>参数</h2>");
            sb.AppendLine("<br/><table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width='100'>参数名</td><td>描述</td><td>类型</td><td>说明</td><td>缺省值</td></tr>");
            foreach (var p in api.Params)
            {
                sb.AppendFormat("<tr><td>{0}&nbsp;</td><td>{1}&nbsp;</td><td>{2}&nbsp;</td><td>{3}&nbsp;</td><td>{4}&nbsp;</td></tr>"
                    ,p.Name
                    ,p.Description
                    ,p.Type
                    ,p.Info
                    ,p.DefaultValue
                    );
            }
            sb.AppendLine("</tr></table>");
            return sb;
        }


        //-----------------------------------------------------------
        // 生成客户端可直接调用的 javascript 脚本
        //-----------------------------------------------------------
        // 获取json.js文件
        static string GetJsonScript()
        {
            return ResourceHelper.GetResourceText(
                Assembly.GetExecutingAssembly(),
                typeof(HttpApiHelper).Namespace + ".Js.json2.min.js"
                );
        }

        // 取得嵌入的脚本模版(js,jq,ext)
        static string GetTemplateScript(string scriptType)
        {
            return ResourceHelper.GetResourceText(
                Assembly.GetExecutingAssembly(),
                typeof(HttpApiHelper).Namespace + ".Js." + scriptType + "Template.js"
                );
        }

        // 获取js的封装的namespace
        // （1）先尝试获取输入参数 dataNamespace
        // （2）再尝试获取对象类的 WebMethodNamespace 特性
        // （3）默认返回对象类的 Namespace
        private static string GetJsNamespace(Type type, Dictionary<string, object> args)
        {
            if (args.ContainsKey("dataNamespace"))
                return args["dataNamespace"].ToString();
            else
            {
                ScriptAttribute attr = ReflectHelper.GetScriptAttribute(type);
                return (attr != null && attr.NameSpace != null) ? attr.NameSpace : type.Namespace;
            }
        }

        // 获取js的封装的className
        private static string GetJsClassName(Type type, Dictionary<string, object> args)
        {
            if (args.ContainsKey("dataClassName"))
                return args["dataClassName"].ToString();
            else
            {
                ScriptAttribute attr = ReflectHelper.GetScriptAttribute(type);
                return (attr != null && attr.ClassName != null) ? attr.ClassName : type.Name;
            }
        }


        /// <summary>
        /// 注册Js对应的Namespace（请重载ProcessRequest中调用该函数）
        /// 太麻烦了，写webconfig吧，以后再说，先移除
        /// </summary>
        /// <example>
        ///   public override void ProcessRequest(HttpContext context)
        ///   {
        ///       RegistJsNamespace("MyNamespace", "MyClass");
        ///       base.ProcessRequest(context);
        ///   }
        /// </example>
        public void RegistJsType(string nameSpace, string className)
        {
            HttpContext.Current.Request.Params["dataNamespace"] = nameSpace;
            HttpContext.Current.Request.Params["dataClassName"] = className;
        }


        /// <summary>
        /// 生成客户端调用服务器端方法的脚本
        /// </summary>
        static StringBuilder GetJs(Type type, string nameSpace, string className, int cacheDuration, string scriptType="js")
        {
            // 读取对应的模板
            string script = GetTemplateScript(scriptType);
            Uri uri = HttpContext.Current.Request.Url;
            string filePath = HttpContext.Current.Request.FilePath;

            // 并进行字符串替换：描述、时间、地址、类名
            string url = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, filePath);
            script = script.Replace("%DATE%", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            script = script.Replace("%DURATION%", cacheDuration.ToString());
            script = script.Replace("%URL%", url);
            script = script.Replace("%NS-BUILD%", GetNamespaceScript(nameSpace));
            script = script.Replace("%NS%", nameSpace);
            script = script.Replace("%CLS%", className);

            // 依次生成函数调用脚本
            var typeapi = GetTypeApi(type);
            StringBuilder scriptBuilder = new StringBuilder(script);
            foreach (var api in typeapi.Apis)
            {
                scriptBuilder.AppendLine("//-----------------------------------------------------------------");
                scriptBuilder.AppendLine("// 说明  : " + api.Description);
                scriptBuilder.AppendLine("// 地址  : " + api.UrlTest);
                scriptBuilder.AppendLine("// 缓存  : " + api.CacheDuration.ToString() + " 秒");
                scriptBuilder.AppendLine("// 类型  : " + api.ReturnType);
                scriptBuilder.AppendLine("// 备注  : " + api.Remark);
                scriptBuilder.AppendLine("// 限登录: " + api.AuthLogin);
                scriptBuilder.AppendLine("// 限用户: " + api.AuthUsers);
                scriptBuilder.AppendLine("// 限角色: " + api.AuthRoles);
                scriptBuilder.AppendLine("//-----------------------------------------------------------------");

                string func = GetFunctionScript(nameSpace, className, api.Method, api.RType);
                scriptBuilder.AppendLine(func);
            }

            // 插入json2.js并输出
            scriptBuilder.Insert(0, GetJsonScript());
            return scriptBuilder;
        }




        // 构造namespace创建语句
        static string GetNamespaceScript(string ns)
        {
            StringBuilder sb = new StringBuilder();
            string[] parts = ns.Split('.');
            string ns2 = "";
            foreach (string part in parts)
            {
                ns2 += (ns2 == "") ? part : "." + part;
                sb.AppendFormat("\r\nif (typeof({0}) == 'undefined') {0}={{}};", ns2);
            }
            return sb.ToString();
        }

        // 获取API方法展示地址
        static string GetMethodDisplayUrl(string rootUrl, MethodInfo method)
        {
            return string.Format("{0}/{1}_", rootUrl, method.Name);
        }

        // 获取API方法测试地址
        static string GetMethodTestUrl(string rootUrl, MethodInfo method)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}/{1}", rootUrl, method.Name);
            ParameterInfo[] ps = method.GetParameters();
            if (ps.Length > 0)
            {
                sb.Append("?");
                foreach (ParameterInfo p in ps)
                    sb.Append(p.Name + "=x&");
            }
            return sb.ToString().TrimEnd('&');
        }

        /// <summary>
        /// 取得函数调用代码段
        /// </summary>
        static string GetFunctionScript(string nameSpace, string className, MethodInfo method, ResponseType format)
        {
            StringBuilder sb = new StringBuilder();

            // 函数名和参数
            sb.AppendFormat("{0}.{1}.{2}", nameSpace, className, method.Name);
            sb.Append(" = function(");
            foreach (ParameterInfo p in method.GetParameters())
                sb.Append(p.Name + ", ");
            sb.Append("callback, senderId)");

            // 使用jquery调用服务器端的函数
            sb.AppendLine("{");
            sb.Append("    var args = {");
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                string item = (i == 0) ? p.Name + ":" + p.Name : ", " + p.Name + ":" + p.Name;
                sb.Append(item);
            }
            sb.AppendLine("};");
            switch (format)
            {
                case ResponseType.HTML:       sb.AppendLine("    var options = {dataType:'html'};");    break;
                case ResponseType.XML:        sb.AppendLine("    var options = {dataType:'xml'};");     break;
                case ResponseType.JSON:       sb.AppendLine("    var options = {dataType:'json'};");    break;
                case ResponseType.JavaScript: sb.AppendLine("    var options = {dataType:'script'};");  break;
                case ResponseType.Text:       sb.AppendLine("    var options = {dataType:'text'};");    break;
                default:                          sb.AppendLine("    var options = {dataType:'text'};");    break;
            }
            sb.AppendFormat("    return this.CallWebMethod('{0}', args, options, callback, senderId);\r\n", method.Name);
            sb.AppendLine("}");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
