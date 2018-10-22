using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace App.HttpApi
{
    /// <summary>
    /// 标准数据返回结果
    /// </summary>
    public class DataResult
    {
        /// <summary>结果（字符串类型）</summary>
        public String Result { get; set; }

        /// <summary>详细信息（文本类型，一些说明性的文字）</summary>
        public String Info { get; set; }

        /// <summary>详细数据（自定义类型，可为数组、对象）</summary>
        public object Data { get; set; }

        /// <summary>附加数据（自定义类型，如分页信息DataPager）</summary>
        public object Extra { get; set; }

        public DataResult(object result, String info="", object data=null, object extra=null)
        {
            Result = ToString(result);
            Info = info;
            Data = data;
            Extra = extra;
        }

        // 转化为字符串输出
        string ToString(object o)
        {
            if (o == null)
                return "";
            if (o is bool)
                return o.ToString().ToLower();
            if (o.GetType().IsEnum)
            {
                if (HttpApiConfig.Instance.FormatEnum == EnumFomatting.Int)
                    return ((int)o).ToString();
            }
            return o.ToString();
        }
    }

    /// <summary>
    /// 数据分页描述信息
    /// </summary>
    public class DataPager
    {
        /// <summary>总记录数</summary>
        public int Total { get; set; }

        /// <summary>分页大小</summary>
        public int PageSize { get; set; }

        /// <summary>总页数</summary>
        public int PageCount { get; set; }

        /// <summary>当前页</summary>
        public int PageIndex { get; set; }


        public DataPager(int total, int pageSize, int pageIndex)
        {
            Total = total;
            PageSize = pageSize;
            int pageCount = Convert.ToInt32(Math.Ceiling((double)Total / (double)PageSize));
            PageCount = pageCount < 1 ? 1 : pageCount;
            PageIndex = pageIndex;
        }

    }
}
