# App.HttpApi


## 1.说明

* (01) 一种轻量的提供数据接口的框架，可作为 WebAPI 的替代方案。
* (02) 可将类中的方法暴露为http接口，如：
```
http://.../HttpApi/TypeName/Method?p1=x&p2=x
```
* (03) 可将页面类中的方法暴露为http接口，如：
```
http://.../Page1.aspx/GetData?page=1&rows=2&sort=abc&order=desc
http://.../Handler1.ashx/GetData?page=1&rows=2&sort=abc&order=desc
```
* (04) 自动生成客户端调用脚本
```
http://.../HttpApi/TypeName/js
```
* (05) 自动生成API清单、API接口测试页面
```
HttpApi/TypeName/api
HttpApi/TypeName/apis
HttpApi/TypeName/Method$
```

* (06) 带缓存机制：可指定方法返回值的缓存时间、方式; 客户端可控强制刷新缓存。
* (07) 带鉴权机制：访问IP、动作、 是否登录、用户名、角色、Token。可自定义接口鉴权逻辑。
* (08) 带封装机制：可将方法返回值自动包裹为 APIResult 结构体。
* (09）可配置输出格式：枚举、递进、日期、长数字、错误时的输出方式等。
* (10）服务器端和客户端都可指定接口返回的数据格式，如 text, xml，json, file, image, base64image 等。
* (11）支持可空数据类型参数、默认参数。

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
  formatEnum="Text"                      // 枚举输出格式: Text | Int
  formatIndented="Indented"              // json加空格换行递进格式化后输出
  formatDateTime="yyyy-MM-dd"            // 时间类型输出格式
  formatLowCamel="false"                 // 是否用小字母开头驼峰方式输出
  formatLongNumber="Int64,Decimal"       // 长数字输出为字符串，避免客户端js因为精度问题出错
  errorResponse="APIResult"              // 错误时的输出：APIResult | HttpError
  apiTypePrefix="App."                   // 可省略的API前缀，如原始路径为 /HttpAPI/App.Base/Demo 可简化为 /HttpApi/Base/Demo
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

### (3) 自动生成 Api 列表、详情及测试页面
```
http://..../HttpApi/Demo/api
http://..../HttpApi/Demo/HelloWorld$
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


### （4） 缓存控制
```
[HttpApi("输出系统时间", CacheSeconds=30)]
public DateTime GetTime()
{
    return System.DateTime.Now;
}
```

调用时数据将会缓存30秒再刷新：
```
/HttpAPI/Common/GetTime
```

如果需要强制刷新缓存，可增加一个_refresh参数，常用于接口调测用。如:
```
/HttpAPI/Common/GetTime?_refresh=true
```


### (5) 输出类型数据类型控制
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

客户端指定输出类型为xml
```
http://...../HttpApi/Demo/HelloWorld?_type=xml
```

该接口将输出 XML：
```
<APIResult>
    <Result>True</Result>
    <Info>获取成功</Info>
    <CreateDt>2019-07-16 10:26:30</CreateDt>
    <Data>Hello world!</Data>
    <Extra/>
</APIResult>
```

### （6） 访问鉴权控制

#### 常见的数据接口安全性策略及HttpAPI解决方案

- 用 Https 传输接口数据：从网络层上着手，避免交互数据被监听、篡改。
- 完全公开的接口：这种接口无需任何鉴权即可调用。这种方式现在很少见了，仅用于内部系统。
- 安全参数保护的接口：访问者必须事先和接口提供网站约定安全参数，访问者调用接口时必须附上该安全参数。HttpAPI可用 AuthToken 方式实现。
- 动态授权访问的接口：是方法2的升级版本，此时的安全参数是动态分配且有时间限制的（通常由appid+appsecret+timestamp生成，也就是常见的oauth token机制）。HttpAPI可用 AuthToken 方式实现。
- 需要登陆访问的接口：如获取自己的订单，该类接口需要先登陆生成cookie验票（包含用户及角色信息），访问此类接口需带上cookie，服务器端解析该cookie以判断当前访问者的登陆状态、名称、角色等信息。HttpApi可用AuthLogin、AuthUser、AuthRole方式实现。
- 其它限制：如访问IP、访问频率、访问动作等HttpAPI都有相应处理方法。


#### HttpAPI支持以下标签来控制接口访问鉴权

```
[HttpApi("...", AuthVerbs="Get,Post")]      // 校验访问动作
[HttpApi("...", AuthLogin=true)]            // 校验登陆状态
[HttpApi("...", AuthUsers="A,B")]           // 校验登陆用户名
[HttpApi("...", AuthRoles="A,B")]           // 校验登陆用户角色
[HttpApi("...", AuthIP=true)]               // 校验IP
[HttpApi("...", AuthToken=true)]            // 校验Token
```

#### 登陆状态、用户名、角色的鉴权
```
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
```



#### AuthToken 及 AuthIP 的实现

出于灵活性和统一性考虑，这两种方法需要编写自定义访问鉴权代码（如从数据库中获取授权IP和token进行校对），示例代码如下：

```
public class Global : System.Web.HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        // HttpApi 自定义访问校验
        HttpApiConfig.Instance.OnAuth += (ctx, method, attr, ip, token) =>
        {
            Debug.WriteLine(string.Format("IP={0}, User={1}, Token={2}, Method={3}.{4}, AuthIP={5}, AuthToken={6}, AuthLogin={7}, AuthUsers={8}, AuthRoles={9}",
                ip,
                ctx.User?.Identity.Name,
                securityCode,
                method.DeclaringType.FullName,
                method.Name,
                attr.AuthIP,
                attr.AuthToken,
                attr.AuthLogin,
                attr.AuthUsers,
                attr.AuthRoles
                ));
            if (attr.AuthIP && !CheckIP(ip))
                throw new HttpApiException("该IP禁止访问本接口", 401);
            if (attr.AuthToken && !CheckToken(token))
                throw new HttpApiException("请核对授权token", 401);
            // 其它自定义的鉴权逻辑，如访问频率等。如果鉴权失败，抛出HttpApiException即可。
        };
    }
}
```


### （7） 统一的接口数据格式 APIResult

我们常常将接口吐出的数据统一格式，便于客户端调用，HttpAPI中内置了APIResult结构体，可在输出时指定。

```
[HttpApi("输出系统时间")]
public APIResult GetTime()
{
    return new APIResult(true, "操作成功", System.DateTime.Now);
}
```

输出格式为
```
{
    Result: true,
    Info: "操作成功",
    CreateDt: "2019-07-16 10:24:14",
    Data: '2019-01-01',
    Extra: {...}
}
```


### （8） 更多可控参数
```
/// <summary>描述信息</summary>
public string Description { get; set; }

/// <summary>示例</summary>
public string Example { get; set; }

/// <summary>备注</summary>
public string Remark { get; set; }

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

[HttpApi("返回APIResult对象")]
public static APIResult GetPersons()
{
    var persons = new List<Person>(){
        new Person(){ Name="Kevin", Sex=Sex.Male, Birth=new DateTime(2000, 01, 01)},
        new Person(){ Name="Cherry", Sex=Sex.Female, Birth=new DateTime(2010, 01, 01)}
    };
    return new APIResult(true, "", persons);
}
```      

## 7.项目目标
- 立项初衷：（1）简化服务器端接口开发代码量；（2）自动完成客户端js代码，减少出错率；
- 后来又想集成鉴权、缓存、输出格式、错误控制、统一输出结构等逻辑；
- WebAPI 有众多限制：http://blog.csdn.net/leeyue_1982/article/details/51305950
- Restful 方式的API动作过少（GET/POST/DELETE/)，无法覆盖到所有动作，干脆放开方法名，让开发者自己定义好了
- WebAPI 要想实现我的目标，有很大的代码工作量，故全新开发本框架。


## 8.History
- 2012-08  初版
- 2014-06  支持默认参数；增加问授权（角色、用户、登录）；错误输出可控（APIResult 或 HTTP ERROR）
- 2016-06  增加api展示窗口，修正Image方式输出故障
- 2017-11  简化和优化 HttpApiAttribute，可选缓存方式
- 2017-12  Nuget发布：install-package App.HttpApi，增加 HttpApiConfig 配置节
- 2018-10  增加自定义鉴权事件；实现Api展示页面；用配置节控制Json输出格式；简化访问路径；完善xml输出
- 2018-11  默认参数可为空也可不填写；可空类型参数可为空也可不填写；可在api介绍页面上输出枚举类型成员信息；
- 2019-03  Api 测试页面（填写参数；选择方法Get/Post；发送请求；显示输出结果）
- 2019-06  客户端可控强制刷新缓存（url参数中增加 _refresh=true）
- 2019-07  长数字类型可控输出为文本，避免客户端js因为精度问题导致的各种错误。


## 9.任务
- XML 格式控制：属性/成员、递进、大小写等
- 写一个动态token的接口调用示例

## 10.参考
- http://www.cnblogs.com/wzcheng/archive/2010/05/20/1739810.html


## 11.截图

![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Apis?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Api?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/Auth?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/ExportJson.png?raw=true)
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/ExportXml.png?raw=true)


