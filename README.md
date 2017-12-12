HttpApi
==================================


## 说明：
    （1）一种轻量的提供数据的框架，可替代WebAPI
    （2）可以以类似webservice url的方式来提供数据，如：
        GET: Handler1.ashx\GetData?page=1&rows=2&sort=abc&order=desc
        POST:Handler1.ashx\GetData  data:{page:1,rows:2,sort:'abc',order:'desc'}
    （3）客户端可指定服务器端返回的数据格式，如text, xml，json, file 等（非string、image类型，默认类型输出为 json）
    （4）自动生成客户端调用脚本(支持三种脚本：jquery, ext, js)
    （5）带缓存机制，可定义函数数据的缓存时间
    （6）带授权访问机制，可限制某些接口必须登录后才能访问

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

## WebAPI的限制
    - http://blog.csdn.net/leeyue_1982/article/details/51305950

