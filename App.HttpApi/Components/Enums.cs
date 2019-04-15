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
        HttpError = 0,
        DataResult = 1
    }

    /// <summary>
    /// 枚举输出方式
    /// </summary>
    public enum EnumFomatting
    {
        Text = 0,
        Int = 1
    }

    /// <summary>
    /// API 状态
    /// </summary>
    public enum ApiStatus : int
    {
        Published = 0,
        Testing = 1,
        Deprecated = 2
    }
}
