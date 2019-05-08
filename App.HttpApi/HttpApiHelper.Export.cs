using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.Text;

namespace App.HttpApi
{

    /// <summary>
    /// HttpApi 的逻辑实现。
    /// </summary>
    public partial class HttpApiHelper
    {
        //-----------------------------------------------------------
        // 生成接口页面
        //-----------------------------------------------------------
        /// <summary>
        /// 构造接口清单页面
        /// </summary>
        static string BuildApiListHtml(TypeAPI typeapi)
        {
            // 概述信息
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<h1>" + typeapi.Description + "</h1>");
            foreach (HistoryAttribute history in typeapi.Histories)
            {
                sb.AppendFormat("<small>{0}, {1}, {2}</small><br/>", history.Date, history.User, history.Info);
            }

            // 接口清单
            sb.AppendLine("<table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendLine(@"<tr>
                <td width='200'>接口名</td>
                <td width='200'>说明</td>
                <td width='70'>返回类型</td>
                <td width='70'>缓存(秒)</td>
                <td width='70'>校验IP</td>
                <td width='80'>校验安全码</td>
                <td width='70'>校验登录</td>
                <td width='70'>校验用户</td>
                <td width='70'>校验角色</td>
                <td width='70'>校验动作</td>
                <td width='70'>访问日志</td>
                <td width='70'>状态</td>
                <td>备注</td>
                </tr>");
            foreach (var api in typeapi.Apis)
            {
                sb.AppendFormat("<tr>");
                sb.AppendFormat("<td><a target='_blank' href='{0}'>{1}&nbsp;</a></td>", api.Url, api.Name);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Description);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.ReturnType);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.CacheDuration);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthIP);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthSecurityCode);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthLogin);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthUsers);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthRoles);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthVerbs);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Log);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Status);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Remark);
                sb.AppendFormat("</tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }


        /// <summary>
        /// 构造接口页面
        /// </summary>
        static string BuildApiHtml(API api)
        {
            // 概述信息
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<h1>{0}</h1>", api.Name);
            sb.AppendFormat("<h3>{0}</h3>", api.Description);
            sb.AppendFormat("<div>{0}</div></br>", api.UrlTest);
            sb.AppendFormat("<div>{0}</div></br>", api.Remark);
            sb.AppendFormat("<div>{0}</div></br>", api.Example);

            // 属性
            sb.AppendFormat("<h2>属性</h2>");
            sb.AppendLine("<table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendLine(@"<tr>
                <td width='70'>返回类型</td>
                <td width='70'>缓存(秒)</td>
                <td width='70'>校验IP</td>
                <td width='80'>校验安全码</td>
                <td width='70'>校验登录</td>
                <td width='70'>校验用户</td>
                <td width='70'>校验角色</td>
                <td width='70'>校验动作</td>
                <td width='70'>访问日志</td>
                <td width='70'>状态</td>
                <td>备注</td>
                </tr>");
            sb.AppendFormat("<tr>");
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.ReturnType);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.CacheDuration);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthIP);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthSecurityCode);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthLogin);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthUsers);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthRoles);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthVerbs);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.Log);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.Status);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.Remark);
            sb.AppendFormat("</tr>");
            sb.AppendLine("</table>");

            // 参数
            sb.AppendFormat("<h2>参数</h2>");
            sb.Append(BuildApiTestHtml(api));
            return sb.ToString();
        }

        /// <summary>
        /// 构造API参数页面
        /// </summary>
        static string BuildApiParamsHtml(API api)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<br/><table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width='100'>参数名</td><td>描述</td><td>类型</td><td>说明</td><td>缺省值</td></tr>");
            foreach (var p in api.Params)
            {
                sb.AppendFormat("<tr><td>{0}&nbsp;</td><td>{1}&nbsp;</td><td>{2}&nbsp;</td><td>{3}&nbsp;</td><td>{4}&nbsp;</td></tr>"
                    , p.Name
                    , p.Description
                    , p.Type
                    , p.Info
                    , p.DefaultValue
                    );
            }
            sb.AppendFormat("</tr></table>");
            return sb.ToString();
        }

        /// <summary>
        /// 构造API测试页面
        /// </summary>
        static string BuildApiTestHtml(API api)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<form action='{0}' method='post'>", api.Url.TrimEnd('_', '$', '!'));
            sb.AppendFormat("<br/><table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width='100'>参数名</td><td>值</td><td>描述</td><td>类型</td><td>说明</td><td>缺省值</td></tr>");
            foreach (var p in api.Params)
            {
                sb.AppendFormat("<tr><td>{0}&nbsp;</td><td><input type='text' name='{0}' style='width:200px; border:none'/></td><td>{1}&nbsp;</td><td>{2}&nbsp;</td><td>{3}&nbsp;</td><td>{4}&nbsp;</td></tr>"
                    , p.Name
                    , p.Description
                    , p.Type
                    , p.Info
                    , p.DefaultValue
                    );
            }
            sb.AppendFormat("</tr></table>");
            sb.AppendFormat("<input type='submit' value='提 交' style='margin-top:30px' />");
            sb.AppendFormat("</form>");
            return sb.ToString();
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
            string rootUrl = GetApiRootUrl(type);

            // 并进行字符串替换：描述、时间、地址、类名
            script = script.Replace("%DATE%", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            script = script.Replace("%DURATION%", cacheDuration.ToString());
            script = script.Replace("%URL%", rootUrl);
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
                scriptBuilder.AppendLine("// 校验IP: " + api.AuthIP);
                scriptBuilder.AppendLine("// 校验安全码: " + api.AuthSecurityCode);
                scriptBuilder.AppendLine("// 校验登录: " + api.AuthLogin);
                scriptBuilder.AppendLine("// 校验用户: " + api.AuthUsers);
                scriptBuilder.AppendLine("// 校验角色: " + api.AuthRoles);
                scriptBuilder.AppendLine("// 校验动作: " + api.AuthVerbs);
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
            return string.Format("{0}/{1}$", rootUrl, method.Name);
        }

        // 获取API方法测试地址
        static string GetMethodTestUrl(string rootUrl, MethodInfo method, bool authSecurityCode)
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
            if (authSecurityCode)
                sb.Append("securityCode=x");
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
