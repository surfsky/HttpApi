HttpApi
==================================


## 说明：
    （1）一种轻量的提供数据的框架，可作为 WebAPI 的替代方案
    （2）可以将类中的方法暴露为http接口，如：
        Handler1.ashx\GetData?page=1&rows=2&sort=abc&order=desc
        HttpApi.App.TypeName.axd\GetData  data:{page:1,rows:2,sort:'abc',order:'desc'}
        HttpApi.App.TypeName.axd\api
    （3）服务器端和客户端都可指定接口返回的数据格式，如text, xml，json, file, base64image 等
    （4）自动生成客户端调用脚本(支持三种脚本：jquery, ext, js)
    （5）带缓存机制：可定义函数数据的缓存时间、方式
    （6）带授权机制：访问IP、动词、 是否登录、用户名、角色、安全码。可自定义接口鉴权逻辑。
    （7）带封装机制：可将返回值自动包裹为 DataResult 结构体
    （8）带输出控制：可设置枚举值的输出格式

## 作者
　　程建和
　　surfsky@sina.com
　　surfsky.cnblogs.com

## 使用:
    - Nuget: install-package App.HttpApi
    - 详看示例项目 Test
    - 参考 http://www.cnblogs.com/wzcheng/archive/2010/05/20/1739810.html


## History
    - 2012-08-01  初版
    - 2014-06-01  支持默认参数；增加问授权（角色、用户、登录）；错误输出可控（DataResult 或 HTTP ERROR）
    - 2016-06-06  增加api展示窗口，修正Image方式输出故障
    - 2017-11-23  简化和优化 HttpApiAttribute，可选缓存方式
    - 2017-12-11  Nuget发布：install-package App.HttpApi
    - 2017-12-12  增加 HttpApiConfig 配置节
    - 2018-10-19  增加自定义鉴权事件；实现Api展示页面；用配置节控制Json输出格式；

## 参考：WebAPI的限制
    - http://blog.csdn.net/leeyue_1982/article/details/51305950


## 任务
    - 用HttpModule实现
    - 用更友好的地址方式来访问，如/HttpApi/A.B.C/Method
