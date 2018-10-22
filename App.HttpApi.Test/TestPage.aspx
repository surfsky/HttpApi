<%@ Page Language="C#" AutoEventWireup="true" ClassName="App.TestPage" Inherits="App.HttpApi.HttpApiPageBase" %>
<%@ Import Namespace="App.HttpApi" %>
<script runat="server">
    [HttpApi("Hello", Type = ResponseType.Text)]
    public static string HelloWorld(string info)
    {
        System.Threading.Thread.Sleep(200);
        return "Hello world : " + info;
        //return DemoClass.GetStaticObject().ToString();  // cool，直接调用动态编译文件的方法
    }

    [HttpApi(Type = ResponseType.Text)]
    public DateTime GetTime()
    {
        return System.DateTime.Now;
    }
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script type="text/javascript" src="js/jquery-1.8.0.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>HttpApiHttpPage测试</h1>
        <pre>
        </pre>
        <div id="clickMe" style="background:lightblue;">click me</div>
    </form>
    <script type="text/javascript">
        $(function () {
            $("#clickMe").click(function () {
                var o = App.TestPage.GetTime();
                App.TestPage.HelloWorld("...kevin...", function (data) { }, "clickMe");
            });
        });
    </script>
</body>
</html>
