using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.Text;
using System.Linq;
using App.HttpApi.Components;

namespace App.HttpApi
{
    /// <summary>
    /// HttpApi 异常
    /// </summary>
    public class HttpApiException : Exception
    {
        public int Code { get; set; }
        public HttpApiException(int code, string message)
            : base(message)
        {
            this.Code = code;
        }
    }



    /// <summary>
    /// HttpApi 的逻辑实现。
    /// </summary>
    public partial class HttpApiHelper
    {
        //----------------------------------------------
        // 入口
        //----------------------------------------------
        /// <summary>处理 Web 方法调用请求</summary>
        public static void ProcessRequest(HttpContext context, object handler)
        {
            // 解析类型、方法、参数等信息
            // 对于aspx页面，执行时编译出来的是类似ASP.xxx_aspx的类，这不是我们想要处理的类，追溯处理父类(IMPORTANT!)
            var decoder = RequestDecoder.CreateInstance(context);
            var args = decoder.ParseArguments();
            var type = handler.GetType();
            if (type.FullName.StartsWith("ASP.") && type.BaseType != null)
                type = type.BaseType;
            var methodName = decoder.MethodName;
            var method = ReflectHelper.GetMethod(type, methodName);
            var attr = ReflectHelper.GetHttpApiAttribute(method);

            // 流量限制
            var ip = Asp.GetClientIP();
            var url = context.Request.Url.AbsolutePath.ToLower();
            System.Diagnostics.Trace.WriteLine($"IP={ip}, URL={url}");
            if (IPFilter.IsBanned(ip))
            {
                HttpContext.Current.Request.Abort();
                return;
            }
            if (attr != null && attr.AuthTraffic > 0)
            {
                if (VisitCounter.IsHeavy(ip, url, 10, attr.AuthTraffic * 10))  // 每10秒为一个周期
                {
                    IPFilter.Ban(ip, HttpApiConfig.Instance.BanMinutes);
                    HttpApiConfig.Instance.DoBan(ip, url);
                    return;
                }
            }

            // 处理预留方法
            if (ProcessReservedMethod(context, type, methodName, args))
                return;

            // 普通方法调用
            try
            {
                // 检测方法可用性
                // 获取需要的参数
                CheckMethod(context, method, attr, args);
                var parameters = ReflectHelper.GetParameters(method, args);

                // 获取方法调用结果并依情况缓存
                object result;
                if (attr.CacheSeconds == 0)
                    result = method.Invoke(handler, parameters);
                else
                {
                    var p = SerializeHelper.ToJson(parameters).ClearSpace().TrimStart('[').TrimEnd(']');//.Replace("\"", "");
                    var cacheKey = string.Format("{0}-{1}-{2}", handler, method.Name, p);  // 可考虑用 MD5 做缓存健名
                    var expireDt = DateTime.Now.AddSeconds(attr.CacheSeconds);
                    // 强制刷新缓存
                    if (args.ContainsKey("_refresh"))
                    {
                        bool refresh = (args["_refresh"].ToString() == "true");
                        if (refresh)
                             CacheHelper.SetCachedObject<object>(cacheKey, expireDt, () => { return method.Invoke(handler, parameters); });
                    }
                    result = CacheHelper.GetCachedObject<object>(cacheKey, expireDt, () => { return method.Invoke(handler, parameters); });
                }

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
                if (ex is HttpApiException)
                {
                    // HttpApi 调用异常
                    var err = ex as HttpApiException;
                    WriteError(context, err.Code, err.Message);
                    HttpApiConfig.Instance.DoException(method, ex);
                }
                else
                {
                    // 方法内部异常
                    var ex2 = GetInnerException(ex);
                    WriteError(context, 500, ex2.Message);
                    HttpApiConfig.Instance.DoException(method, ex2);
                }
            }
            finally
            {
                HttpApiConfig.Instance.DoEnd(context);
            }
        }

        /// <summary>递归获取内部异常</summary>
        static Exception GetInnerException(Exception ex)
        {
            if (ex.InnerException != null)
                return GetInnerException(ex.InnerException);
            return ex;
        }

