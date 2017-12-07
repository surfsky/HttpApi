<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="js/jquery-1.8.0.js"></script>
    <script type="text/javascript" src="HttpApi.App.DemoClass.axd/js"></script>
    <script type="text/javascript">
        $(function () {
            $("#clickMe").click(function () {
                var o = App.DemoClass.GetTime(); // 同步方法
                App.DemoClass.HelloWorld("...ooo...", function (data) { }, "clickMe");
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>调用App_Code类方法（依赖jquery）</h1>
        <pre>
类
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Data;
    using App.HttpApi;
    namespace App
    {
        public class DemoClass
        {
            [HttpApi(Type = ResponseDataType.Text)]
            public string HelloWorld(string info)
            {
                System.Threading.Thread.Sleep(200);
                return &quot;hello world &quot; + info;
            }
        }
    }

调用
    &lt;script type=&quot;text/javascript&quot; src=&quot;HttpApi.App.DemoClass.axd/jq&quot;&gt;&lt;/script&gt;
    &lt;script type=&quot;text/javascript&quot;&gt;
        $(function () {
            $(&quot;#clickMe&quot;).click(function () {
                var o = App.DemoClass.GetTime(); // 同步方法
                App.DemoClass.HelloWorld(&quot;...ooo...&quot;, function (data) { }, &quot;clickMe&quot;);
            });
        });
    &lt;/script&gt;
        </pre>
        <div id="clickMe" style="background:lightblue;">click me</div>
    </form>
</body>
</html>
