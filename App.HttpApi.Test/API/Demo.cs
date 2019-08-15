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

        [HttpApi("默认方法参数示例", Remark = "p2的默认值为a", Status = ApiStatus.Deprecated, AuthVerbs ="GET")]
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

        //---------------------------------------------
        // 返回各种基础对象
        //---------------------------------------------
        [HttpApi("plist文件下载示例", CacheSeconds = 30, MimeType="text/plist", FileName="app.plist")]
        public string GetFile(string info)
        {
            System.Threading.Thread.Sleep(200);
            return string.Format("This is plist file demo! {0} {1}", info, DateTime.Now);
        }

        [HttpApi("输出系统时间", CacheSeconds=30)]
        public DateTime GetTime()
        {
            return System.DateTime.Now;
        }

        [HttpApi("输出DataTable")]
        public DataTable GetDataTable()
        {
            DataTable dt = new DataTable("test");
            dt.Columns.Add("column1");
            dt.Columns.Add("column2");
            dt.Rows.Add("a1", "b1");
            dt.Rows.Add("a2", "b2");
            return dt;
        }

        [HttpApi("输出DataRow")]
        public DataRow GetDataRow()
        {
            DataTable dt = new DataTable("test");
            dt.Columns.Add("column1");
            dt.Columns.Add("column2");
            dt.Rows.Add("a1", "b1");
            dt.Rows.Add("a2", "b2");
            return dt.Rows[0];
        }

        [HttpApi("输出Dictionary")]
        public IDictionary GetDictionary()
        {
            var dict = new Dictionary<int, Person>();
            dict.Add(0, new Person() { Name = "Marry" });
            dict.Add(1, new Person() { Name = "Cherry" });
            return dict;
        }

        [HttpApi("输出图像", CacheSeconds=60)]
        public Image GetImage(string text)
        {
            Bitmap bmp = new Bitmap(200, 200);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(
                text, 
                new Font("Arial", 16, FontStyle.Bold), 
                new SolidBrush(Color.FromArgb(255, 206, 97)), 
                new PointF(5, 5)
                );
            return bmp;
        }


        //---------------------------------------------
        // 自定义类
        //---------------------------------------------
        [HttpApi("Complex type parameter")]
        [Param("father", "Father，如：{Name:'Kevin', Birth:'1979-12-01', Sex:0}")]
        public Person CreateGirl(Person father)
        {
            return new Person()
            {
                Name = father?.Name + "'s dear daughter",
                Birth = System.DateTime.Now,
                Sex = Sex.Female,
                Father = father
            };
        }

        [HttpApi("null值处理")]
        public static Person CreateNull()
        {
            return null;
        }

        [HttpApi("返回复杂对象")]
        public static Person GetPerson()
        {
            return new Person() { Name = "Cherry" };
        }


        [HttpApi("返回Xml对象", Type=ResponseType.XML)]
        public static Person GetPersonXml()
        {
            return new Person() { Name = "Cherry" };
        }

        [HttpApi("返回复杂对象，并用APIResult进行封装", Wrap =true)]
        public static Person GetPersonData()
        {
            return new Person() { Name = "Kevin" };
        }

        [HttpApi("返回APIResult对象")]
        public static APIResult GetPersons()
        {
            var persons = new List<Person>(){
                new Person(){ Name="Kevin", Sex=Sex.Male, Birth=new DateTime(2000, 01, 01)},
                new Person(){ Name="Cherry", Sex=Sex.Female, Birth=new DateTime(2010, 01, 01)}
            };
            return new APIResult(true, "", persons);
        }
    }
}