        //----------------------------------------------
        // 辅助处理方法
        //----------------------------------------------
        /// <summary>获取请求类型名</summary>
        public static string GetRequestTypeName()
        {
            var path = HttpContext.Current.Request.FilePath;
            var u = HttpContext.Current.Request.Url.AbsolutePath;
            var url = u.ToString().ToLower();

            // 以 /HttpApi/Type/Method 方式调用
            if (path.ToLower().StartsWith("/httpapi"))
            {
                // 去头尾
                path = path.Substring(9);
                int n = path.LastIndexOf("/");
                if (n != -1)
                    path = path.Substring(0, n);

                // 如果类名用的是简写，加上前缀
                if (path.IndexOf(".") == -1)
                    path = HttpApiConfig.Instance.TypePrefix + path;

                // 用点来串联
                var typeName = path.Replace('-', '.').Replace('_', '.');
                return typeName;
            }
            else
            {
                // 找到页面对应处理类（未完成）
                var typeName = path;
                return typeName;
            }
            //return "";
        }

        // 预处理保留方法（"js", "jq", "ext", "api", "apis"）
        static bool ProcessReservedMethod(HttpContext context, Type type, string methodName, Dictionary<string, object> args)
        {
            string methodNameLower = methodName.ToLower();
            int cacheDuration = ReflectHelper.GetCacheDuration(type);

            // 输出api接口展示页面
            var lastChar = methodNameLower.Substring(methodNameLower.Length - 1);
            if (lastChar == "_" || lastChar == "$")
            {
                var name = methodName.Substring(0, methodName.Length - 1);
                var typeapi = GetTypeApi(type);
                var api = FindApi(typeapi, name);
                var result = BuildApiHtml(api);
                context.Response.Clear();
                context.Response.ContentType = "text/html";
                context.Response.Write(result.ToString());
            }
            // 输出api接口测试页面
            else if (lastChar == "!")
            {
                var name = methodName.Substring(0, methodName.Length - 1);
                var typeapi = GetTypeApi(type);
                var api = FindApi(typeapi, name);
                var result = BuildApiTestHtml(api);
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
                    var result = BuildApiListHtml(typeapi);
                    context.Response.Clear();
                    context.Response.ContentType = "text/html";
                    context.Response.Write(result);
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


        /// <summary>方法可访问性校验</summary>
        private static void CheckMethod(HttpContext context, MethodInfo method, HttpApiAttribute attr, Dictionary<string,object> inputs)
        {
            // 方法未找到或未公开
            if (method == null || attr == null)
                throw new HttpApiException(404, "Not Found.");

            // 访问事件
            var instance = HttpApiConfig.Instance;
            instance.DoVisit(context, method, attr, inputs);

            // 校验方法可用性
            AuthHelper.LoadPrincipalFromCookie();  // 获取身份验票
            CheckMethodEnable(context, method, attr);

            // 自定义鉴权
            string token = inputs.Keys.Contains("token") ? inputs["token"].ToString() : "";
            instance.DoAuth(context, method, attr, token);
        }

        // 检测方法的可用性
        static void CheckMethodEnable(HttpContext context, MethodInfo method, HttpApiAttribute attr)
        {
            // API 已经删除
            if (attr.Status == ApiStatus.Delete)
                throw new HttpApiException(400, "Api deleted.");

            // 校验访问方式
            if (!attr.AuthVerbs.IsEmpty())
            {
                var verbs = attr.VerbList;
                if (verbs.Count == 0)
                    return;
                if (!verbs.Contains(context.Request.HttpMethod.ToLower()))
                    throw new HttpApiException(400, "Auth verbs fail: " + attr.AuthVerbs);
            }

            // 校验登录与否
            if (attr.AuthLogin)
            {
                if (!Asp.IsLogin())
                    throw new HttpApiException(401, "Auth login fail");
            }

            // 校验用户
            if (!string.IsNullOrEmpty(attr.AuthUsers))
            {
                if (!Asp.IsInUsers(attr.AuthUsers.Split(',', ';')))
                    throw new HttpApiException(401, "Auth user fail");
            }

            // 校验角色
            if (!string.IsNullOrEmpty(attr.AuthRoles))
            {
                if (!Asp.IsInRoles(attr.AuthRoles.Split(',', ';')))
                    throw new HttpApiException(401, "Auth role fail");
            }
        }



        //-----------------------------------------------------------
        // 输出调用结果
        //-----------------------------------------------------------
        // 输出数据
        public static void WriteResult(
            HttpContext context, object result, 
            ResponseType dataType, string mimeType = null, string fileName = null, 
            int cacheSeconds = 0, HttpCacheability cacheLocation = HttpCacheability.NoCache, 
            bool wrap = false, string wrapInfo = null
            )
        {
            // 是否需要做 DataResult JSON 封装
            if (wrap && (dataType == ResponseType.JSON || dataType == ResponseType.ImageBase64 || dataType == ResponseType.Text || dataType == ResponseType.XML))
            {
                result = new APIResult(true, wrapInfo, result, null);
                if (dataType != ResponseType.XML)
                    dataType = ResponseType.JSON;
            }

            var encoder = new ResponseEncoder(dataType, mimeType, fileName, cacheSeconds, cacheLocation);
            encoder.Write(result);
        }


        // 输出错误（根据AppSettings）
        // 若为HttpError，浏览器会跳转到对应的错误页面
        // 若为DataResult，直接输出DataResult错误信息（默认）
        public static void WriteError(HttpContext context, int errorCode, string info)
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
                var result = new APIResult(false, info, null, null, errorCode.ToString());
                WriteResult(context, result, ResponseType.JSON);
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
            // 获取接口列表
            var rootUrl = GetApiRootUrl(type);
            var typeapi = new TypeAPI();
            var apis = new List<API>();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                HttpApiAttribute attr = ReflectHelper.GetHttpApiAttribute(method);
                var desc = ReflectHelper.GetDescription(method);
                if (attr != null)
                {
                    var api = new API()
                    {
                        Name = method.Name,
                        Group = attr.Group,
                        Description = attr.Description,
                        ReturnType = ParseDataType(attr.Type, method.ReturnType).ToString(),
                        CacheDuration = attr.CacheSeconds,
                        AuthIP = attr.AuthIP,
                        AuthToken = attr.AuthToken,
                        AuthLogin = attr.AuthLogin,
                        AuthUsers = attr.AuthUsers,
                        AuthRoles = attr.AuthRoles,
                        AuthVerbs = attr.AuthVerbs.IsEmpty() ? "" : attr.AuthVerbs.ToUpper(),
                        AuthTraffic = attr.AuthTraffic,
                        PostFile = attr.PostFile,
                        Status = attr.Status,
                        Log = attr.Log,
                        Remark = attr.Remark.IsEmpty() ? desc : attr.Remark,
                        Example = attr.Example,
                        Url = GetMethodDisplayUrl(rootUrl, method),
                        UrlTest = GetMethodTestUrl(rootUrl, method, attr.AuthToken),
                        Params = GetMethodParams(method, attr),
                        Method = method,
                        RType = attr.Type
                    };
                    apis.Add(api);
                }
            }


            //
            typeapi.Apis = apis.OrderBy(t => t.Group).ThenBy(t => t.Name).ToList();
            typeapi.Description = ReflectHelper.GetDescription(type);
            typeapi.Histories = ReflectHelper.GetHistories(type);
            return typeapi;
        }

