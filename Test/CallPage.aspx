<%@ Page Language="C#" AutoEventWireup="true" ClassName="App.DemoPage" %>
<%@ Import Namespace="App.HttpApi" %>
<script runat="server">
    [HttpApi(Description = "Hello", Type = ResponseDataType.Text)]
    public static string HelloWorld(string info)
    {
        System.Threading.Thread.Sleep(200);
        object o = DemoClass.GetStaticObject();  // cool，直接调用动态编译文件的方法
        return o.ToString();
    }
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script type="text/javascript" src="js/jquery-1.8.0.js"></script>
    <script type="text/javascript" src="HttpApi.App.DemoPage.axd/js"></script>
    <script type="text/javascript">
        $(function () {
            $("#clickMe").click(function () {
                App.DemoPage.HelloWorld("...ooo...", function (data) { }, "clickMe");
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1>调用 aspx 类方法</h1>
        <pre>
服务器端代码
    &lt;%@ Page Language=&quot;C#&quot; AutoEventWireup=&quot;true&quot; ClassName=&quot;App.DemoPage&quot; %&gt;
    &lt;%@ Import Namespace=&quot;App.HttpApi&quot; %&gt;
    &lt;script runat=&quot;server&quot;&gt;
        [HttpApi(Description = &quot;Hello&quot;, Type = ResponseDataType.Text)]
        public static string HelloWorld(string info)
        {
            System.Threading.Thread.Sleep(200);
            object o = DemoClass.GetStaticObject();  // 可调用App_Code中的类
            return o.ToString();
        }
    &lt;/script&gt;
调用
    &lt;script type=&quot;text/javascript&quot; src=&quot;HttpApi.App.DemoPage.axd/js&quot;&gt;&lt;/script&gt;
    &lt;script type=&quot;text/javascript&quot;&gt;
        $(function () {
            $(&quot;#clickMe&quot;).click(function () {
                App.DemoPage.HelloWorld(&quot;...ooo...&quot;, function (data) { }, &quot;clickMe&quot;);
            });
        });
    &lt;/script&gt;
    
        </pre>
        <div id="clickMe" style="background:lightblue;">click me</div>
    </form>
</body>
</html>
