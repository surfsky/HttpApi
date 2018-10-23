<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="js/jquery-1.8.0.js"></script>
    <style>
        h3 { text-indent:-10px; font-size:10px;}
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <h1>轻量级Web数据服务框架：App.HttpApi</h1>
    <hr />
    <h2>示例</h2>
    <ul>
        <h3>方法映射</h3>
        <li><a target="_blank" href="HttpApi/DemoClass/js">查看App_Code类映射到客户端的js文件</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/jq">查看App_Code类映射到客户端的js文件（依赖jquery）</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/api">查看App_Code类提供的接口清单(html)</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/apis">查看App_Code类提供的接口清单(json)</a></li>
        <li><a target="_blank" href="TestPage.aspx/js">查看Aspx文件映射到客户端的js文件</a></li>
        <li><a target="_blank" href="TestHandler.ashx/js">查看Ashx文件映射到客户端的js文件</a></li>
        <li><a target="_blank" href="HttpApi/TestPage/js">查看Aspx类映射到客户端的js文件</a></li>
        <li><a target="_blank" href="HttpApi/TestHandler/js">查看Ashx类映射到客户端的js文件</a></li>
        <h3>方法调用</h3>
        <li><a target="_blank" href="CallJs.aspx">调用类方法（用映射到客户端的js函数调用）</a></li>
        <li><a target="_blank" href="CallJquery.aspx">调用类方法（用Jquery手工写调用代码）</a></li>
        <li><a target="_blank" href="CallPage.aspx">调用Aspx类方法</a></li>
        <li><a target="_blank" href="TestAuth.aspx">访问权限控制</a></li>
        <h3>输出格式</h3>
        <li><a target="_blank" href="HttpApi/DemoClass/GetStaticObject">返回：匿名对象 json</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetStaticObject&_type=xml">返回：匿名对象 xml</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetDictionary">返回：字典 json</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetDictionary?_type=xml">返回：字典 xml</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/HelloWorld?info=kevin">返回：text</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetFile?info=xxx">返回：plist</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetPerson">返回：object json</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetPersonXml">返回：object xml</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetPersonDataResult">返回：object DataResult</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetImage?text=Hello">返回：image</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetImage?text=xxx&_type=imagebase64">返回：image base64</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetPersons">返回：list json</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetPersons?_type=xml">返回：list xml</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetDataTable?_type=xml">返回：datatable xml</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/GetDataTable?_type=json">返回：datatable json</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/CreateNull">返回：null json</a></li>
        <li><a target="_blank" href="HttpApi/DemoClass/CreateNull?_type=xml">返回：null xml</a></li>
    </ul>

<h2>说明</h2>
<pre>
简介：
    （1）是一种轻量、便捷的 Web 数据服务框架。
    （2）可针对任意服务器端的类（包括动态编译的类，如aspx和app_code中放置的cs类），自动生成客户端 js 调用脚本
    （3）以类似WebService Url的方式来提供数据，如：Handler1.ashx\GetTime
    （4）返回的格式很丰富（xml，json，text），且客户端可指定服务器端返回的数据格式
    （5）带缓存机制

使用
    （1）添加 App.HttpApi.dll 引用
    （2）创建任意类。使用[HttpApi]特性标签
        服务器端代码如：
            using System;
            using App.HttpApi;
            namespace App
            {
                public class DemoClass
                {
                    [HttpApi(
                        Type = ResponseDataType.Text,
                        Description="演示自定义序列化为HTML"),
                        CacheDuration = 30
                    ]
                    public string HelloWorld(string info)
                    {
                        System.Threading.Thread.Sleep(200);
                        return "hello world " + info;
                    }
                }
            }
    （2）注册HttpHandler：
        &lt;httpHandlers&gt;
          &lt;add verb=&quot;*&quot; path=&quot;HttpApi.*.axd&quot; type=&quot;App.HttpApi.HttpApiHandler, App.HttpApi&quot;/&gt;
        &lt;/httpHandlers&gt;
    （3）直接用生成的客户端js函数调用
        1.引用
            &lt;script type=&quot;text/javascript&quot; src=&quot;HttpApi.App.DemoClass.axd/js&quot;&gt;&lt;/script&gt;
            生成的 JS 函数如：
            App.DemoClass.HelloWorld = function(info, callback, senderId){
                var args = {info:info};
                var options = {dataType:'text'};
                return this.CallWebMethod('HelloWorld', args, options, callback, senderId);
            }
        2.调用
            (1) 同步调用： var txt = App.DemoClass.HelloWorld("kevin");
            (2) 异步调用1：App.DemoClass.HelloWorld("kevin", function(data){ $("#clickMe").html(data); });
            (3) 异步调用2：App.DemoClass.HelloWorld("kevin", function(){}, "clickMe");
        3.若有必要可修改这两个全局函数（可用于做全局loading效果）
            App.PreCallWebMethod = function (id) {if (id != null) $("#" + id).html("loading...");};
            App.AfterCallWebMethod = function (id, data) {if (id != null) $("#" + id).html(data);};
    （4）手工写jquery方法调用
        $.ajax({
            url: "HttpApi.App.DemoClass.axd/HelloWorld",
            data: { info: 'kevin' }
        }).always(function (ret) {
            $("#clickMe").html(ret);
        });
</pre>
    </form>
</body>
</html>