        private static string GetApiRootUrl(Type type)
        {
            var uri = HttpContext.Current.Request.Url;
            var rootUrl = string.Format("{0}://{1}/HttpApi/{2}", uri.Scheme, uri.Authority, GetTypeTrimName(type));
            return rootUrl;
        }

        private static string GetTypeTrimName(Type type)
        {
            var typeName = type.FullName;
            var typePrefix = HttpApiConfig.Instance.TypePrefix;
            if (!typePrefix.IsEmpty() && typeName.StartsWith(typePrefix))
                typeName = typeName.Substring(typePrefix.Length);
            return typeName;
        }

        /// <summary>获取方法参数信息</summary>
        private static List<HttpParamAttribute> GetMethodParams(MethodInfo method, HttpApiAttribute attri)
        {
            var items = new List<HttpParamAttribute>();
            var attrs = ReflectHelper.GetParamAttributes(method);
            var paras = method.GetParameters();
            foreach (var p in paras)
            {
                var attr = attrs.AsQueryable().FirstOrDefault(t => t.Name == p.Name);
                string desc = attr?.Description;
                items.Add(new HttpParamAttribute(
                    p.Name,
                    desc,
                    ReflectHelper.GetTypeString(p.ParameterType),
                    attr?.MaxLen,
                    ReflectHelper.GetTypeSummary(p.ParameterType),
                    GetObjectString(p.DefaultValue)
                    ));
            }
            if (attri.AuthToken)
                items.Add(new HttpParamAttribute("token", "Token", "String", -1, "", ""));
            if (attri.PostFile)
                items.Add(new HttpParamAttribute("file", "File", "File", -1, "", ""));
            return items;
        }

        // 获取对象字符串
        static string GetObjectString(object o)
        {
            if (o == null)
                return "Null";
            if (o is string && (o as string)== "")
                return "\"\"";
            return o.ToString();
        }

    }
}
