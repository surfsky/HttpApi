using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.Text;
using System.Linq;

namespace App.HttpApi
{
    /// <summary>
    /// Http 错误
    /// </summary>
    public class HttpError
    {
        public int Code { get; set; }
        public string Info { get; set; }
        public HttpError(int code, string info)
        {
            this.Code = code;
            this.Info = info;
        }
    }


    /// <summary>
    /// HttpApi 的逻辑实现。
    /// </summary>
    public partial class HttpApiHelper
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
            Dictionary<string, object> args = decoder.ParseArguments();

            // 获取类型和方法信息
            // 对于aspx页面，执行时编译出来的是类似ASP.xxx_aspx的类，这不是我们想要处理的类，追溯处理父类(IMPORTANT!)
            Type type = handler.GetType();
            if (type.FullName.StartsWith("ASP.") && type.BaseType != null)
                type = type.BaseType;
            string methodName = decoder.MethodName;
            MethodInfo method = ReflectHelper.GetMethod(type, methodName);
            HttpApiAttribute attr = ReflectHelper.GetHttpApiAttribute(method);

            // 处理预留方法
            if (ProcessReservedMethod(context, type, methodName, args))
                return;

            // 访问鉴权
            string ip = Asp.GetClientIP();
            var principal = App.Core.AuthHelper.LoadCookiePrincipal();  // 获取身份验票
            string securityCode = context.Request.Params["securityCode"];
            var err = CheckMethodEnable(context, method, methodName, attr);
            if (err != null)
            {
                WriteError(context, err.Code, err.Info);
                return;
            }
            err = HttpApiConfig.Instance.DoAuth(context, method, attr, ip, securityCode);
            if (err != null)
            {
                WriteError(context, err.Code, err.Info);
                return;
            }

