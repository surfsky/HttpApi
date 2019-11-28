using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Drawing;
using App.HttpApi;
using System.ComponentModel;
using App.Core;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Web.Script.Serialization;
using System.Collections;

namespace App
{
    public partial class Demo
    {

        //---------------------------------------------
        // Token 机制
        // 1. 获取动态token
        // 2. 访问需要健权的接口，带上token参数
        // 3. 服务器端统一在 global 里面做token校验
        //---------------------------------------------
        [HttpApi("获取Token")]
        public string GetToken(string appKey, string appSecret)
        {
            return Token.Create(appKey, appSecret, 1);
        }


        [HttpApi("NeedTokenApi", AuthToken=true)]
        public string GetData()
        {
            var now = DateTime.Now;
            return now.ToString();
        }



        //---------------------------------------------
        // 控制访问权限
        //---------------------------------------------
        [HttpApi("User login", AuthTraffic=1)]
        public string Login()
        {
            AuthHelper.Login("Admin", new string[] { "Admins" }, DateTime.Now.AddDays(1));
            System.Threading.Thread.Sleep(200);
            return "Login success";
        }
 
        [HttpApi("注销")]
        public string Logout()
        {
            AuthHelper.Logout();
            System.Threading.Thread.Sleep(200);
            return "注销成功";
        }


        [HttpApi("用户必须登录后才能访问该接口，若无授权则返回401错误", AuthLogin=true)]
        public string LimitLogin()
        {
            System.Threading.Thread.Sleep(200);
            return "访问成功（已登录）";
        }

        [HttpApi("限制用户访问，若无授权则返回401错误", AuthUsers = "Admin,Kevin")]
        public string LimitUser()
        {
            System.Threading.Thread.Sleep(200);
            return "访问成功（限制用户Admin,Kevin）";
        }

        [HttpApi("限制角色访问，若无授权则返回401错误", AuthRoles = "Admins")]
        public string LimitRole()
        {
            System.Threading.Thread.Sleep(200);
            return "访问成功（限制角色Admins）";
        }
        
    }
}
