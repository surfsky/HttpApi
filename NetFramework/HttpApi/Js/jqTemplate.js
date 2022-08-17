

/*
* 功能说明: 使用jquery ajax调用服务器端方法
* 服务地址: %URL%/jq
* 创建时间: %DATE%
* 缓存时间: %DURATION% 秒
* 依赖脚本：JQuery
* 内核维护: surfsky.cnblogs.com
* 调用方法：
*     同步调用
*         var val = %NS%.%CLS%.Foo(parameters);
*     异步调用
*         %NS%.%CLS%.Foo(parameters, function(data){$('#id').html(data);});
*         %NS%.%CLS%.Foo(parameters, function(){}, 'id');
*/
%NS-BUILD%
%NS%.BeforeCallWebMethod = function (id)       {
    if (id != null) 
        $("#" + id).html("...");
};
%NS%.AfterCallWebMethod  = function (id, data) {
    if (id != null) 
        $("#" + id).html(data);
};
%NS%.%CLS% = {
    _url: "%URL%",
    defaultOptions: {
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST"
    },

    // 调用服务器端方法
    CallWebMethod: function (methodName, args, options, callback, senderId) {
        // 调用前处理
        %NS%.BeforeCallWebMethod(senderId);
        var url = this._url + "/" + methodName;
        var data = JSON.stringify(args);

        // 若回调函数不为空，则启用异步方式
        if (callback != null) {
            var fn = function (data) {
                callback(data);
                %NS%.AfterCallWebMethod(senderId, data);
            };
            $.extend(options, this.defaultOptions, { url: url, data: data, async: true});
            $.ajax(options).always(function(ret) {fn(ret.responseText);});
        }
        // 否则启用同步方式
        else {
            var val;
            var fn = function (data) {
                val = data;
                %NS%.AfterCallWebMethod(senderId, data);
            };
            $.extend(options, this.defaultOptions, { url: url, data: data, async: false});
            $.ajax(options).always(function(ret) {fn(ret.responseText);});
            return val;
        }
    }
};

