<%@ Page Language="C#" AutoEventWireup="true"  %>
<%@ Import Namespace="System.Security.Principal" %>
<%@ Import Namespace="App.Core" %>
<%@ Import Namespace="App.HttpApi" %>
<%@ Import Namespace="App" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
            ShowUser();
    }

    // 登陆
    protected void btnLogin_Click(object sender, EventArgs e)
    {
        IPrincipal p = AuthHelper.Login("Surfsky", null, DateTime.Now.AddDays(1));
        ShowUser(p);
    }
    protected void btnLogin2_Click(object sender, EventArgs e)
    {
        IPrincipal p = AuthHelper.Login("Kevin", new string[] { "Admins" }, DateTime.Now.AddDays(1));
        ShowUser(p);
    }

    // 注销
    protected void btnLogout_Click(object sender, EventArgs e)
    {
        AuthHelper.Logout();
        ShowUser(null);
    }

    // 显示用户状态
    void ShowUser()
    {
        ShowUser(HttpContext.Current.User);
    }
    void ShowUser(IPrincipal p)
    {
        if (p == null || p.Identity.Name == "")
            this.lblInfo.Text = "未登录";
        else
        {
            this.lblInfo.Text = p.Identity.Name + "    ";
            if (p.IsInRole("Admins"))
                lblInfo.Text += "Admins";
        }
    }
</script>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link href="/res/css/site.css" rel="stylesheet" />
    <script type="text/javascript" src="/HttpApi/Demo/js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label runat="server" ID="lblInfo"  Text="当前用户信息" />
        <button id="btn1" onclick="App.Demo.LimitLogin(function (data) { }, 'btn1')" class="btn">登录后才可以调用</button>
        <button id="btn2" onclick="App.Demo.LimitUser(function (data) { }, 'btn2');" class="btn">指定用户（Kevin）才可以调用</button>
        <button id="btn3" onclick="App.Demo.LimitRole(function (data) { }, 'btn3');" class="btn">指定角色（Admins）才可以调用</button>
        <br />
        <br />
        <asp:Button runat="server" ID="btnLogin" Text="登录为 Surfsky" OnClick="btnLogin_Click" class="btn" />
        <asp:Button runat="server" ID="btnLogin2" Text="登录为 Kevin (Admins)" OnClick="btnLogin2_Click" class="btn" />
        <asp:Button runat="server" ID="btnLogout" Text="注销" OnClick="btnLogout_Click" class="btn" />
    </div>

    <pre>
【使用方法】
（1）在需认证的HttpApi方法上加上特性标签
        - AllowLogin： 校验登陆状态
        - AllowUsers ：校验允许访问的用户（用逗号分隔）
        - AuthRoles ： 校验允许访问的角色（用逗号分隔）
    示例
        [HttpApi()]
        public string Login()
        {
            AuthHelper.Login("Admin", null, DateTime.Now.AddDays(1));
            return "登录成功";
        }
        [HttpApi(AuthLogin=true)]
        public string LimitLogin()
        {
            return "用户必须登录后才能访问该接口";
        }
        [HttpApi(AuthUsers = "Admin,Kevin")]
        public string LimitUser()
        {
            return "指定用户才能访问该方法（Admin,Kevin）";
        }
        [HttpApi(AuthRoles = "Admins")]
        public string LimitRole()
        {
            return "指定角色才能访问该方法（Admins）";
        }

（2）在Global.asax.cs中写下以下代码，从cookie验票中获取当前用户信息
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            AuthHelper.LoadCookieTicket();
        }

（4）可在Web.confit中配置错误时返回格式（不设置的话默认为APIResult）
        // 若为HttpError，会输出标准的HTTP错误，浏览器的话会跳转到对应的错误页面
        // 若为APIResult，直接输出 APIResult json 错误信息


【关于HttpContext.Current.User】
HttpContext.Current.User 保存了当前访问用户的信息
    - 含两个基本接口
        IPrincipal p = HttpContext.Current.User;
        String name = p.Name;
        bool b = p.IsInRole("Admins");

    </pre>
    </form>
</body>
</html>
