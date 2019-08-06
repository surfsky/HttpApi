using System.Web;
using System.Web.UI;

namespace App.HttpApi
{
    /// <summary>
    /// 辅助类，继承至该类的 Page 都有提供Web方法调用的能力。
    /// 
    /// 经测试，访问xxxx.aspx/method 时若Content-Type:application/json，
    /// 则会被aspnet自带webservice模块截获，无法调用服务器端方法。
    /// 
    /// Aspx页面提供web方法调用的3个解决方案：
    /// （1）客户端post请求，不加上Content-Type参数
    /// （2）服务器端注册自己的httpModule
    /// （3）统一用 App.HttpApi.HttpApiHandler 来实现，手工写js引用
    /// 
    /// 建议统一用方案（3）
    /// 若客户想继承其它Page类时，就没法用这个类了。
    /// </summary>
    public class HttpApiPageBase : Page
    {
        public override void ProcessRequest(HttpContext context)
        {
            bool hasPathInfo = !string.IsNullOrEmpty(context.Request.PathInfo);
            if (hasPathInfo)
            {
                // web方法调用请求
                HttpApiHelper.ProcessRequest(context, this);
            }
            else
            {
                // 普通页面请求（给页面上附加上<script>标签）
                this.Load += (s2, e2) => { RegistJs(this); };
                base.ProcessRequest(context);
            }
        }

        // 向客户端注册<script>标签
        private void RegistJs(Page page)
        {
            if (!page.IsPostBack)
            {
                string url = Request.Path + "/js";
                page.ClientScript.RegisterClientScriptInclude(this.GetType().ToString(), url);
            }
        }
    }
}