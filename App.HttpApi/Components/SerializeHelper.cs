using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;

namespace App.HttpApi
{
    /// <summary>
    /// 序列化方法类
    /// </summary>
    internal class SerializeHelper
    {
        //----------------------------------------------------
        // 序列化转换
        //----------------------------------------------------
        // 转化为字符串
        public static string ToText(object obj)
        {
            return (obj == null) ? "" : obj.ToString();
        }

        // 转化为json字符串（用 Newtonsoft.Json 序列化）
        public static string ToJson(object obj)
        {
            if (obj == null)
                return "{}";
            else
            {
                var cfg = HttpApiConfig.Instance;

                var settings = new JsonSerializerSettings();
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                // 递进格式
                settings.Formatting = cfg.JsonIndented;

                // 时间格式
                IsoDateTimeConverter datetimeConverter = new IsoDateTimeConverter();
                datetimeConverter.DateTimeFormat = cfg.JsonDateTimeFormat;
                settings.Converters.Add(datetimeConverter);

                // 枚举格式
                StringEnumConverter enumConverter = new StringEnumConverter();
                if (cfg.JsonEnumFormatting == EnumFomatting.Text)
                    settings.Converters.Add(enumConverter);

                //
                return JsonConvert.SerializeObject(obj, settings);
            }
        }

        // 转化为xml
        public static string ToXml(object obj)
        {
            if (obj == null)
                return "";
            else
            {
                MemoryStream stream = new MemoryStream();
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(obj.GetType());
                    xs.Serialize(writer, obj);
                    writer.Close();
                }
                return UnicodeEncoding.UTF8.GetString(stream.GetBuffer());
            }
        }

        // 转化为base64编码的图像字符串
        public static string ToImageBase64(object obj)
        {
            Bitmap img = obj as Bitmap;
            if (img == null)
                return "";
            else
            {
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.GetBuffer();
                string str = "data:image/png;base64," + Convert.ToBase64String(bytes);
                return str;
            }
        }

        // 转化为二进制字节数组
        public static byte[] ToBinary(object obj)
        {
            if (obj == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter ser = new BinaryFormatter();
                ser.Serialize(ms, obj);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }

        // 转化为二进制图像字节数组
        public static byte[] ToImageBytes(object obj)
        {
            Bitmap img = obj as Bitmap;
            if (img == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }

    }
}
