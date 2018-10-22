using App.HttpApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Test
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // HttpApi 自定义访问校验（安全码存在 QueryString["securityCode"] 中)
            HttpApiConfig.Instance.OnAuth += (ctx, method, attr, ip, securityCode) =>
            {
                Debug.WriteLine(string.Format("IP={0}, User={1}, SecurityCode={2}, Method={3}.{4}, AuthIP={5}, AuthSecurityCode={6}, AuthLogin={7}, AuthUsers={8}, AuthRoles={9}",
                    ip,
                    ctx.User?.Identity.Name,
                    securityCode,
                    method.DeclaringType.FullName,
                    method.Name,
                    attr.AuthIP,
                    attr.AuthSecurityCode,
                    attr.AuthLogin,
                    attr.AuthUsers,
                    attr.AuthRoles
                    ));
                return null;
            };

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}