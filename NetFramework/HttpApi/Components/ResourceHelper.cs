using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace App.HttpApi
{
    /// <summary>
    /// ��ȡWeb��Ƕ��Դ�ĸ�������
    /// ������Ƕ��Դ
    /// ��1������Ƕ��Դ�ı������͸�Ϊ��Ƕ�����Դ��enbeded resource��
    /// ��2����Assembly.cs��ע�����Դ���磺
    ///     [Assembly: WebResource("SampleProject.Sample.jpg", "image/png")]
    ///     [Assembly: WebResource("SampleProject.SamplePicture.png", "image/png")]
    ///     [assembly: WebResource("SampleProject.Help.htm", "text/html")]
    ///     [assembly: WebResource("SampleProject.MyStyleSheet.css", "text/css")]
    ///     [assembly: WebResource("SampleProject.smallFail.gif", "image/gif")]
    ///     [assembly: WebResource("SampleProject.smallSuccess.gif", "image/gif")]
    ///     [assembly: WebResource("SampleProject.MyScript.js", "text/javascript", PerformSubstitution = true)]
    /// ��3��ʹ����Ƕ��Դ���磺
    ///     image1.ImageUrl = GetResourceUrl("SampleProject.Sample.jpg");
    ///     RegistCss("SampleProject.MyStyleSheet.css");
    ///     RegistScript("SampleProject.MyScript.js");
    /// </summary>
    internal class ResourceHelper
    {
        /// <summary>��ȡ���ݼ��е���Դ��</summary>
        /// <param name="assembly">���ݼ�</param>
        /// <param name="resourceName">��Դ����</param>
        /// <param name="caseSensitive">�Ƿ��Сд����</param>
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

        /// <summary>���ͼ����Դ</summary>
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
                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(s))
                        image.Save(response.OutputStream, type);
        }

        /// <summary>����ı���Դ</summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="resourceName">��Դ���ơ���Kingsow.Web.Handlers.WebHandlers.Help.txt</param>
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
                    response.Write(temp);
                }
        }

        /// <summary>�����������Դ</summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="resourceName">��Դ���ơ���Kingsow.Web.Handlers.WebHandlers.Help.txt</param>
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
                    response.BinaryWrite(buffer);
                    response.Flush();
                    response.End();
                }
        }


        //----------------------------------------------------------------------
        // ASP.NET 2.0 ��WebResource.axd��װ�����ṩ��Դ��url��ַ
        //----------------------------------------------------------------------
        /// <summary>��ȡ��Դurl</summary>
        /// <param name="resourceName"></param>
        /// <returns>
        /// ���ƣ�WebResource.axd?a=pWebCtrl&amp;r=WebCtrl.cutecat.jpg&amp;t=632390947985312500
        /// a  - assembly
        /// r  - resourceName
        /// t  - assembly's timeStamp
        /// </returns>
        public string GetResourceUrl(string resourceName)
        {
            return (HttpContext.Current.Handler as Page).ClientScript.GetWebResourceUrl(this.GetType(), resourceName);
        }

        /// <summary>����Դע��Ϊcss</summary>
        /// <param name="resourceName"></param>
        public void RegistCss(string resourceName)
        {
            Page page = HttpContext.Current.Handler as Page;
            if (page != null)
            {
                string url = page.ClientScript.GetWebResourceUrl(this.GetType(), resourceName);
                string text = string.Format("<link rel='stylesheet' text='text/css' href='{0}' />", url);
                LiteralControl lc = new LiteralControl(text);
                page.Header.Controls.Add(lc);
            }
        }

        /// <summary>����Դע��Ϊscript</summary>
        /// <param name="resourceName"></param>
        public void RegistScript(string resourceName)
        {
            Page page = HttpContext.Current.Handler as Page;
            if (page != null)
            {
                string url = page.ClientScript.GetWebResourceUrl(this.GetType(), resourceName);
                page.ClientScript.RegisterClientScriptInclude(resourceName, url);
            }
        }
    }
}
