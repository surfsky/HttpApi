

/*
* Http Api Invoke
* %URL%/js
* Cache Seconds: %DURATION%
* Create Date: %DATE%
* Core Maintainer: https://github.com/surfsky/AppPlat.HttpApi/
* Usage：
*     synchronous
*         var val = %NS%.%CLS%.Foo(parameters);
*     asynchronous
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

    // call web method
    CallWebMethod: function (methodName, args, options, callback, senderId) {
        // before call
        %NS%.BeforeCallWebMethod(senderId);
        var url = this._url + "/" + methodName;
        var data = JSON.stringify(args);
        var ajax;
        if (window.ActiveXObject) {
            ajax = new ActiveXObject('Microsoft.XMLHTTP');
        } else if (window.XMLHttpRequest) {
            ajax = new XMLHttpRequest();
        }

        // Asynchronous if callback is not null
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
        // Synchronous if callback is null
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

