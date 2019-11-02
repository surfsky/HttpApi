# App.HttpApi

Chinese readme file is [here](README-CN.md)

## 1.Description

* (01) HttpApi is a convinent framework to provide data by http, It can be the upgrating replacement for WebAPI.
* (02) HttpApi can export class function to http interface, eg.
```
http://.../HttpApi/TypeName/Method?p1=x&p2=x
```
* (03) HttpApi can export page's method to http interface, eg.
```
http://.../Page1.aspx/GetData?page=1&rows=2&sort=abc&order=desc
http://.../Handler1.ashx/GetData?page=1&rows=2&sort=abc&order=desc
```
* (04) HttpApi can auto create client javascript.
```
http://.../HttpApi/TypeName/js
```
* (05) HttpApi can auto create api list page, api test page. eg.
```
HttpApi/TypeName/api
HttpApi/TypeName/apis
HttpApi/TypeName/Method$
```

* (06) Caching: You can assign api result caching duration. And client can refresh cache by '_refresh=true' parameter.
* (07) Auth: IP, Method, LoginStatus, UserName, UserRole, Token, and custom logic.
* (08) Capsule: return standard APIResult object to client.
* (09) Output configuration: You can config output format, such as enum, datetime, long number, error.
* (10) Server site and client can assign api output data format, such as  text, xml, json, file, image, base64image.
* (11) Support nullable and default parameter.

## 2.Author
```
http://github.com/surfsky
```

## 3.Install
```
Nuget: install-package App.HttpApi
```

## 4.Usage

Skip to step 3 if use neget to install httpapi.
(1) Import App.HttpApi.dll      
(2) Modify web.config file
```
<system.webServer>
  <modules>
    <add name="HttpApiModule" type="App.HttpApi.HttpApiModule" />
  </modules>
</system.webServer>
```
       
(3) Modify method, add [HttpApi] Attribute
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
      
(4) Ok, Client can call this api by url:
```
http://...../HttpApi/Demo/HelloWorld?info=x
```
or test api:
```
http://...../HttpApi/Demo/HelloWorld$
```

        


## 5.Senior guidline
### (1) Control HttpApi output format
```
Web.Config
<configSections>
  <section name="httpApi" type="App.HttpApi.HttpApiConfig, App.HttpApi"/>
</configSections>
<httpApi 
  formatEnum="Text"                      // Decide how to export Enum: Text | Int
  formatIndented="Indented"              // Decide whether to beautify json output by and space indent and line break
  formatDateTime="yyyy-MM-dd"            // Decide how to export DateTime
  formatLowCamel="false"                 // Decide whether to use low camel for property name
  formatLongNumber="Int64,Decimal"       // Decide which number type to string, to avoiding javascript number precision error
  errorResponse="APIResult"              // Decide error output when catch exception: APIResult | HttpError
  typePrefix="App."                      // Url abbr support. eg. Raw url /HttpAPI/App.Base/Demo can change to the short path: /HttpApi/Base/Demo
  language="en"                          // Culture support: en, zh-CN
  />
```

### (2) Auto create client javascript
```
<script src="http://.../HttpApi/Demo/js"></script>
```
You can add [Script] attrubute to class, to control the js content:
```
[Script(CacheDuration =0, ClassName ="Demo", NameSpace ="App")]
```

### (3) Auto create api list, api test page.
```
http://..../HttpApi/Demo/api
http://..../HttpApi/Demo/HelloWorld$
```
You can add [History] attribute, to display api modify history.
You can add [Param] attribute, to display api parameter infomation.
```
[History("2016-11-01", "SURFSKY", "modify A")]
public class Demo
{
    [HttpApi("HelloWorld", CacheSeconds=10)]
    [Param("info", "information")]
    public static string HelloWorld(string info)
    {
        ....
    }
}
```


### (4)  Caching
```
[HttpApi("Output system time", CacheSeconds=30)]
public DateTime GetTime()
{
    return System.DateTime.Now;
}
```

The api result will cache 30 seconds:
```
/HttpAPI/Common/GetTime
```

Add _refresh parameter if you want to refresh cache right now. It's useful when testing:
```
/HttpAPI/Common/GetTime?_refresh=true
```


### (5) Control the output data type
*Server site*
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

*Client side*
```
http://...../HttpApi/Demo/HelloWorld?_type=xml
<APIResult>
    <Result>True</Result>
    <Info>获取成功</Info>
    <CreateDt>2019-07-16 10:26:30</CreateDt>
    <Data>Hello world!</Data>
    <Extra/>
</APIResult>
```

### (6)  Auth

#### The usualy api security protection, and HttpApi solution:

