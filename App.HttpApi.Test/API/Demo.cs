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
    [Description("HttpApi Demo")]
    [Script(CacheDuration =0, ClassName ="Demo", NameSpace ="App")]
    [History("2016-11-01", "SURFSKY", "History log1")]
    [History("2019-08-15", "SURFSKY", "Fix token")]
    public partial class Demo
    {
        //---------------------------------------------
        // 静态方法
        //---------------------------------------------
        [HttpApi("HelloWorld")]
        public static string HelloWorld(string info)
        {
            System.Threading.Thread.Sleep(200);
            return string.Format("Hello world! {0} {1}", info, DateTime.Now);
        }

        [HttpApi("TestSession")]
        public static string TestSession(string info)
        {
            HttpContext.Current.Session["info"] = info;
            return HttpContext.Current.Session["info"] as string;
        }

        [HttpApi("静态方法示例", Type = ResponseType.JSON)]
        public static object GetStaticObject()
        {
            return new { h = "3", a = "1", b = "2", c = "3" };
        }

        [HttpApi("Json结果包裹器示例", Wrap = true, WrapCondition ="获取数据成功")]
        public static object TestWrap()
        {
            return new { h = "3", a = "1", b = "2", c = "3" };
        }

        [HttpApi("默认方法参数示例", Remark = "p2的默认值为a", Status = ApiStatus.Delete, AuthVerbs ="GET")]
        public static object TestDefaultParameter(string p1, string p2="a")
        {
            return new { p1 = p1, p2 = p2};
        }

        [HttpApi("测试错误")]
        public static object TestError()
        {
            int n = 0;
            int m = 1 / n;
            return true;
        }

        [HttpApi("限制访问方式", AuthVerbs ="Post")]
        public static string TestVerbs()
        {
            return HttpContext.Current.Request.HttpMethod;
        }

        [HttpApi("测试枚举返回值（可在web.config中设置）")]
        public static Sex TestEnum()
        {
            return Sex.Male;
        }

        [HttpApi("测试可空枚举值")]
        public static Sex? GetNullalbeEnum(Sex? sex)
        {
            return sex;
        }

        [HttpApi("测试可空枚举值2")]
        public static Sex? GetNullalbeEnum2(Sex? sex=Sex.Male)
        {
            return sex;
        }

    }
}
