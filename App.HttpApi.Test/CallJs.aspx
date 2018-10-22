<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script type="text/javascript" src="HttpApi/DemoClass/js?4"></script>
    <script type="text/javascript">
        function ClickMe() {
            // 同步方法
            var o = App.DemoClass.CreateGirl({ Name: "Scotter", Birth: "2012-01-01"});
            document.getElementById("clickMe").innerHTML = o;

            // 异步方法
            //App.DemoClass.HelloWorld("...ooo...", function (data) { }, "clickMe");
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>调用App_Code类方法（用映射到客户端的函数）</h1>
        <pre>
调用
    &lt;script type=&quot;text/javascript&quot; src=&quot;HttpApi.App.DemoClass.axd/js&quot;&gt;&lt;/script&gt;
    &lt;script type=&quot;text/javascript&quot;&gt;
        function ClickMe() {
            var o = App.DemoClass.CreateGirl({ Name: "Scotter"});
            document.getElementById("clickMe").innerHTML = o;
        }
    &lt;/script&gt;
        </pre>
        <div id="clickMe" style="background:lightblue;" onclick="ClickMe()">click me</div>
    </form>
</body>
</html>
