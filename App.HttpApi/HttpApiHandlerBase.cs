using System.Web;

namespace App.HttpApi
{
    /// <summary>
    /// 继承至该类的HttpHandler都有提供Web方法调用的能力
    /// </summary>
    public class HttpApiHandlerBase : IHttpHandler
    {
        public bool IsReusable { get { return false; } }
        public void ProcessRequest(HttpContext context)
        {
            HttpApiHelper.ProcessRequest(context, this);
        }
    }
}