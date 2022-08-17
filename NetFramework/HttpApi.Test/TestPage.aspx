<%@ Page Language="C#" AutoEventWireup="true" ClassName="App.TestPage"  %>
<%@ Import Namespace="App.HttpApi" %>
<script runat="server">
    [HttpApi("Hello", Type = App.HttpApi.ResponseType.Text)]
    public static string HelloWorld(string info)
    {
        System.Threading.Thread.Sleep(200);
        return "Hello : " + info;
    }
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="/res/css/site.css" rel="stylesheet" />
    <script type="text/javascript" src="/res/js/jquery-1.8.0.js"></script>

    <!--script type="text/javascript" src="/HttpApi/TestPage/js"></!--script-->
    <script type="text/javascript" src="/TestPage.aspx/js"></script>
    <script type="text/javascript">
        $(function () {
            $("#clickMe").click(function () {
                App.TestPage.HelloWorld("Kevin", null, "clickMe");
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
