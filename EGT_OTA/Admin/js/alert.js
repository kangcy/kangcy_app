var MyAlert;

//弹出框
function MessageBox(url, title, w, h) {
    $.dialog({
        lock: true,
        content: "url:" + url,
        width: w,
        height: h,
        title: title,
        max: false,
        min: false
    });
}

function MessageBoxByContent(Content, title, w, h) {
    $.dialog({
        lock: true,
        content: Content,
        width: w,
        height: h,
        title: title,
        max: false,
        min: false
    });
}

function MessageBoxID(id, url, title, w, h) {
    id = $.dialog({
        lock: true,
        content: "url:" + url,
        width: w,
        height: h,
        title: title,
        max: false,
        min: false
    });
}

function SeceltAll(obj) {
    var chk = $("input[type=checkbox]");
    switch (obj) {
        case 1: //全选
            chk.attr("checked", true);
            break;
        case 2: //反选
            chk.attr("checked", false);
            break;
        case 3: //反选
            chk.each(function () {
                $(this).attr("checked", !$(this).attr("checked"));
            });
            break;
    }
}

//获取URL参数
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return "";
}

//弹框点击确认执行实现
function Confirm(title, func) {
    $.dialog({
        lock: true,
        id: "dialog_" + Math.random(),
        title: "系统提示",
        content: title,
        icon: "alert.gif",
        button: [{
            name: "确定",
            callback: function () {
                try {
                    if ($.isFunction(func))
                        func();
                }
                catch (e) { }
                return true;
            }, focus: true
        },
        {
            name: "取消",
            callback: function () {
                //关闭当前Dialog 
                $(this).dialog("close");
            }
        }]
    });
}

//弹框点击确认执行实现
function Alert(title, func1,func2) {
    $.dialog({
        lock: true,
        id: "dialog_" + Math.random(),
        title: "系统提示",
        content: title,
        icon: "alert.gif",
        button: [{
            name: "确定",
            callback: function () {
                try {
                    if ($.isFunction(func))
                        func();
                }
                catch (e) { }
                return true;
            }, focus: true
        },
        {
            name: "取消",
            callback: function () {
                func2();
                $(this).dialog("close");
            }
        }]
    });
}

//弹框点击确认执行实现
function Click(title, func) {
    $.dialog({
        lock: true,
        id: "dialog_" + Math.random(),
        title: "系统提示",
        content: title,
        icon: "alert.gif",
        button: [{
            name: "确定",
            callback: function () {
                try {
                    if ($.isFunction(func))
                        func();
                }
                catch (e) { }
                return true;
            }, focus: true
        }]
    });
}

//只弹框
function Alert(title) {
    try {
        if (MyAlert != null) {
            MyAlert.close();
        }
    } catch (e) {

    }
    MyAlert = $.dialog({
        lock: true,
        id: "dialog_" + Math.random(),
        title: "系统提示",
        content: title,
        icon: "alert.gif",
        button: [{
            name: "确定",
            callback: function () {
                try {
                    callbackfunction;
                }
                catch (e) { }
                return true;
            },
            focus: true
        }]
    });
}
