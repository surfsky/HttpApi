<%@ WebHandler Language="C#" Class="App.TestHandler" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using App.HttpApi;

namespace App
{
    public class TestHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context)
        {
            HttpContext.Current.Session["info"] = "hello";
            context.Response.Write(HttpContext.Current.Session["info"] as string);
        }


        [HttpApi(Type = ResponseType.JSON)]
        public static object GetStaticObject()
        {
            return new { h="0", a = "1", b = "2", c="3"};
        }
    }
}


