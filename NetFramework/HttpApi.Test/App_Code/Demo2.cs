using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Drawing;
using App.HttpApi;
using System.ComponentModel;

namespace App
{
    /// <summary>
    /// 接口示例类。
    /// 测试释放资源，请实现IDisposable接口
    /// </summary>
    [Description("HttpApi Demo2.  /App_Code/ dynamic class")]
    public class Demo2 : IDisposable
    {
        public void Dispose()
        {
            // Release something
        }

        [HttpApi("HelloWorld", AuthUsers="Admin")]
        public string HelloWorld(string info)
        {
            System.Threading.Thread.Sleep(200);
            return "hello world " + info;
        }

        [HttpApi("静态方法示例", Type = ResponseType.JSON)]
        public static object GetStaticObject()
        {
            return new { h = "3", a = "1", b = "2", c = "3" };
        }

    }
}
