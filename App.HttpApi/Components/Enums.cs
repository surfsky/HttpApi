using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.HttpApi
{
    /// <summary>
    /// 错误时返回方式
    /// </summary>
    public enum ErrorResponse
    {
        /// <summary>输出Http错误</summary>
        HttpError = 0,
        /// <summary>输出APIResult结构</summary>
        APIResult = 1
    }

    /// <summary>
    /// 枚举输出方式
    /// </summary>
    public enum EnumFomatting
    {
        /// <summary>输出字符串</summary>
        Text = 0,
        /// <summary>输出整型</summary>
        Int = 1
    }

    /// <summary>
    /// API 状态
    /// </summary>
    public enum ApiStatus : int
    {
        /// <summary>正式发布</summary>
        Published = 0,
        /// <summary>测试接口</summary>
        Testing = 1,
        /// <summary>该接口已废弃，随时可能被删除</summary>
        Deprecated = 2
    }
}
