# App.HttpApi



## 1.说明

* 一种轻量的提供数据接口的框架，可作为 WebAPI 的替代方案
* 可将类中的方法暴露为http接口，如：
```
http://.../HttpApi/TypeName/Method?p1=x&p2=x
```
* 可将页面类中的方法暴露为http接口，如：
```
http://.../Page1.aspx/GetData?page=1&rows=2&sort=abc&order=desc
http://.../Handler1.ashx/GetData?page=1&rows=2&sort=abc&order=desc
```
* 自动生成客户端调用脚本
```
http://.../HttpApi/TypeName/js
```
* 自动生成API清单及API接口参数展示页面
```
HttpApi/TypeName/api
HttpApi/TypeName/apis
HttpApi/TypeName/Method_
```

* 带缓存机制：可指定方法返回值的缓存时间、方式
* 带鉴权机制：访问IP、动作、 是否登录、用户名、角色、安全码。可自定义接口鉴权逻辑。
* 带封装机制：可将方法返回值自动包裹为 DataResult 结构体
* 可配置Json输出格式：枚举输出、json递进、日期、错误时的输出方式
* 服务器端和客户端都可指定接口返回的数据格式，如text, xml，json, file, image, base64image 等

## 2.作者
```
http://github.com/surfsky
```

## 3.安装
```
Nuget: install-package App.HttpApi
```

## 4.使用

(1) 引用类库（用nuget安装的话会自动完成）
```
App.Core.dll
App.HttpApi.dll
```
       
(2) 修改 web.config 文件（用nuget安装的话会自动修改）
```
<system.webServer>
  <modules>
    <add name="HttpApiModule" type="App.HttpApi.HttpApiModule" />
  </modules>
</system.webServer>
```
       
(3) 在需要导出HttpApi的方法上写上标注
```
namespace App
{
    public class Demo
    {
        [HttpApi("HelloWorld")]
        public static string HelloWorld(string info)
        {
           System.Threading.Thread.Sleep(200);
           return string.Format("Hello world! {0} {1}", info, DateTime.Now);
        }
    }
}
```
      
(4) 客户端调用
```
http://...../HttpApi/Demo/HelloWorld?info=x
```
        


## 5.高级操作
### （1） 控制 HttpApi 输出
Web.Config
```
<configSections>
  <section name="httpApi" type="App.HttpApi.HttpApiConfig, App.HttpApi"/>
</configSections>
<httpApi 
  formatEnum="Text" 
  formatIndented="Indented" 
  formatDateTime="yyyy-MM-dd" 
  formatLowCamel="false"
  errorResponse="DataResult" 
  apiTypePrefix="App." 
  wrap="" 
  />
```

### （2）自动生成客户端调用的 javascript 脚本
```
<script src="http://.../App/Demo/js"></script>
```
可在类上附上标签，控制生成的脚本内容
```
[Script(CacheDuration =0, ClassName ="Demo", NameSpace ="App")]
```

### (3) 自动生成 Api 介绍页面
```
http://..../HttpApi/Demo/api
http://..../HttpApi/Demo/HelloWorld_
```
可附上标签，显示 Api 修改历史/参数信息/输出类型/缓存等
```
[History("2016-11-01", "SURFSKY", "修改了A")]
public class Demo
{
    [HttpApi("HelloWorld", CacheSeconds=10)]
    [Param("info", "信息")]
    public static string HelloWorld(string info)
    {
        ....
    }
}
```
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Apis.png?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Api.png?raw=true)

### (4) 指定 HttpApi 方法的输出类型
服务器端指定输出类型
```
[HttpApi("...", Type = ResponseType.JSON)]
[HttpApi("...", Type = ResponseType.XML)]
[HttpApi("...", Type = ResponseType.Text)]
[HttpApi("...", Type = ResponseType.Html)]
[HttpApi("...", Type = ResponseType.Javascript)]
[HttpApi("...", Type = ResponseType.Image)]
[HttpApi("...", Type = ResponseType.ImageBase64)]
[HttpApi("...", Type = ResponseType.TextFile,)]
[HttpApi("...", Type = ResponseType.BinaryFile)]
```

