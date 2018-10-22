HttpApi
==================================


## 说明：
    （1）一种轻量的提供数据的框架，可作为 WebAPI 的替代方案
    （2）将类中的方法暴露为http接口，如：
        HttpApi/Name/GetData  data:{page:1,rows:2,sort:'abc',order:'desc'}
        HttpApi/Name/api
        HttpApi/Name/apis
    （3）将页面类中的方法暴露为http接口，如：
        Handler1.aspx/GetData?page=1&rows=2&sort=abc&order=desc
        Handler1.ashx/GetData?page=1&rows=2&sort=abc&order=desc
    （4）自动生成客户端调用脚本(支持三种脚本：jquery, ext, js)
        HttpApi/TypeName/js
        HttpApi/TypeName/jq
        HttpApi/TypeName/ext
    （5）带缓存机制：可指定方法返回值的缓存时间、方式
    （6）带鉴权机制：访问IP、动作、 是否登录、用户名、角色、安全码。可自定义接口鉴权逻辑。
    （7）可配置Json输出格式：枚举输出、json递进、日期、错误时的输出方式
    （8）带封装机制：可将方法返回值自动包裹为 DataResult 结构体
    （9）服务器端和客户端都可指定接口返回的数据格式，如text, xml，json, file, image, base64image 等

## 作者
　　程建和
　　http://surfsky.cnblogs.com
      http://github.com/surfsky

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
    - 2018-10-22  简化路径为：/HttpApi/Name/Method

## 参考：WebAPI的一些限制
    - http://blog.csdn.net/leeyue_1982/article/details/51305950

## 任务
    - Api 测试页面（填写参数；选择方法Get/Post；发送请求；显示输出结果）
