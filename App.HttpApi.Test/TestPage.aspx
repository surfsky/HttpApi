<%@ Page Language="C#" AutoEventWireup="true" ClassName="App.DemoPage" Inherits="App.HttpApi.HttpApiPageBase" %>
<%@ Import Namespace="App.HttpApi" %>
<script runat="server">
    [HttpApi("Hello", Type = App.HttpApi.ResponseType.Text)]
    public static string HelloWorld(string info)
    {
        System.Threading.Thread.Sleep(200);
        object o = Demo.GetStaticObject();
        return o.ToString();
    }
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="/res/css/site.css" rel="stylesheet" />
    <script type="text/javascript" src="/res/js/jquery-1.8.0.js"></script>
    <script type="text/javascript" src="/HttpApi/DemoPage/js"></script>
    <script type="text/javascript">
        $(function () {
            $("#clickMe").click(function () {
                App.DemoPage.HelloWorld("Kevin", function (data) { }, "clickMe");
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>调用 aspx 类方法</h1>
        <div id="clickMe" class="btn">click me</div>
    </form>
</body>
</html>