            // 普通方法调用
            try
            {
                // 获取需要的参数
                var parameters = ReflectHelper.GetParameters(method, args);
                var p = SerializeHelper.ToJson(parameters).ClearSpace().TrimStart('[').TrimEnd(']');//.Replace("\"", "");
                //var p = HttpContext.Current.Request.Url.Query.TrimStart('?');  // 不能用 URL，要兼容 Post 方式调用
                var cacheKey = string.Format("{0}-{1}-{2}", handler, method.Name, p);
                var expireDt = DateTime.Now.AddSeconds(attr.CacheSeconds);

                // 获取方法调用结果并依情况缓存
                object result;
                if (attr.CacheSeconds == 0)
                    result = method.Invoke(handler, parameters);
                else
                    result = CacheHelper.GetCachedObject<object>(
                        cacheKey, expireDt, 
                        () =>{return method.Invoke(handler, parameters);}
                        );

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
                HttpApiConfig.Instance.DoException(method, ex);
                string result = string.Format("Api {0}() FAIL. {1} {2}", methodName, ex.Message, ex.InnerException?.Message);
                WriteError(context, 400, result);
            }
            finally
            {
                HttpApiConfig.Instance.DoEnd(context);
            }
        }

        // 预处理保留方法（"js", "jq", "ext", "api", "apis"）
        static bool ProcessReservedMethod(HttpContext context, Type type, string methodName, Dictionary<string, object> args)
        {
            string methodNameLower = methodName.ToLower();
            int cacheDuration = ReflectHelper.GetCacheDuration(type);

            // 输出api接口展示页面（方法名后面跟了个_)
            var lastChar = methodNameLower.Substring(methodNameLower.Length - 1);
            if (lastChar == "_")
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
        static HttpError CheckMethodEnable(HttpContext context, MethodInfo method, string methodName, HttpApiAttribute attr)
        {
            // 方法未找到或未公开
            if (method == null || attr == null)
                return new HttpError(404, "Function " + methodName + " not found. Please check the [HttpApi] attribute.");

            // 校验访问方式
            if (!attr.AuthVerbs.IsNullOrEmpty())
            {
                var verbs = attr.VerbList;
                if (verbs.Count == 0)
                    return null;
                if (!verbs.Contains(context.Request.HttpMethod.ToLower()))
                    return new HttpError(400, "Auth verbs fail: " + attr.AuthVerbs);
            }

            // 校验登录与否
            if (attr.AuthLogin)
            {
                if (!Asp.IsLogin())
                    return new HttpError(401, "Auth login fail");
            }

            // 校验用户
            if (!string.IsNullOrEmpty(attr.AuthUsers))
            {
                if (!Asp.IsInUsers(attr.AuthUsers.Split(',', ';')))
                    return new HttpError(401, "Auth user fail");
            }

            // 校验角色
            if (!string.IsNullOrEmpty(attr.AuthRoles))
            {
                if (!Asp.IsInRoles(attr.AuthRoles.Split(',', ';')))
                    return new HttpError(401, "Role auth fail");
            }

            return null;
        }

        /// <summary>获取请求类型名</summary>
        public static string GetRequestTypeName()
        {
            var path = HttpContext.Current.Request.FilePath;

            // 去头
            if (path.StartsWith("/HttpApi") || path.StartsWith("/httpapi"))
                path = path.Substring(9);

            // 去尾
            int n = path.LastIndexOf("/");
            if (n != -1)
                path = path.Substring(0, n);

            // 去扩展名
            //n = path.LastIndexOf(".axd");
            //if (n != -1)
            //    path = path.Substring(0, n);

            // 如果类名用的是简写，加上前缀
            if (path.IndexOf(".") == -1)
                path = HttpApiConfig.Instance.ApiTypePrefix + path;

            // 用点来串联
            var typeName = path.Replace('-', '.').Replace('_', '.');
            return typeName;
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
                result = new DataResult(true, wrapInfo, result, null);
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
                DataResult dr = new DataResult(false, info, errorCode, null);
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
            // 获取接口列表
            var rootUrl = GetApiRootUrl(type);
            var typeapi = new TypeAPI();
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
                        AuthIP = attr.AuthIP,
                        AuthSecurityCode = attr.AuthSecurityCode,
                        AuthLogin = attr.AuthLogin,
                        AuthUsers = attr.AuthUsers,
                        AuthRoles = attr.AuthRoles,
                        AuthVerbs = attr.AuthVerbs.IsNullOrEmpty() ? "" : attr.AuthVerbs.ToUpper(),
                        Status = attr.Status,
                        Remark = attr.Remark,
                        Example = attr.Example,
                        Url = GetMethodDisplayUrl(rootUrl, method),
                        UrlTest = GetMethodTestUrl(rootUrl, method, attr.AuthSecurityCode),
                        Params = GetMethodParams(method, attr.AuthSecurityCode),
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

        private static string GetApiRootUrl(Type type)
        {
            var uri = HttpContext.Current.Request.Url;
            var rootUrl = string.Format("{0}://{1}/HttpApi/{2}", uri.Scheme, uri.Authority, GetTypeTrimName(type));
            return rootUrl;
        }

        private static string GetTypeTrimName(Type type)
        {
            var typeName = type.FullName;
            var typePrefix = HttpApiConfig.Instance.ApiTypePrefix;
            if (!typePrefix.IsNullOrEmpty() && typeName.StartsWith(typePrefix))
                typeName = typeName.Substring(typePrefix.Length);
            return typeName;
        }

        /// <summary>获取方法参数信息</summary>
        private static List<ParamAttribute> GetMethodParams(MethodInfo method, bool authSecurityCode)
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
                    ReflectHelper.GetTypeString(p.ParameterType),
                    ReflectHelper.GetTypeSummary(p.ParameterType),
                    p.DefaultValue?.ToString()
                    ));
            }
            if (authSecurityCode)
                items.Add(new ParamAttribute("securityCode", "安全码", "String", "", ""));
            return items;
        }


    }
}