- Use Https to transport api data, to avoiding be listenned and modified.
- Full open api: This kinds of api is only use in inner trusted system environment.
- Fix Token protected api: This kind of token is fix string, such as appid.
- Dynamic token protected api: Token is create by appid + appsecret + timestamp.
- Need Login api: such as get my order.
- Other limit: IP, Frequence, Action.


#### HttpAPI make some AuthXXX properties to support api security.

```
[HttpApi("...", AuthVerbs="Get,Post")]      // check visit verb
[HttpApi("...", AuthLogin=true)]            // check user login status
[HttpApi("...", AuthUsers="A,B")]           // check user name
[HttpApi("...", AuthRoles="A,B")]           // check user role
[HttpApi("...", AuthIP=true)]               // check visit IP
[HttpApi("...", AuthToken=true)]            // check Token
```

#### Check login status, user name, user role
```
[HttpApi("Login")]
public string Login()
{
    AuthHelper.Login("Admin", new string[] { "Admins" }, DateTime.Now.AddDays(1));
    System.Threading.Thread.Sleep(200);
    return "Login success ";
}

[HttpApi("Sign out")]
public string Logout()
{
    AuthHelper.Logout();
    System.Threading.Thread.Sleep(200);
    return "Sign ok";
}

[HttpApi("User must login", AuthLogin=true)]
public string LimitLogin()
{
    System.Threading.Thread.Sleep(200);
    return "OK(Logined) ";
}

[HttpApi("User must be admin or kevin", AuthUsers = "Admin,Kevin")]
public string LimitUser()
{
    System.Threading.Thread.Sleep(200);
    return "OK(Limit Admin,Kevin) ";
}

[HttpApi("Use must has 'admins' role", AuthRoles = "Admins")]
public string LimitRole()
{
    System.Threading.Thread.Sleep(200);
    return "OK(Limit Admins) ";
}
```



#### AuthToken and AuthIP


You can check token and ip in custom way, eg.

```
public class Global : System.Web.HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        // HttpApi custom auth
        HttpApiConfig.Instance.OnAuth += (ctx, method, attr, token) =>
        {
            if (attr.AuthIP && !CheckIP(ip))
                throw new HttpApiException("This ip is forbidden", 401);
            if (attr.AuthToken && !CheckToken(token))
                throw new HttpApiException("Please check token", 401);
            if (attr.Log)
                Logger.Log(...);
            // Other auth logic, such as visit frequence.
            // Throw HttpApiException if auth fail.
        };
    }
}
```


### (7)  Uniform data frmat: APIResult

HttpApi support union api rsult format to simply client calling.

```
[HttpApi("Ouput system datetime")]
public APIResult GetTime()
{
    return new APIResult(true, "OK", System.DateTime.Now);
}
```

Then the output maybe
```
{
    Result: true,
    Info: "OK",
    CreateDt: "2019-07-16 10:24:14",
    Data: '2019-01-01',
    Extra: {...}
}
```


### (8)  Other parameter
```
public string Description { get; set; }
public string Example { get; set; }
public string Remark { get; set; }
public string MimeType { get; set; }
public string FileName { get; set; }
public bool Wrap { get; set; } = false;
public ApiStatus Status { get; set; }
```


## 6. More examples
```

[HttpApi("Json Wrapper", Wrap = true)]
public static object TestWrap()
{
    return new { h = "3", a = "1", b = "2", c = "3" };
}

[HttpApi("Default paramter", Status = ApiStatus.Delete, AuthVerbs ="GET")]
public static object TestDefaultParameter(string p1, string p2="a")
{
    return new { p1 = p1, p2 = p2};
}

[HttpApi("Exception Test")]
public static object TestError()
{
    int n = 0;
    int m = 1 / n;
    return true;
}

[HttpApi("Auth verb", AuthVerbs ="Post")]
public static string TestVerbs()
{
    return HttpContext.Current.Request.HttpMethod;
}

[HttpApi("Return enum")]
public static Sex TestEnum()
{
    return Sex.Male;
}

//---------------------------------------------
// Other basic data type
//---------------------------------------------
[HttpApi("plist file", CacheSeconds = 30, MimeType="text/plist", FileName="app.plist")]
public string GetFile(string info)
{
    System.Threading.Thread.Sleep(200);
    return string.Format("This is plist file demo! {0} {1}", info, DateTime.Now);
}

[HttpApi("date time", CacheSeconds=30)]
public DateTime GetTime()
{
    return System.DateTime.Now;
}

[HttpApi("DataTable")]
public DataTable GetDataTable()
{
    DataTable dt = new DataTable("test");
    dt.Columns.Add("column1");
    dt.Columns.Add("column2");
    dt.Rows.Add("a1", "b1");
    dt.Rows.Add("a2", "b2");
    return dt;
}

[HttpApi("DataRow")]
public DataRow GetDataRow()
{
    DataTable dt = new DataTable("test");
    dt.Columns.Add("column1");
    dt.Columns.Add("column2");
    dt.Rows.Add("a1", "b1");
    dt.Rows.Add("a2", "b2");
    return dt.Rows[0];
}

[HttpApi("Dictionary")]
public IDictionary GetDictionary()
{
    var dict = new Dictionary<int, Person>();
    dict.Add(0, new Person() { Name = "Marry" });
    dict.Add(1, new Person() { Name = "Cherry" });
    return dict;
}

[HttpApi("Image", CacheSeconds=60)]
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
// Class
//---------------------------------------------
[HttpApi("father:{Name:'Kevin', Birth:'1979-12-01', Sex:0};")]
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

[HttpApi("null")]
public static Person CreateNull()
{
    return null;
}

[HttpApi("Output class object")]
public static Person GetPerson()
{
    return new Person() { Name = "Cherry" };
}


[HttpApi("Output Xml", Type=ResponseType.XML)]
public static Person GetPersonXml()
{
    return new Person() { Name = "Cherry" };
}

[HttpApi("Output class, and wrap with APIResult", Wrap =true)]
public static Person GetPersonDataResult()
{
    return new Person() { Name = "Kevin" };
}

[HttpApi("Output APIResult")]
public static APIResult GetPersons()
{
    var persons = new List<Person>(){
        new Person(){ Name="Kevin", Sex=Sex.Male, Birth=new DateTime(2000, 01, 01)},
        new Person(){ Name="Cherry", Sex=Sex.Female, Birth=new DateTime(2010, 01, 01)}
    };
    return new APIResult(true, "", persons);
}
```      

