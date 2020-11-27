using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.Text;
using HttpApiCore.Properties;
//using AppPlat.HttpApi.Properties;

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
        // 样式控制
        static string BuildCss()
        {
            return @"
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link rel=""stylesheet"" href=""https://cdn.staticfile.org/twitter-bootstrap/4.1.0/css/bootstrap.min.css"">
    <script src=""https://cdn.staticfile.org/jquery/3.2.1/jquery.min.js""></script>
    <script src=""https://cdn.staticfile.org/popper.js/1.12.5/umd/popper.min.js""></script>
    <script src=""https://cdn.staticfile.org/twitter-bootstrap/4.1.0/js/bootstrap.min.js""></script>
    <style>
        body {padding: 20px;}
        h1 {font-size:1.8rem;}
        h2 {font-size:1.6rem;}
        h3 {font-size:1.4rem;}
        form {width: 100%;}
        thead {background-color: ghostwhite;}
    </style>
</head>
    ";
        }

        /// <summary>
        /// 构造接口清单页面
        /// </summary>
        static string BuildApiListHtml(TypeAPI typeapi)
        {
            // 概述信息
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(BuildCss());
            sb.AppendLine("<h1>" + typeapi.Description + "</h1>");
            foreach (HistoryAttribute history in typeapi.Histories)
            {
                sb.AppendFormat("<small>{0}, {1}, {2}</small><br/>", history.Date, history.User, history.Info);
            }

            // 接口清单
            sb.AppendFormat("<br/>");
            sb.AppendFormat("<table class='table table-sm table-hover'>");
            sb.AppendFormat("<thead><tr>");
            sb.AppendFormat("<td>{0}</td>", Resources.Group);
            sb.AppendFormat("<td>{0}</td>", Resources.Name);
            sb.AppendFormat("<td>{0}</td>", Resources.Description);
            sb.AppendFormat("<td>{0}</td>", Resources.ReturnType);
            sb.AppendFormat("<td>{0}</td>", Resources.CacheSeconds);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthIP);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthToken);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthLogin);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthUser);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthRole);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthVerbs);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthTraffic);
            sb.AppendFormat("<td>{0}</td>", Resources.PostFile);
            sb.AppendFormat("<td>{0}</td>", Resources.Log);
            sb.AppendFormat("<td>{0}</td>", Resources.Status);
            sb.AppendFormat("</tr></thead>");
            foreach (var api in typeapi.Apis)
            {
                sb.AppendFormat("<tr>");
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Group);
                sb.AppendFormat("<td><a target='_blank' href='{0}'>{1}&nbsp;</a></td>", api.Url, api.Name);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Description);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.ReturnType);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.CacheDuration);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthIP);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthToken);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthLogin);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthUsers);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthRoles);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthVerbs);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthTraffic);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.PostFile);
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Log);
                if (api.Status == ApiStatus.Publish)
                    sb.AppendFormat("<td>{0}&nbsp;</td>", api.Status);
                else
                    sb.AppendFormat("<td class='text-warning'>{0}&nbsp;</td>", api.Status);
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
            sb.AppendLine(BuildCss());
            sb.AppendFormat("<h1>{0}</h1>", api.Name);
            sb.AppendFormat("<h3>{0}</h3>", api.Description);
            sb.AppendFormat("<small>{0}</small></br>", api.UrlTest);
            sb.AppendFormat("<small>{0}</small></br>", api.Remark);
            sb.AppendFormat("<code>{0}</code></br>", api.Example);

            // 属性
            sb.AppendFormat("<h3>{0}</h3><br/>", Resources.Property);
            sb.AppendFormat("<table class='table table-sm table-hover'>");
            sb.AppendFormat("<thead><tr>");
            sb.AppendFormat("<td>{0}</td>", Resources.ReturnType);
            sb.AppendFormat("<td>{0}</td>", Resources.CacheSeconds);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthIP);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthToken);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthLogin);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthUser);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthRole);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthVerbs);
            sb.AppendFormat("<td>{0}</td>", Resources.AuthTraffic);
            sb.AppendFormat("<td>{0}</td>", Resources.PostFile);
            sb.AppendFormat("<td>{0}</td>", Resources.Log);
            sb.AppendFormat("<td>{0}</td>", Resources.Status);
            sb.AppendFormat("</tr></thead>");
            sb.AppendFormat("<tr>");
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.ReturnType);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.CacheDuration);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthIP);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthToken);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthLogin);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthUsers);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthRoles);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthVerbs);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.AuthTraffic);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.PostFile);
            sb.AppendFormat("<td>{0}&nbsp;</td>", api.Log);

            if (api.Status == ApiStatus.Publish)
                sb.AppendFormat("<td>{0}&nbsp;</td>", api.Status);
            else
                sb.AppendFormat("<td class='text-warning'>{0}&nbsp;</td>", api.Status);

            sb.AppendFormat("</tr>");
            sb.AppendLine("</table>");

            // 参数
            sb.AppendFormat("<h3>{0}</h3>", Resources.Parameters);
            sb.Append(BuildApiTestHtml(api));
            return sb.ToString();
        }


        /// <summary>
        /// 构造API测试页面
        /// </summary>
        static string BuildApiTestHtml(API api)
        {
            var sb = new StringBuilder();
            if (api.PostFile)
                sb.AppendFormat("<form action='{0}' method='post' enctype='multipart/form-data'>", api.Url.TrimEnd('_', '$', '!'));
            else
                sb.AppendFormat("<form action='{0}' method='post'>", api.Url.TrimEnd('_', '$', '!'));
            sb.AppendFormat("<br/><table class='table table-sm table-hover'>");
            sb.AppendFormat("<thead><tr>");
            sb.AppendFormat("<td>{0}</td>", Resources.ParamName);
            sb.AppendFormat("<td>{0}</td>", Resources.ParamValue);
            sb.AppendFormat("<td>{0}</td>", Resources.Type);
            sb.AppendFormat("<td>{0}</td>", Resources.DefaultValue);
            sb.AppendFormat("<td>{0}</td>", Resources.Description);
            sb.AppendFormat("<td>{0}</td>", Resources.Remark);
            sb.AppendFormat("</tr></thead>");

            foreach (var p in api.Params)
            {
                sb.AppendFormat("<tr>");
                sb.AppendFormat("<td>{0}&nbsp;</td>", p.Name);
                if (p.Type == "File")
                    sb.AppendFormat("<td><input type='file' name='{0}' class='form-control form-control-sm'/></td>", p.Name);
                else
                    sb.AppendFormat("<td><input type='text' name='{0}' class='form-control form-control-sm'/></td>", p.Name);
                sb.AppendFormat("<td>{0}&nbsp;</td>", p.Type);
                sb.AppendFormat("<td>{0}&nbsp;</td>", p.DefaultValue);
                sb.AppendFormat("<td>{0}&nbsp;</td>", p.Description);
                sb.AppendFormat("<td>{0}&nbsp;</td>", p.Remark);
                sb.AppendFormat("</tr>");
            }
            sb.AppendFormat("</tr></table>");
            sb.AppendFormat("<input type='submit' value='{0}' class='btn btn-primary btn-sm' />", Resources.Submit);
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
            var sb = new StringBuilder(script);
            foreach (var api in typeapi.Apis)
            {
                sb.AppendFormat("// {0}\r\n", api.Description);
                sb.AppendFormat("// {0}\r\n", api.Url);

                string func = GetFunctionScript(nameSpace, className, api.Method, api.RType, api.AuthToken);
                sb.AppendLine(func);
            }

            // 插入json2.js并输出
            sb.Insert(0, GetJsonScript());
            return sb;
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
        static string GetMethodTestUrl(string rootUrl, MethodInfo method, bool authToken)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}/{1}", rootUrl, method.Name);
            ParameterInfo[] ps = method.GetParameters();
            if (ps.Length > 0)
            {
                sb.Append("?");
                foreach (ParameterInfo p in ps)
                    sb.Append(p.Name + "=x&");
            }
            if (authToken)
            {
                if (sb.ToString().IndexOf('?') == -1)
                    sb.Append('?');
                sb.Append("token=x");
            }
            return sb.ToString().TrimEnd('&');
        }

        /// <summary>
        /// 取得函数调用代码段
        /// </summary>
        static string GetFunctionScript(string nameSpace, string className, MethodInfo method, ResponseType format, bool authToken)
        {
            var sb = new StringBuilder();

            // 函数名和参数
            sb.AppendFormat("{0}.{1}.{2}", nameSpace, className, method.Name);
            sb.Append(" = function(");
            foreach (ParameterInfo p in method.GetParameters())
                sb.Append(p.Name + ", ");
            if (authToken)
                sb.Append("token, ");
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
            if (authToken)
                sb.AppendFormat("token : token");
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
