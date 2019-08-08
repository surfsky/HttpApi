using App.HttpApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using App.Core;
using App;

namespace Test
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // HttpApi 自定义访问校验
            HttpApiConfig.Instance.OnAuth += (ctx, method, attr, token) =>
            {
                if (attr.AuthToken && !CheckToken(token))
                    throw new HttpApiException(404, "Token failure.");
            };
        }

        /// <summary>检测 API 访问验票</summary>
        private bool CheckToken(string token)
        {
            var o = token.DesDecrypt("12345678").ParseJson<Token>();
            if (o != null && o.ExpireDt > DateTime.Now)
                return true;
            return false;
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