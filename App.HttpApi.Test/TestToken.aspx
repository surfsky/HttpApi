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
    <title>动态 Token 示例（未完成）</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link href="/res/css/site.css" rel="stylesheet" />
    <script type="text/javascript" src="/HttpApi/Demo/js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label runat="server" ID="lblInfo"  Text="当前用户信息" />
        <ul>
            <li><a id="btn4" onclick="App.Demo.Login(function (data) { }, 'btn4')" class="btn">登录</a></li>
            <li><a id="btn5" onclick="App.Demo.Logout(function (data) { }, 'btn5')" class="btn">注销</a></li>
        </ul>
        <ul>
            <li><a id="btn1" onclick="App.Demo.LimitLogin(function (data) { }, 'btn1')" class="btn">登录后才可以调用</a></li>
            <li><a id="btn2" onclick="App.Demo.LimitUser(function (data) { }, 'btn2');" class="btn">指定用户（Kevin）才可以调用</a></li>
            <li><a id="btn3" onclick="App.Demo.LimitRole(function (data) { }, 'btn3');" class="btn">指定角色（Admins）才可以调用</a></li>
        </ul>
        <br />
        <br />
        <asp:Button runat="server" ID="btnLogin" Text="登录为surfsky" OnClick="btnLogin_Click" class="btn" />
        <asp:Button runat="server" ID="btnLogin2" Text="登录为kevin" OnClick="btnLogin2_Click" class="btn" />
        &nbsp;<asp:Button runat="server" ID="btnLogout" Text="注销" OnClick="btnLogout_Click" class="btn" />
    </div>

    </form>
</body>
</html>
