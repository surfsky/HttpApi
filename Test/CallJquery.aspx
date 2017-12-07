<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script type="text/javascript" src="js/jquery-1.8.0.js"></script>
    <script type="text/javascript">
        $(function () {
            $("#clickMe").click(function () {
                $.ajax({
                    url: "HttpApi.App.DemoClass.axd/HelloWorld",
                    data: { info: 'kevin' }
                }).always(function (ret) {
                    $("#clickMe").html(ret);
                });
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>调用App_Code类方法（用Jquery手工写调用代码）</h1>
        <pre>
调用
    $.ajax({
        url: "HttpApi.App.DemoClass.axd/HelloWorld",
        data: { info: 'kevin' }
    }).always(function (ret) {
        $("#clickMe").html(ret);
    });
        </pre>
        <div id="clickMe" style="background:lightblue;">click me</div>
    </form>
</body>
</html>
