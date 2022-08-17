using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace App.HttpApiCore
{
    /// <summary>
    /// 认证及授权中间件
    /// </summary>
    public static class AuthMiddleware
    {
        /// <summary>登录（并保存信息到验票）</summary>
        public static void SignIn(HttpContext context, long id, string name, string email, string mobile, int keepDays=14)
        {
            var identity = new ClaimsIdentity("Cookie");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, name));
            identity.AddClaim(new Claim(ClaimTypes.Email, email));
            identity.AddClaim(new Claim(ClaimTypes.MobilePhone, mobile));
            var principal = new ClaimsPrincipal(identity);
            context.SignInAsync(principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(keepDays)
            });
        }

        /// <summary>注销</summary>
        public static void SignOut(HttpContext context)
        {
            context.SignOutAsync();
        }

        /// <summary>验票验证</summary>
        public static IApplicationBuilder UseAuthorize(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                // 根目录、登录页、注销页、其它页面
                if (context.Request.Path == "/")
                    await next();
                else if (context.Request.Path == "/signin")
                {
                    AuthMiddleware.SignIn(context, 1, "kevin", "kevin@some.com", "12345678");
                    await context.Response.WriteAsync("SignIn ok");
                }
                else if (context.Request.Path == "/signout")
                {
                    AuthMiddleware.SignOut(context);
                    await context.Response.WriteAsync("SignOut ok");
                }
                else
                {
                    var user = context.User;
                    if (user?.Identity?.IsAuthenticated ?? false)
                        await next();
                    else
                        await context.ChallengeAsync();
                }
            });
        }
    }
}
