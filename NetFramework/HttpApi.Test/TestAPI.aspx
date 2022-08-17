<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="/res/css/site.css" rel="stylesheet" />
    <script type="text/javascript" src="/res/js/jquery-1.8.0.js"></script>
    <script type="text/javascript" src="HttpApi/Demo/js"></script>
    <script type="text/javascript">
        $(function () {
            // 调用JS方法（异步）
            $("#btn1").click(function () {
                App.Demo.HelloWorld("kevin", function (data) { }, "btn1");
            });

            // 调用JS方法（同步）
            $("#btn2").click(function () {
                var o = App.Demo.HelloWorld("Kevin");
                document.getElementById("btn2").innerHTML = o;
            });


            // 手工写接口调用
            $("#btn3").click(function () {
                $.ajax({
                    url: "HttpApi/Demo/HelloWorld",
                    data: { info: 'kevin' }
                }).always(function (ret) {
                    $("#btn3").html(ret);
                });
            });

            // 复杂方法调用
            $("#btn4").click(function () {
                var o = App.Demo.CreateGirl({ Name: "Scotter", Birth: "2012-01-01" });
                document.getElementById("btn4").innerHTML = o;
            });

        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>Call HttpApi</h1>
        <div id="btn1" class="btn" >调用JS方法（异步）</div>
        <div id="btn2" class="btn" >调用JS方法（同步）</div>
        <div id="btn3" class="btn" >手工写接口调用</div>
        <div id="btn4" class="btn" >复杂方法参数调用</div>
    </form>
</body>
</html>
