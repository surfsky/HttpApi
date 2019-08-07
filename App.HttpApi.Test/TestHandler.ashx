<%@ WebHandler Language="C#" Class="App.TestHandler" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.HttpApi;

namespace App
{
    public class TestHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }
        public void ProcessRequest(HttpContext context) { }


        [HttpApi(Type = ResponseType.JSON)]
        public static object GetStaticObject()
        {
            return new { h="0", a = "1", b = "2", c="3"};
        }
    }
}


