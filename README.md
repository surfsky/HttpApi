HttpApi
==================================


## 说明：
    （1）一种轻量的、提供数据接口的框架，可作为 WebAPI 的替代方案
    （2）将类中的方法暴露为http接口，如：
        HttpApi/TypeName/Method?p1=x&p2=x
        HttpApi/TypeName/api
        HttpApi/TypeName/apis
    （3）将页面类中的方法暴露为http接口，如：
        Handler1.aspx/GetData?page=1&rows=2&sort=abc&order=desc
        Handler1.ashx/GetData?page=1&rows=2&sort=abc&order=desc
    （4）自动生成客户端调用脚本
        HttpApi/TypeName/js
    （5）api 展示及接口参数展示页面
        HttpApi/TypeName/api
        HttpApi/TypeName/Method_
    （6）带缓存机制：可指定方法返回值的缓存时间、方式
    （7）带鉴权机制：访问IP、动作、 是否登录、用户名、角色、安全码。可自定义接口鉴权逻辑。
    （8）带封装机制：可将方法返回值自动包裹为 DataResult 结构体
    （9）可配置Json输出格式：枚举输出、json递进、日期、错误时的输出方式
    （10）服务器端和客户端都可指定接口返回的数据格式，如text, xml，json, file, image, base64image 等

## 作者
　　程建和
      http://github.com/surfsky

## 安装
    - Nuget: install-package App.HttpApi


## 使用
(1) 引用类库（用nuget安装的话会自动完成）
       App.Core.dll
       App.HttpApi.dll
    - 修改 web.config 文件（用nuget安装的话会自动修改）
       ```xml
       <system.webServer>
         <modules>
           <add name="HttpApiModule" type="App.HttpApi.HttpApiModule" />
         </modules>
       </system.webServer>
       ```
(2) 在需要导出HttpApi的方法上写上标注
       ```c#
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
      '''
(3) 客户端调用
        http://...../HttpApi/Demo/HelloWorld?info=x
(4) 用法详见示例项目

##  高级操作
(1) 控制 HttpApi 输出
       ```xml
   <httpApi 
      formatEnum="Text" 
      formatIndented="Indented" 
      formatDateTime="yyyy-MM-dd" 
      formatLowCamel="false"
      errorResponse="DataResult" 
      apiTypePrefix="App." 
      wrap="" 
      />
  <system.webServer>
    <modules>
      <add name="HttpApiModule" type="App.HttpApi.HttpApiModule" />
    </modules>
  </system.webServer>
    ```
(2) 自动生成客户端调用的 javascript 脚本
    <script src="http://.../App/Demo/js"></script>
    可在类上附上标签，控制生成的脚本内容
    [Script(CacheDuration =0, ClassName ="Demo", NameSpace ="App")]

(3) 自动生成 Api 介绍页面
    http://..../HttpApi/Demo/api
    http://..../HttpApi/Demo/HelloWorld_
    可附上标签，附加显示 Api 修改历史，参数信息等
    ```c#
          [History("2016-11-01", "SURFSKY", "修改了A")]
          public class Demo
          {
              [HttpApi("HelloWorld")]
              [Param("info", "信息")]
              public static string HelloWorld(string info)
              {
                   System.Threading.Thread.Sleep(200);
                   return string.Format("Hello world! {0} {1}", info, DateTime.Now);
              }
          }
    ```
      

## History
    - 2012-08  初版
    - 2014-06  支持默认参数；增加问授权（角色、用户、登录）；错误输出可控（DataResult 或 HTTP ERROR）
    - 2016-06  增加api展示窗口，修正Image方式输出故障
    - 2017-11  简化和优化 HttpApiAttribute，可选缓存方式
    - 2017-12  Nuget发布：install-package App.HttpApi，增加 HttpApiConfig 配置节
    - 2018-10  增加自定义鉴权事件；实现Api展示页面；用配置节控制Json输出格式；简化访问路径；完善xml输出
    - 2018-11  默认参数可为空也可不填写；可空类型参数可为空也可不填写；可在api介绍页面上输出枚举类型成员信息；

## 项目目标
    - WebAPI的一些限制：http://blog.csdn.net/leeyue_1982/article/details/51305950
    - 这个项目的初衷是简化接口开发，并自动完成客户端js代码的封装，简化服务器端和客户端的开发代码量，减少出错率
    - 并想集成鉴权、缓存、输出格式控制等逻辑
    - Restful 方式的API动作过少（GET/POST/DELETE/)，无法覆盖到所有动作，干脆放开方法名，让开发者自己定义好了

## 任务
    - Api 测试页面（填写参数；选择方法Get/Post；发送请求；显示输出结果）
    - XML 格式控制：属性/成员、递进、大小写等


## 参考
    - 参考 http://www.cnblogs.com/wzcheng/archive/2010/05/20/1739810.html
