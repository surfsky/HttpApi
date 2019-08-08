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
        // 控制访问权限
        //---------------------------------------------
        [HttpApi("获取Token")]
        public string GetToken(string appKey, string appSecret)
        {
            var now = DateTime.Now;
            var o = new Token(appKey, appSecret, now.ToTimeStamp(), now.AddDays(1));
            return o.ToString().DesEncrypt("12345678");
        }


        [HttpApi("登录")]
        public string Login()
        {
            AuthHelper.Login("Admin", new string[] { "Admins" }, DateTime.Now.AddDays(1));
            System.Threading.Thread.Sleep(200);
            return "访问成功（已登录）";
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
