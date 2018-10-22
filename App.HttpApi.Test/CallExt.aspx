<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script type="text/javascript" charset="utf-8" src="http://cdn.sencha.io/ext-4.1.0-gpl/ext-all.js"></script>
    <script type="text/javascript" src="HttpApi.App.DemoClass.axd/js"></script>
    <script type="text/javascript">
        Ext.onReady(function () {
            Ext.get("clickMe").on("click", function () {
                App.DemoClass.HelloWorld("...ooo...", function (data) { }, "clickMe");
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>调用App_Code类方法（依赖ExtJs）</h1>
        <pre>
调用
    &lt;script type=&quot;text/javascript&quot; src=&quot;HttpApi.App.DemoClass.axd/ext&quot;&gt;&lt;/script&gt;
    &lt;script type=&quot;text/javascript&quot;&gt;
        Ext.onReady(function () {
            Ext.get("clickMe").on("click", function () {
                App.DemoClass.HelloWorld("...ooo...", function (data) { }, "clickMe");
            });
        });
    &lt;/script&gt;
        </pre>
        <div id="clickMe" style="background:lightblue;">click me</div>
    </form>
</body>
</html>
