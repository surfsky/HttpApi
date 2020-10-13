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
using System.IO;

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
        [HttpApi("Group2", "HelloWorld")]
        public static string HelloWorld(string info)
        {
            System.Threading.Thread.Sleep(200);
            return string.Format("Hello world! {0} {1}", info, DateTime.Now);
        }

        [HttpApi("Group1", "TestSession")]
        public static string TestSession(string info)
        {
            HttpContext.Current.Session["info"] = info;
            return HttpContext.Current.Session["info"] as string;
        }

        [HttpApi("Group1", "静态方法示例", Type = ResponseType.JSON)]
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


        [HttpApi("Upload file", PostFile=true)]
        [HttpParam("filePath", "file path, eg. Article")]
        [HttpParam("fileName", "file name, eg. a.png")]
        public APIResult Up(string filePath, string fileName)
        {
            var exts = new List<string> { ".jpg", ".png", ".gif", ".mp3", ".mp4" };
            var ext = fileName.GetFileExtension();
            if (!exts.Contains(ext))
                return new APIResult(false, "File deny", 13);

            // 构造存储路径
            var url = GetUploadPath(filePath, fileName);
            var path = Asp.MapPath(url);
            var fi = new FileInfo(path);
            if (!fi.Directory.Exists)
                Directory.CreateDirectory(fi.Directory.FullName);

            // 存储第一个文件
            var files = Asp.Request.Files;
            if (HttpContext.Current.Request.Files.Count == 0)
                return new APIResult(false, "File doesn't exist", 11);
            Asp.Request.Files[0].SaveAs(path);
            return new APIResult(true, url);
        }

        /// <summary>获取上传文件要保存的虚拟路径</summary>
        public static string GetUploadPath(string folderName, string fileName = ".png")
        {
            // 默认保存在 /Files/ 目录下
            string folder = string.Format("~/Files/{0}", folderName);

            // 如果 folderName 以/开头，则保存在 folderName 目录下
            if (folderName != null && folderName.StartsWith("/"))
                folder = folderName;

            // 合并目录和文件名
            string extension = fileName.GetFileExtension();
            string path = string.Format("{0}/{1}{2}", folder, new SnowflakeID().NewID(), extension);
            return Asp.ResolveUrl(path);
        }
    }
}