## 7. Project motivation
- Basic motivation: (1) Simply api coding amount for http server; (2) Auth create client javascript
- And more complex function, such as auth, security, caching, format, exception, uniform api result, etc.
- WebAPI has many limits: http://blog.csdn.net/leeyue_1982/article/details/51305950
- WebAPI don't support my target, so I create this project, and maintain so many years.



## 8.Snapshots

Api define<br/>
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/apicode.png?raw=true)

Api list page<br/>
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/apilist.png?raw=true)

Api test page<br/>
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/api.png?raw=true)

Api output(defautl is json) <br/>
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/apiresult.png?raw=true)

Api output xml <br/>
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/apixml.png?raw=true)

Token demo<br/>
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/token.png?raw=true)

Auth demo<br/>
![](https://github.com/surfsky/App.HttpApi/blob/master/Snap/auth.png?raw=true)


## 9.Reference
- http://www.cnblogs.com/wzcheng/archive/2010/05/20/1739810.html




## 10.More targets
- XML format control: property/field, indent, case...
- AuthInterval
- File upload
- Long time connect api




## 11.History
2012-08  
- Init

2014-06
- Support defaul parameter; 
- Auth login, user, role; 
- Exception output format(APIResult or HTTP ERROR) 

2016-06  
- Add api display page
- Fix Image output error

2017-11  
- Simply HttpApiAttribute
- Caching

2017-12  
- Nuget: install-package App.HttpApi
- Add  HttpApiConfig configuration section

2018-10  
- Support custom auth event; 
- Add Api display page; 
- Config json output format;
- Simply visit path
- Fix XML output; 

2018-11  
- Default parameter can be null or leased.
- Nullable parameter can be null or leased.
- Add enum parameter description; 

2019-03  
- Add Api test page

2019-06  
- Client can refresh cache by parameter(_refresh=true) 

2019-07  
- Long number can be outputed to string.

2019-08
- Apply  Bootstrap style
- Simply Page.aspx/Method or Handler.ashx/Method api call, Need't inherit any class(absolete HttpApiPageBase and HttpApiHandlerBase)
- Global culture support. Add configuration parameter: language="zh-CN"
- Add dynamic token example page.
- Update  Json.Net to version 11.0.2
- simply and fix App.HttpApi.Test project.
- Fix nullable parameter error bug
- Fix javascript parameter leasing problem when AuthToken=true.

2.4.0
- Remove App.Core reliation.

2.5
- Remove App.Core reliation, but keepping Enum GetUIDescription capacility by reflection.
- Modifey Json MIMETYPE from "application/json" to "text/json";
- Fix Web.Config
    ```
    <!-- Some server will lost session, so add this two lines -->
    <remove name="Session" />
    <add name="Session" type="System.Web.SessionState.SessionStateModule"/>
    ```

2.5.3
* ParseCookie don't throw exception

2.5.4
+ HttpApiAttribute.Deprecated -> Obsolete
+ HttpApiAttribute.Delete
* fix bug:  "Object of type 'System.Int32' cannot be converted to type 'System.Nullable`1[App.Sex]'. See example: GetNullalbeEnum2