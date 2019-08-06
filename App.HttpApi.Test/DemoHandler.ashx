<%@ WebHandler Language="C#" Class="App.DemoHandler" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.HttpApi;

namespace App
{
    public class DemoHandler : HttpApiHandlerBase
    {
        [HttpApi(Type = ResponseType.JSON)]
        public static object GetStaticObject()
        {
            return new { h="0", a = "1", b = "2", c="3"};
        }
    }
}