客户端指定输出类型
```
http://...../HttpApi/Demo/HelloWorld?info=x&_type=xml
```


### （5） 访问鉴权
由标签控制访问鉴权
```
[HttpApi("...", AuthVerbs="Get,Post")]
[HttpApi("...", AuthLogin=true)]
[HttpApi("...", AuthUsers="A,B")]
[HttpApi("...", AuthRoles="A,B")]
[HttpApi("...", AuthIP=true)]
[HttpApi("...", AuthSecurityCode=true)]
```
出于灵活性考虑，AuthIP和AuthSecurityCode需要编写自定义访问鉴权代码（如从数据库中获取授权IP和安全码进行校对）：
```
public class Global : System.Web.HttpApplication
{

    protected void Application_Start(object sender, EventArgs e)
    {
        // HttpApi 自定义访问校验（安全码存在 Params["securityCode"] 中)
        HttpApiConfig.Instance.OnAuth += (ctx, method, attr, ip, securityCode) =>
        {
            Debug.WriteLine(string.Format("IP={0}, User={1}, SecurityCode={2}, Method={3}.{4}, AuthIP={5}, AuthSecurityCode={6}, AuthLogin={7}, AuthUsers={8}, AuthRoles={9}",
                ip,
                ctx.User?.Identity.Name,
                securityCode,
                method.DeclaringType.FullName,
                method.Name,
                attr.AuthIP,
                attr.AuthSecurityCode,
                attr.AuthLogin,
                attr.AuthUsers,
                attr.AuthRoles
                ));
            return null;
        };
    }
}
```
### （6） 更多可控参数
```
/// <summary>描述信息</summary>
public string Description { get; set; }

/// <summary>示例</summary>
public string Example { get; set; }

/// <summary>备注</summary>
public string Remark { get; set; }

/// <summary>缓存的秒数。默认为0，即没有任何缓存。</summary>
public int CacheSeconds { get; set; } = 0;

/// <summary>缓存位置（默认服务器和客户端都缓存）</summary>
public HttpCacheability CacheLocation { get; set; } = HttpCacheability.ServerAndPrivate;

/// <summary>导出文件的MIME类别</summary>
public string MimeType { get; set; }

/// <summary>导出文件名</summary>
public string FileName { get; set; }

/// <summary>是否对文本类型（Json, Text, Xml, ImageBase64)的数据进行 DataResult 封装</summary>
public bool Wrap { get; set; } = false;

/// <summary>封装条件</summary>
public string WrapCondition { get; set; }

/// <summary>状态（Testing, Published, Deprecated)</summary>
public ApiStatus Status { get; set; }
```

