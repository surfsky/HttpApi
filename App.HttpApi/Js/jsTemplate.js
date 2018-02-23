

/*
* 功能说明: 使用js ajax调用服务器端方法
* 服务地址: %URL%/js
* 创建时间: %DATE%
* 缓存时间: %DURATION% 秒
* 内核维护: http://surfsky.cnblogs.com
* 调用方法：
*     同步调用
*         var val = %NS%.%CLS%.Foo(parameters);
*     异步调用
*         %NS%.%CLS%.Foo(parameters, function(data){document.getElementById('id').innerHTML = data;});
*         %NS%.%CLS%.Foo(parameters, function(){}, 'id');
*/
%NS-BUILD%
%NS%.BeforeCallWebMethod = function (id)       {
    if (id != null) 
        document.getElementById(id).innerHTML = "...";
};
%NS%.AfterCallWebMethod  = function (id, data) {
    if (id != null) 
        document.getElementById(id).innerHTML = data;
};
%NS%.%CLS% = {
    _url: "%URL%",

    // 调用服务器端方法
    CallWebMethod: function (methodName, args, options, callback, senderId) {
        // 调用前处理
        %NS%.BeforeCallWebMethod(senderId);
        var url = this._url + "/" + methodName;
        var data = JSON.stringify(args);
        var ajax;
        if (window.ActiveXObject) {
            ajax = new ActiveXObject('Microsoft.XMLHTTP');
        } else if (window.XMLHttpRequest) {
            ajax = new XMLHttpRequest();
        }

        // 若回调函数不为空，则启用异步方式
        if (callback != null) {
            var fn = function (data) {
                callback(data);
                %NS%.AfterCallWebMethod(senderId, data);
            };
            ajax.onreadystatechange = function(){
                if (ajax.readyState == 4){
                    var results = ajax.response;
                    fn(results);
                }
            };
            ajax.open('POST', url, true);
            //ajax.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            ajax.send(data);
        }
        // 否则启用同步方式
        else {
            var val;
            var fn = function (data) {
                val = data;
                %NS%.AfterCallWebMethod(senderId, data);
            };
            ajax.onreadystatechange = function(){
                if (ajax.readyState == 4){
                    var results = ajax.response;
                    fn(results);
                }
            };
            ajax.open('POST', url, false);
            //ajax.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            ajax.send(data);
            return val;
        }
    }
};

