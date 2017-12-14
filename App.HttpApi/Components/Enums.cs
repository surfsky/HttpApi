using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.HttpApi.Components
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
    /// 枚举返回方式
    /// </summary>
    public enum EnumResponse
    {
        Text = 0,
        Int = 1
    }

    /// <summary>
    /// 包裹类型
    /// </summary>
    public enum WrapType
    {
        None,
        DataResult
    }
}
