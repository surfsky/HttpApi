using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
//using System.Web.UI;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using Microsoft.AspNetCore.Http;
using System.DrawingCore.Imaging;
using System.DrawingCore;

namespace App.HttpApi
{
    /// <summary>
    /// 获取Web内嵌资源的辅助函数
    /// 创建内嵌资源
    /// （1）将内嵌资源的编译类型改为：嵌入的资源（enbeded resource）
    /// （2）在Assembly.cs中注册该资源，如：
    ///     [Assembly: WebResource("SampleProject.Sample.jpg", "image/png")]
    ///     [Assembly: WebResource("SampleProject.SamplePicture.png", "image/png")]
    ///     [assembly: WebResource("SampleProject.Help.htm", "text/html")]
    ///     [assembly: WebResource("SampleProject.MyStyleSheet.css", "text/css")]
    ///     [assembly: WebResource("SampleProject.smallFail.gif", "image/gif")]
    ///     [assembly: WebResource("SampleProject.smallSuccess.gif", "image/gif")]
    ///     [assembly: WebResource("SampleProject.MyScript.js", "text/javascript", PerformSubstitution = true)]
    /// （3）使用内嵌资源，如：
    ///     image1.ImageUrl = GetResourceUrl("SampleProject.Sample.jpg");
    ///     RegistCss("SampleProject.MyStyleSheet.css");
    ///     RegistScript("SampleProject.MyScript.js");
    /// </summary>
    internal class ResourceHelper
    {
        /// <summary>获取数据集中的资源流</summary>
        /// <param name="assembly">数据集</param>
        /// <param name="resourceName">资源名称</param>
        /// <param name="caseSensitive">是否大小写敏感</param>
        /// <returns></returns>
        public static Stream GetResource(Assembly assembly, string resourceName, bool caseSensitive = true)
        {
            if (caseSensitive)
                return assembly.GetManifestResourceStream(resourceName);
            else
            {
                resourceName = resourceName.ToLower();
                foreach (string name in assembly.GetManifestResourceNames())
                    if (resourceName == name.ToLower())
                        return assembly.GetManifestResourceStream(resourceName);
                return null;
            }
        }

        public static string GetResourceText(Assembly assembly, string resourceName, bool caseSensitive = true)
        {
            Stream stream = ResourceHelper.GetResource(assembly, resourceName, caseSensitive);
            if (stream == null) return "";
            else
            {
                byte[] buffer = new byte[stream.Length];
                int len = stream.Read(buffer, 0, (int)stream.Length);
                string temp = Encoding.UTF8.GetString(buffer, 0, len);
                return temp;
            }
        }

        /// <summary>输出图像资源</summary>
        /// <param name="response"></param>
        /// <param name="assembly"></param>
        /// <param name="resourceName"></param>
        /// <param name="type">jpg, png, gif, etc</param>
        /// <param name="caseSensitive"></param>
        public static void RenderImage(HttpResponse response, Assembly assembly, string resourceName)
        {
            RenderImage(response, assembly, resourceName, ImageFormat.Jpeg, true);
        }
        public static void RenderImage(HttpResponse response, Assembly assembly, string resourceName, ImageFormat type, bool caseSensitive = true)
        {
            response.ContentType = "image/" + type.ToString().ToLower();
            Stream s = GetResource(assembly, resourceName, caseSensitive);
            if (s != null)
                using (s)
                    using (Image image = Image.FromStream(s))
                        image.Save(response.Body, type);
        }

        /// <summary>输出文本资源</summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="resourceName">资源名称。如Kingsow.Web.Handlers.WebHandlers.Help.txt</param>
        /// <param name="type">plain, css, html, xml, javascript</param>
        public static void RenderText(HttpResponse response, Assembly assembly, string resourceName)
        {
            RenderText(response, assembly, resourceName, Encoding.Default, "plain", true);
        }
        public static void RenderText(HttpResponse response, Assembly assembly, string resourceName, Encoding encode, string type = "plain", bool caseSensitive = true)
        {
            response.ContentType = "text/" + type;
            Stream s = GetResource(assembly, resourceName, caseSensitive);
            if (s != null)
                using (s)
                {
                    byte[] buffer = new byte[s.Length];
                    int len = s.Read(buffer, 0, (int)s.Length);
                    string temp = encode.GetString(buffer, 0, len);
                    response.WriteAsync(temp);
                }
        }

        /// <summary>输出二进制资源</summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="resourceName">资源名称。如Kingsow.Web.Handlers.WebHandlers.Help.txt</param>
        /// <param name="type"></param>
        public static void RenderBinary(HttpResponse response, Assembly assembly, string resourceName, string type = "pdf", bool caseSensitive = true)
        {
            response.ContentType = "application/" + type;
            Stream s = GetResource(assembly, resourceName, caseSensitive);
            if (s != null)
                using (s)
                {
                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    //response.BinaryWrite(buffer);
                    //response.Flush();
                    //response.End();
                    response.Body.Write(buffer, 0, buffer.Length);
                    response.Body.Flush();
                    response.Body.Close();
                }
        }


    }
}