## 6.更多示例
```
[HttpApi("静态方法示例", Type = ResponseType.JS)]
public static object GetStaticObject()
{
    return new { h = "3", a = "1", b = "2", c = "3" };
}

[HttpApi("Json结果包裹器示例", Wrap = true, WrapCondition ="获取数据成功")]
public static object TestWrap()
{
    return new { h = "3", a = "1", b = "2", c = "3" };
}

[HttpApi("默认方法参数示例", Remark = "p2的默认值为a", Status = ApiStatus.Deprecated, AuthVerbs ="GET")]
public static object TestDefaultParameter(string p1, string p2="a")
{
    return new { p1 = p1, p2 = p2};
}

[HttpApi("测试错误")]
public static object TestError()
{
    int n = 0;
    int m = 1 / n;
    return true;
}

[HttpApi("限制访问方式", AuthVerbs ="Post")]
public static string TestVerbs()
{
    return HttpContext.Current.Request.HttpMethod;
}

[HttpApi("测试枚举返回值（可在web.config中设置）")]
public static Sex TestEnum()
{
    return Sex.Male;
}

//---------------------------------------------
// 返回各种基础对象
//---------------------------------------------
[HttpApi("plist文件下载示例", CacheSeconds = 30, MimeType="text/plist", FileName="app.plist")]
public string GetFile(string info)
{
    System.Threading.Thread.Sleep(200);
    return string.Format("This is plist file demo! {0} {1}", info, DateTime.Now);
}

[HttpApi("输出系统时间", CacheSeconds=30)]
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

[HttpApi("输出图像", CacheSeconds=60)]
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

//---------------------------------------------
// 控制访问权限
//---------------------------------------------
[HttpApi("登录")]
public string Login()
{
    AuthHelper.Login("Admin", new string[] { "Admins" }, DateTime.Now.AddDays(1));
    System.Threading.Thread.Sleep(200);
    return "访问成功（已登录）";
}

[HttpApi("注销")]
public string Logout()
{
    AuthHelper.Logout();
    System.Threading.Thread.Sleep(200);
    return "注销成功";
}


[HttpApi("用户必须登录后才能访问该接口，若无授权则返回401错误", AuthLogin=true)]
public string LimitLogin()
{
    System.Threading.Thread.Sleep(200);
    return "访问成功（已登录）";
}

[HttpApi("限制用户访问，若无授权则返回401错误", AuthUsers = "Admin,Kevin")]
public string LimitUser()
{
    System.Threading.Thread.Sleep(200);
    return "访问成功（限制用户Admin,Kevin）";
}

[HttpApi("限制角色访问，若无授权则返回401错误", AuthRoles = "Admins")]
public string LimitRole()
{
    System.Threading.Thread.Sleep(200);
    return "访问成功（限制角色Admins）";
}


//---------------------------------------------
// 自定义类
//---------------------------------------------
[HttpApi("解析自定义类。father:{Name:'Kevin', Birth:'1979-12-01', Sex:0};")]
public Person CreateGirl(Person father)
{
    return new Person()
    {
        Name = father.Name + "'s dear daughter",
        Birth = System.DateTime.Now,
        Sex = Sex.Female,
        Father = father
    };
}

[HttpApi("null值处理")]
public static Person CreateNull()
{
    return null;
}

[HttpApi("返回复杂对象")]
public static Person GetPerson()
{
    return new Person() { Name = "Cherry" };
}


[HttpApi("返回Xml对象", Type=ResponseType.XML)]
public static Person GetPersonXml()
{
    return new Person() { Name = "Cherry" };
}

[HttpApi("返回复杂对象，并用DataResult进行封装", Wrap =true)]
public static Person GetPersonDataResult()
{
    return new Person() { Name = "Kevin" };
}

[HttpApi("返回DataResult对象")]
public static DataResult GetPersons()
{
    var persons = new List<Person>(){
        new Person(){ Name="Kevin", Sex=Sex.Male, Birth=new DateTime(2000, 01, 01)},
        new Person(){ Name="Cherry", Sex=Sex.Female, Birth=new DateTime(2010, 01, 01)}
    };
    return new DataResult(true, "", persons);
}
```      

## 7.History
- 2012-08  初版
- 2014-06  支持默认参数；增加问授权（角色、用户、登录）；错误输出可控（DataResult 或 HTTP ERROR）
- 2016-06  增加api展示窗口，修正Image方式输出故障
- 2017-11  简化和优化 HttpApiAttribute，可选缓存方式
- 2017-12  Nuget发布：install-package App.HttpApi，增加 HttpApiConfig 配置节
- 2018-10  增加自定义鉴权事件；实现Api展示页面；用配置节控制Json输出格式；简化访问路径；完善xml输出
- 2018-11  默认参数可为空也可不填写；可空类型参数可为空也可不填写；可在api介绍页面上输出枚举类型成员信息；

## 8.项目目标
- WebAPI的一些限制：http://blog.csdn.net/leeyue_1982/article/details/51305950
- 这个项目的初衷是简化接口开发，并自动完成客户端js代码的封装，简化服务器端和客户端的开发代码量，减少出错率
- 并想集成鉴权、缓存、输出格式控制等逻辑
- Restful 方式的API动作过少（GET/POST/DELETE/)，无法覆盖到所有动作，干脆放开方法名，让开发者自己定义好了

## 9.任务
- Api 测试页面（填写参数；选择方法Get/Post；发送请求；显示输出结果）
- XML 格式控制：属性/成员、递进、大小写等


## 10.参考
- http://www.cnblogs.com/wzcheng/archive/2010/05/20/1739810.html


## 11.截图

![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Apis?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Api?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Auth?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/ExportJson.png?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/ExportXml.png?raw=true)


