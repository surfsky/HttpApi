using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
//using System.Drawing;
using App.HttpApi;
using System.ComponentModel;
//using App.Core;
//using Newtonsoft.Json;
//using System.Xml.Serialization;
//using System.Web.Script.Serialization;
using System.Collections;
using App.Utils;
using System.DrawingCore;

namespace App
{
    public partial class Demo
    {
        //---------------------------------------------
        // 返回各种基础对象
        //---------------------------------------------
        [HttpApi("plist文件下载示例", CacheSeconds = 30, MimeType = "text/plist", FileName = "app.plist")]
        public string GetFile(string info)
        {
            System.Threading.Thread.Sleep(200);
            return string.Format("This is plist file demo! {0} {1}", info, DateTime.Now);
        }

        [HttpApi("输出系统时间", CacheSeconds = 30)]
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

        [HttpApi("输出图像", CacheSeconds = 60)]
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

    }
}
