

# History

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


2.6
* ParamAttribute -> HttpParamAttribute  to avoid confliction with App.Core.ParamAttribute

2.6.1
+ Add HttpApiConfig.Instance.JsonSetting

2.6.2
+ Support TAttribute to get enum description

2.7
+ Support Postfile (see Demo.Up)

2.8
+ Support AuthTraffic to defence attack (see Demo.Login)

2.8.3
+ Support DescriptionAttribute on method export

2.8.4
+ Parameter mismatch check

2.8.5
+ Friendly exception