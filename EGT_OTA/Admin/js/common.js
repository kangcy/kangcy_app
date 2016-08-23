/***
* javascript通用方法的封装
* 依赖文件：jquery.js
* 2011-09-02
*/

(function () {

    //页面加载时执行的方法
    $(function () {
        //屏蔽只读文本框的后退键
        $("input[type='text']").keydown(function (event) {
            if ($(this).attr("readonly") && event.keyCode == 8) {
                return false;
            }
        })
    }).ajaxError(function (event, XMLHttpRequest, ajaxOptions, thrownError) {//ajax请求出现异常时的处理方法
        if ($c.debug) {
            $t.debugMsg(XMLHttpRequest.responseText || XMLHttpRequest.responseXML)
        }
        else {
            //Dialog.alert({ content: "请求出现异常，未进行任何处理。" });
        }
    });


    /***********************
    * 全局配置类(Config)
    * 2011-09-02
    ***********************/
    $c = {
        //站点根目录
        basePath: "/",
        //是否为调试状态
        debug: true,
        //域名
        domain: "",
        //编辑器工具栏配置
        UDArticleTool: [['FullScreen', 'Source', 'Preview', '|', 'Bold', 'Italic', 'Underline', 'ForeColor', 'BackColor', 'FontFamily', 'FontSize', 'Indent', 'LineHeight', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyJustify', 'RemoveFormat', 'AutoTypeSet', '|', 'Link', 'Unlink', 'InsertTable', 'InsertImage', 'Emotion', 'InsertVideo', 'map']],
        UDEasyTool: [['Bold', 'Italic', 'Underline', 'ForeColor', 'BackColor', 'FontFamily', 'FontSize', 'Indent', 'LineHeight', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyJustify', 'RemoveFormat', 'AutoTypeSet', '|', 'Link', 'Unlink', 'InsertImage', 'Emotion', 'InsertVideo']],
        UDUserTool: [['Preview', 'Undo', 'Redo', 'InsertImage', 'Emotion']],
        dfUEditImgUrl: "http://f4.dj.com/ueditor.axd",
        forumImageUrl: "http://f5.dj.com/ueditor.axd?Module=UEditorForumImg",
        articleImageUrl: "http://f1.dj.com/ueditor.axd?Module=UEditorArticlImg",
        guidEmpty: "00000000-0000-0000-0000-000000000000",
        module: { Article: 0, House: 1, Enterprise: 2, JobInfo: 3, Resume: 4, UsedGoods: 5, Forum: 6, ShopProduct: 7, GroupPurchase: 8, Other: 100 }, //请不要随意修改
        //编辑器工具栏配置
        UDDefaultTool: [['FullScreen', 'Source', 'Preview', '|', 'Bold', 'Italic', 'Underline', 'ForeColor', 'BackColor', 'FontFamily', 'FontSize', 'Indent', 'LineHeight', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyJustify', 'RemoveFormat', 'AutoTypeSet', '|', 'Link', 'Unlink', 'InsertTable', 'InsertImage', 'Emotion', 'InsertVideo', 'music', 'map']],
        getModuleName: function (moduleID) {
            for (var name in $c.module) {
                if ($c.module[name] == moduleID) {
                    return name;
                }
            }
        },
        searchUrl: { Article: "/article/search.aspx", House: "/house/search.aspx", Enterprise: "/enterprise/search.aspx", JobInfo: "/job/search.aspx", Resume: "/resume/search.aspx", UsedGoods: "/usedgoods/search.aspx", Forum: "/forum/search.aspx", ShopProduct: "/shop/search.aspx", GroupPurchase: "/group/search.aspx" },
        getSearchUrl: function (moduleID) {
            for (var name in $c.module) {
                if ($c.module[name] == moduleID) {
                    return $c.searchUrl[name];
                }
            }
        },
        //金额小数保留的位数
        decimalDigits: 2,
        version: "1.0.0",
        months: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
    };

    window["$c"] = $c;

    //若本页面未定义Dialog对象，尝试从父级页面中获取Dialog弹出对话框管理对象
    if (typeof Dialog == "undefined") {
        var topWindow = window.parent;

        while (!topWindow.Dialog && topWindow.parent != topWindow) {
            try {
                if (topWindow.parent.document.domain != document.domain) {
                    break;
                }
                if (topWindow.parent.document.getElementsByTagName('frameset').length > 0) {
                    break;
                }
            } catch (e) {
                break;
            }
            topWindow = topWindow.parent;
        }
        window["Dialog"] = topWindow.Dialog;
    };

    //若本页面未定义$tag对象，试图从父级页面中获取$tag页面标签管理对象
    if (typeof $tag == "undefined") {
        var topWindow = window.parent;

        while (!topWindow.$tag && topWindow.parent != topWindow) {
            try {
                if (topWindow.parent.document.domain != document.domain) {
                    break;
                }
                if (topWindow.parent.document.getElementsByTagName('frameset').length > 0) {
                    break;
                }
            } catch (e) {
                break;
            }
            topWindow = topWindow.parent;
        }
        window["$tag"] = topWindow.$tag;
    };

    /***********************
    * 工具类：一些实用的函数(tools)
    * 2011-09-02
    ***********************/
    var $t = {
        //按照比例对图片进行缩放
        imageResize: function (img, width, height) {
            var oImg;
            if (typeof img == "string") {
                oImg = document.getElementById(img);
            }
            else {
                oImg = img;
            }
            if (!oImg) {
                return;
            }
            var image = new Image();
            var iwidth = width;           //定义允许图片宽度   
            var iheight = height;         //定义允许图片高度
            image.src = oImg.src;
            if (image.width > 0 && image.height > 0) {
                if (image.width / image.height >= iwidth / iheight) {
                    if (image.width > iwidth) {
                        oImg.width = iwidth;
                        oImg.height = (image.height * iwidth) / image.width;
                    }
                    else {
                        oImg.width = image.width;
                        oImg.height = image.height;
                    }
                }
                else {
                    if (image.height > iheight) {
                        oImg.height = iheight;
                        oImg.width = (image.width * iheight) / image.height;
                    }
                    else {
                        oImg.width = image.width;
                        oImg.height = image.height;
                    }
                }
            }
        },
        /**
        * 功能：为window.onload事件注册方法
        * 参数： func window.onload事件注册的方法
        */
        addDocLoad: function (func) {
            var oldOnload = window.onload;
            if (typeof oldOnload != 'function') {
                window.onload = func;
            }
            else {
                window.onload = function () {
                    oldOnload();
                    func();
                }
            }
        },
        /**
        * 功能：比较两个日期的大小(若开始时间小于或等于结束时间，返回true)
        * 返回值：如果startTime>=endTime返回false，否则返回true
        * 参数： startTime 开始时间(默认格式：2011-06-20 12:34:56)
        *            endTime  结束时间(默认格式：2011-06-20 12:34:56)
        *            format 时间格式
        */
        compareDateTime: function (startTime, endTime, format) {
            if (format == 0) {//格式如2011-06-20
                startTime += " " + "00:00:00";
                endTime += " " + "23:59:59";
            }
            if (format == 1) {//格式如2011-06-20 12:34
                startTime += " " + "00";
                endTime += " " + "00";
            }

            var date1 = startTime.split(" ")[0];
            var time1 = startTime.split(" ")[1];
            var arrDate1 = date1.split("-");
            var arrTime1 = time1.split(":");
            var sDateTime = new Date(arrDate1[0], arrDate1[1], arrDate1[2], arrTime1[0], arrTime1[1], arrTime1[2]);

            var date2 = endTime.split(" ")[0];
            var time2 = endTime.split(" ")[1];
            var arrDate2 = date2.split("-");
            var arrTime2 = time2.split(":");
            var eDateTime = new Date(arrDate2[0], arrDate2[1], arrDate2[2], arrTime2[0], arrTime2[1], arrTime2[2]);

            return sDateTime.getTime() <= eDateTime.getTime();
        },
        /**
        * 功能：将GUID的列表格式化为带引号(')的格式，如：GUID1,GUID2格式化后为'GUID1',GUID2'
        * 返回值：格式化后带引号的GUID列表
        * 参数： IDList 未带引号的格式
        *            separator  GUID列表的分隔符
        */
        formatGuidList: function (IDList, separator) {
            separator = separator || ",";
            var arrIDList = IDList.split(separator);
            for (var i = 0, len = arrIDList.length; i < len; i++) {
                arrIDList[i] = "'" + arrIDList + "'";
            }
            return arrIDList.join(",");
        },
        //格式化url后面的参数
        formatUrlParas: function (url, paras) {
            if (url.indexOf("?") == -1) {
                url += "?";
            }
            else {
                url += "&";
            }
            var para = "";
            for (var i = 0, len = paras.length; i < len; i++) {
                if (paras[i].key == "rnd") {
                    paras[i].value = new Date().valueOf();
                }
                para += "&" + paras[i].key + "=" + paras[i].value;
            }
            if (para.length > 1) {
                para = para.substring(1);
            }
            return url + para;
        },
        //获取域名
        getDomain: function () {
            var hostname = window.location.hostname.toString();
            var hosts = hostname.split(".");
            var len = hosts.length;
            if (/(\d+\.)+/.test(hostname)) {//IP地址
                return hostname
            }
            if (len > 2) {
                hosts.splice(0, 1);
                return hosts.join(".");
            }
            return hostname;
        },
        //获取滚动条的位置
        getScrollPosition: function (relativeWindow) {
            relativeWindow = relativeWindow || window;
            if ($.browser.msie) {
                var oDoc = relativeWindow.document;
                // Try with the doc element.
                var oPos = { x: oDoc.documentElement.scrollLeft, y: oDoc.documentElement.scrollTop };
                if (oPos.x >= 0 || oPos.y >= 0)
                    return oPos;
                // If no scroll, try with the body.
                return { x: oDoc.body.scrollLeft, y: oDoc.body.scrollTop };
            }
            else {
                return { x: relativeWindow.pageXOffset, y: relativeWindow.pageYOffset };
            }
        },
        //获取页面可见区域的高宽
        getViewportSize: function (win) {

            win = win || window;
            var doc = win.document;
            return {
                width: (win.innerWidth) ? win.innerWidth : (doc.documentElement && doc.documentElement.clientWidth) ? doc.documentElement.clientWidth : (doc.body ? doc.body.offsetWidth : 0),
                height: (win.innerHeight) ? win.innerHeight : (doc.documentElement && doc.documentElement.clientHeight) ? doc.documentElement.clientHeight : (doc.body ? doc.body.offsetHeight : 0)
            }
        },
        getPageSize: function (win) {
            var doc = (win || window).document;
            var relElement = this.isStrictMode ? doc.documentElement : doc.body;
            return {
                'width': Math.max(relElement.scrollWidth, relElement.clientWidth, doc.scrollWidth || 0) - 1,
                'height': Math.max(relElement.scrollHeight, relElement.clientHeight, doc.scrollHeight || 0) - 1
            };
        },
        //获取事件event对象
        getEvent: function () {//ie/ff
            if (window.event) {
                return window.event;
            }
            func = getEvent.caller;
            while (func != null) {
                var arg0 = func.arguments[0];
                if (arg0) {
                    if ((arg0.constructor == Event || arg0.constructor == MouseEvent) || (typeof (arg0) == "object" && arg0.preventDefault && arg0.stopPropagation)) {
                        return arg0;
                    }
                }
                func = func.caller;
            }
            return null;
        },
        //获取鼠标的位置
        getMousePos: function (ev) {
            if (!ev) {
                ev = this.getEvent();
            }
            if (ev.pageX || ev.pageY) {
                return {
                    x: ev.pageX,
                    y: ev.pageY
                };
            }
            if (document.documentElement && document.documentElement.scrollTop) {
                return {
                    x: ev.clientX + document.documentElement.scrollLeft - document.documentElement.clientLeft,
                    y: ev.clientY + document.documentElement.scrollTop - document.documentElement.clientTop
                };
            }
            else if (document.body) {
                return {
                    x: ev.clientX + document.body.scrollLeft - document.body.clientLeft,
                    y: ev.clientY + document.body.scrollTop - document.body.clientTop
                };
            }
        },
        //获取元素的位置
        getElementPos: function (el) {
            el = this.getElement(el);
            if (!el) {
                return { "x": 0, "y": 0 };
            }
            var x = 0, y = 0;
            do {
                x += el.offsetLeft;
                y += el.offsetTop;
            } while (el = el.offsetParent);
            return { "x": x, "y": y };
        },
        //根据ID获取元素对象，若id为元素对象则返回此对象
        getElement: function (id) {
            return "string" == typeof id ? document.getElementById(id) : id;
        },
        setOuterHtml: function (obj, html) {
            var Objrange = document.createRange();
            obj.innerHTML = html;
            Objrange.selectNodeContents(obj);
            var frag = Objrange.extractContents();
            obj.parentNode.insertBefore(frag, obj);
            obj.parentNode.removeChild(obj);
        },
        //获取元素的第一个子节点
        firstChild: function (parentObj) {
            return $(parentObj).children(":first").get(0);
        },
        //获取元素的最后一个子节点
        lastChild: function (parentObj, tagName) {
            return $(parentObj).children(":last").get(0);
        },
        isStrictMode: function (document) {
            return $.support.boxModel;
        },
        //获取浏览器类型及其版本
        browser: {
            version: (navigator.userAgent.toLowerCase().match(/.+(?:rv|it|ra|ie)[\/: ]([\d.]+)/) || [0, '0'])[1],
            safari: /webkit/i.test(navigator.userAgent.toLowerCase()) && !this.chrome,
            opera: /opera/i.test(navigator.userAgent.toLowerCase()),
            firefox: /firefox/i.test(navigator.userAgent.toLowerCase()),
            msie: /msie/i.test(navigator.userAgent.toLowerCase()) && !/opera/.test(navigator.userAgent.toLowerCase()),
            mozilla: /mozilla/i.test(navigator.userAgent.toLowerCase()) && !/(compatible|webkit)/.test(navigator.userAgent.toLowerCase()) && !this.chrome,
            chrome: /chrome/i.test(navigator.userAgent.toLowerCase()) && /webkit/i.test(navigator.userAgent.toLowerCase()) && /mozilla/i.test(navigator.userAgent.toLowerCase())
        },
        getTopWindow: function () {
            var topWindow = window;
            while (!topWindow.parent && topWindow.parent != topWindow) {
                try {
                    if (topWindow.parent.document.domain != document.domain) {
                        break;
                    }
                    if (topWindow.parent.document.getElementsByTagName('frameset').length > 0) {
                        break;
                    }
                } catch (e) {
                    break;
                }
                topWindow = topWindow.parent;
            }
            return topWindow;
        },
        /**
        * 功能：将内容中的特殊字符（如：+ &)进行编码
        * 参数： content 需要进行编码的内容
        */
        encodeContent: function (content) {
            return content.replace(/\&/g, "%26").replace(/\+/g, "%2B");
        },
        formatMoney: function (val) {
            return "￥" + val;
        },
        /**
        * 功能：将指定url的页面添加到浏览器收藏夹中
        * 参数： 
        *       url：当前页面的url
        *       title：收藏夹中显示的标题
        */
        addFavorite: function (url, title) {
            try {
                window.external.AddFavorite(url, title);
            }
            catch (e) {
                try {
                    window.sidebar.addPanel(title, url, "");
                }
                catch (e) {
                    alert("加入收藏失败，请使用Ctrl+D进行添加或者是按照您当前浏览器的操作方法将本页添加到收藏夹。");
                }
            }
        },
        /**
        * 功能：将页面设置为首页
        * 参数： 
        */
        setHomePage: function (url) {
            if (document.all) {
                document.body.style.behavior = 'url(#default#homepage)';
                document.body.setHomePage(url);
            }
            else if (window.sidebar) {
                if (window.netscape) {
                    try {
                        netscape.security.PrivilegeManager.enablePrivilege("UniversalXPConnect");
                    }
                    catch (e) {
                        alert("该操作被浏览器拒绝，如果想启用该功能，请在地址栏内输入 about:config,然后将项signed.applets.codebase_principal_support 值该为true");
                    }
                }
                var prefs = Components.classes['@mozilla.org/preferences-service;1'].getService(Components.interfaces.nsIPrefBranch);
                prefs.setCharPref('browser.startup.homepage', url);
            }
        },
        /**
        * 功能：生成页面随机的ID
        *
        **/
        buildRanID: function (pre, doc) {
            if (!doc) doc = document;
            pre = pre || "";
            var id = pre + Math.round(Math.random() * 1000);
            if ($(doc).find(id).length > 0) {
                id = this.buildRanID(pre, doc);
            }
            return id;
        },
        /**
        * 功能：添加用户收藏夹
        */
        addToUserFavorite: function (option) {
            if (!option) return;
            var isRelaod = false;
            this.isLogin({
                login: function () {
                    add();
                },
                unlogin: function () {
                    isRelaod = true;
                    $t.openQuickLogin({ onClose: add });
                }
            });

            function add() {
                var id = option.id;
                var typeID = option.typeID;
                var typeName = option.typeName;
                var title = option.title;
                var url = option.url;
                var remark = option.remark;
                var objectType = option.objectType;
                var objectID = option.objectID;
                var objectTitle = option.objectTitle;
                var objectValue = option.objectValue;
                var locationUrl = "/theme/default/enterprise/AddFavorite.aspx";
                if (id > 0) {
                    locationUrl = "/theme/default/enterprise/AddFavorite.aspx?id=" + id;
                }
                Dialog.open({
                    dataSource: "iframe",
                    url: locationUrl,
                    title: "添加收藏",
                    width: 500,
                    height: 260,
                    theme: "gray",
                    data: {
                        id: id,
                        typeID: typeID,
                        typeName: typeName,
                        title: title,
                        url: url,
                        remark: remark,
                        objectType: objectType,
                        objectID: objectID,
                        objectTitle: objectTitle,
                        objectValue: objectValue
                    },
                    okButton: {
                        callBack: function () {
                            Dialog.close();
                        }
                    }
                });
            }

            function addFavorite() {
                var param = {};
                param["TypeID"] = option.typeID;
                param["TypeName"] = option.typeName;
                param["Title"] = option.title; //标题
                param["Url"] = option.url; //路径
                param["Remark"] = option.remark; //备注
                param["ObjectType"] = option.objectType; //关联对象类型
                param["ObjectID"] = option.objectID;
                param["ObjectTitle"] = option.objectTitle; //关联对象标题
                param["ObjectValue"] = option.objectValue; //关联对象值
                $.ajax({
                    type: "POST",
                    url: "/system.axd?t=FavoriteManage",
                    cache: false,
                    dataType: "json",
                    data: $.param(param),
                    success: function (msg) {
                        if (msg.notLogin) {
                            return;
                        }
                        if (msg.isadd) {
                            Dialog.alert({ content: msg.isadd, onOkButtonClick: function () {
                                if (isRelaod) document.location = document.location;
                            }
                            });
                        }
                        if (msg.success) {
                            Dialog.alert({ content: "添加收藏成功！", onOkButtonClick: function () {
                                if (isRelaod) document.location = document.location;
                            }
                            });
                        }
                    }
                });
            }
        },
        /**
        * 功能：添加用户好友
        */
        addUserFriend: function (option) {
            if (!option) return;
            var isRelaod = false;
            this.isLogin({
                login: function () {
                    addFriend();
                },
                unlogin: function () {
                    isRelaod = true;
                    $t.openQuickLogin({ onClose: addFriend });
                }
            });

            function addFriend() {
                var param = {};
                //param["UserID"] = option.userID; //用户ID,不需要了，在请求处理时直接取当前登录用户的ID
                param["FriendID"] = option.friendID; //朋友ID
                param["Status"] = option.status; //状态
                $.ajax({
                    type: "POST",
                    url: "/system.axd?t=UserFriendManage",
                    cache: false,
                    dataType: "json",
                    data: $.param(param),
                    success: function (msg) {
                        if (msg.notLogin) {
                            Dialog.alert({ content: msg.notLogin });
                            return;
                        }
                        if (msg.fail) {
                            Dialog.alert({ content: msg.fail, onOkButtonClick: function () {
                                if (isRelaod) document.location = document.location;
                            }
                            });
                        }
                        if (msg.success) {
                            Dialog.alert({ content: "成功发送好友申请！", onOkButtonClick: function () {
                                if (isRelaod) document.location = document.location;
                            }
                            });
                        }
                    }
                });
            }
        },
        agreeOrRefuse: function (option) {
            var param = {};
            param["UserID"] = option.userID; //用户ID,不需要了，在请求处理时直接取当前登录用户的ID
            param["FriendID"] = option.friendID; //朋友ID
            param["Status"] = option.status; //同意拒绝
            $.ajax({
                type: "POST",
                url: "/system.axd?t=AgreeOrRefuse",
                cache: false,
                dataType: "json",
                data: $.param(param),
                success: function (msg) {
                    if (msg.notLogin) {
                        Dialog.alert({ content: msg.notLogin });
                        return;
                    }
                    if (msg.fail) {
                        Dialog.alert({ content: msg.fail, onOkButtonClick: function () {
                            if (isRelaod) document.location = document.location;
                        }
                        });
                    }
                    if (msg.success) {
                        Dialog.alert({ content: "操作成功！", onOkButtonClick: function () {
                            if (isRelaod) document.location = document.location;
                        }
                        });
                    }
                }
            });
        },
        /**
        * 功能：判断用户是否已经登录
        * 参数： 
        */
        isLogin: function (option) {
            $.get("/user.axd?t=IsLogin&rnd=" + Math.random(), function (data) {
                if (data.login && option.login && $.isFunction(option.login)) {
                    option.login();
                }
                else if (!data.login && option.unlogin && $.isFunction(option.unlogin)) {
                    option.unlogin();
                }
            }, "json");
        },
        /**
        * 功能：打开用户快速登录窗口
        * 参数： 
        */
        openQuickLogin: function (option) {
            Dialog.open({
                title: [{ title: "快速登录" }, { title: "快速注册"}],
                width: 468,
                height: 322,
                titleHeight: 49,
                bottomHeight: 55,
                dataSource: 'iframe',
                url: "/user/quickLog.aspx",
                theme: "qlogin",
                arg: option.arg,
                okButton: { value: "确 定", callBack: option.onClose },
                maximizeButton: { isShow: false, callback: undefined }
            });
        },
        /**
        * 功能：在调试状态下显示错误信息
        * 参数： message 需要显示的消息内容，可以为html格式
        */
        debugMsg: function (message) {
            var debuggerWin = window.open("about:blank", "debuggerWin_" + Math.round(Math.random() * 1000), "toolbar=no,menubar=no,scrollbars=yes,resizable=yes,location=no,status=no,width=800,height=600,top=100");
            //debuggerWin.document.title = "出现异常-消息窗口";
            debuggerWin.document.write(message);
        },
        /**
        * 功能：获取指定月份的天数
        * 参数： year-年份;month-月份（0,11）
        */
        getMonthDays: function (year, month) {
            month = parseInt(month, 10) + 1;
            var temp = new Date(year, month, 0);
            return temp.getDate();
        },
        /**
        * 功能：获取指定月份的第一个星期的天数
        * 参数： year-年份;month-月份（0,11）
        */
        getMonthFirstWeek: function (year, month) {
            var temp = new Date(year, month, 1);
            var firstDayWeek = temp.getDay(); //0为星期天
            switch (firstDayWeek) {
                case 0: return 2;
                case 1: return 1;
                case 2: return 7;
                case 3: return 6;
                case 4: return 5;
                case 5: return 4;
                case 6: return 3;
            }
        },
        /**
        * 功能：获取制定月份的周数及每周始末时间点
        * 参数： year-年份;month-月份（0-11）
        * 返回值：包含周的始末时间对象的json的数组，形如[ { start : Date , end : Date } , { start : Date , end : Date } , …]
        */
        getMonthWeeks: function (year, month) {
            var week, day = 1, num, tempObj, result = [];
            var date = new Date(year, month, "1");
            num = $t.getMonthDays(year, month);
            day = $t.getMonthFirstWeek(year, month);
            day = (day == 7 ? 1 : day);
            while (day <= num) {
                result.push({
                    start: new Date(year, month, day),
                    end: new Date(year, month, day + 6)
                });
                day += 7;
            }
            return result;
        }
    };

    window["$t"] = $t;

    _$ui = {
        message: function (option) {
            if (!option.id) return;
            option = $.extend({}, true, { fun: "prepend", hideAfter: false/*若设置此项，则提示信息会在设置的时间后自动隐藏*/ }, option);
            var content = "";
            if (option.content.indexOf("#") == 0) {//从页面的元素中获取提示信息
                content = $(option.content).html();
            }
            else {
                content = option.content;
            }
            var html = [];
            html.push("<div class='msgTip' style='display:none;'>");
            html.push("<span class='icon'></span>");
            html.push("<div class='msgContent'>", content, "</div>");
            html.push("<a class='close'>&#215;</a>");
            html.push("<div class='clear'></div>");
            html.push("</div><div class='clear'></div>");
            var container = $("#" + option.id);
            container[option.fun](html.join(""));
            if (option.width) {
                container.width(option.width);
            }
            $(".msgContent", container).width((container.width() || option.width) - 80);
            var msg = container.children(".msgTip").show();
            msg.children("a.close").click(function () {
                msg.hide();
            });

            if (option.hideAfter && !isNaN(option.hideAfter)) {
                setTimeout(function () {
                    msg.fadeOut("slow");
                }, option.hideAfter * 1000);
            }
        },
        button: {//后台管理添加及编辑页面按钮管理对象
            add: function (buttons, option) {
                if (!option.containerID || !buttons || buttons.length == 0) {
                    return false;
                }
                $container = $("#" + option.containerID);
                $.each(buttons, function (i, n) {
                    if (n.separator) {//分隔符
                        $("<div class='btnseparator'></div>").appendTo($container);
                        return;
                    }
                    if (n.group) {//按钮组
                    }
                    else {
                        var html = [];
                        html.push("<div class='fbutton' id='", n.id, "'><div>");
                        if (n.icon) {
                            html.push("<span class='", n.icon, "'></span><span>", n.value, "</span>");
                        }
                        else {
                            html.push("<span class='", n.className, "'>", n.value, "</span>");
                        }
                        html.push("</div></div>");
                        var btn = $(html.join("")).bind("click", n.eventData, function (event) {
                            if (n.click && $.isFunction(n.click)) {
                                n.click(event, n);
                            }
                            return false;
                        }).appendTo($container);
                    }
                });
                return this;
            },
            setEnabled: function (button, enable) {
                if (typeof button == "string") {
                    button = "#" + button;
                }
                if (enable) {
                    $(button).addClass("enabled");
                }
                else {
                    $(button).removeClass("enabled");
                }
            },
            isEnable: function (button) {
                if (typeof button == "string") {
                    button = "#" + button;
                }
                return $(button).hasClass("enabled");
            }
        },
        tip: function ($o, option) {
            option = $.extend({}, true, { width: 240 }, option);
            $o.cluetip({
                cluetipClass: "orange",
                arrows: true,
                dropShadow: true,
                dropShadowSteps: 3,
                showTitle: false,
                hoverIntent: false,
                topOffset: 0,
                leftOffset: 10,
                width: option.width,
                attribute: "tip",
                local: true
            });
        }
    };

    window["$ui"] = _$ui;

    /***********************
    * 常用正则表达式
    * 2011-11-01
    ***********************/
    var _regex = {
        discount: /^[0-9](\.[0-9])?$/, //折扣
        userName: /^[a-zA-Z][a-zA-Z0-9_]{3,19}$/, //用户名
        money: new RegExp("^([1-9][0-9]*|[0-9])(\\.\\d{1," + $c.decimalDigits.toString() + "})?$"), //金额
        digits: /^([1-9][0-9]*|[0-9])(\.\d{1,2})?$/ //整数或带1-2位小数的数字
    }

    window["$reg"] = _regex;

    /***********************
    * 连接字符串
    * 2011-09-02
    ***********************/
    var StringBuilder = function () {
        this.strArr = new Array();
        this.description = "连接字符串";
        this.versiont = "1.0.0";
    };


    ///<summary>
    /// 在字符串后面追加字符
    ///<summary>
    ///<param name="str">追加的字符串</param>
    StringBuilder.prototype.append = function (str) {
        this.strArr.push(str);
    };

    ///<summary>
    /// 格式化追加字符串
    ///<summary>
    ///<param name="str">追加的字符串[para[n],替换字符串中{n}的标识]</param>
    StringBuilder.prototype.appendFormat = function (str) {
        var len = arguments.length;
        var pattern = "";
        for (var i = 1; i < len; i++) {
            pattern = new RegExp("\\{" + (i - 1).toString() + "\\}", "g");
            str = str.replace(pattern, arguments[i]);
        }
        this.append(str);
    };

    //获取连接的字符串
    //isEmpty:true-并清空字符串数组
    StringBuilder.prototype.toString = function (isEmpty) {
        var s = this.strArr.join("");
        if (isEmpty) {
            this.empty();
        }
        return s;
    };

    //获取字符串的长度
    StringBuilder.prototype.getLen = function () {
        return this.strArr.join("").length;
    };

    //获取存放字符串的数组长度
    StringBuilder.prototype.getArrLen = function () {
        return this.strArr.length;
    };

    //清空字符串数组
    StringBuilder.prototype.empty = function () {
        this.strArr.length = 0;
    };

    window["StringBuilder"] = StringBuilder;

    /********************************************
    * 对xml文档的操作
    * 2011-09-02
    *******************************************/
    var $xml = {
        //加载xml文档
        loadXmlDoc: function (xmlUrl) {
            try {
                xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
            }
            catch (e) {
                try {
                    xmlDoc = document.implementation.createDocument("", "", null);
                }
                catch (e) {
                    alert(e.message);
                }
            }
            try {
                xmlDoc.async = false;
                xmlDoc.load(xmlUrl);
                return xmlDoc;
            }
            catch (e) {
                alert(e.message);
            }
            return null;
        },

        //  将字符串转化为xml文档
        createXmlDoc: function (xmlText) {
            if (!xmlText) {
                return null;
            }
            try {
                var xmlDom = new ActiveXObject("Microsoft.XMLDOM");
                xmlDom.loadXML(xmlText);
                return xmlDom;
            }
            catch (e) {
                try {
                    return new DOMParser().parseFromString(xmlText, "text/xml");
                } catch (e) {
                    return null;
                }
            }
        },

        // 获取节点及其子节点的文本内容xml文档
        getXmlText: function (oNode) {
            if (oNode.text) {//IE
                return oNode.text;
            }
            var sText = "";
            for (var i = 0; i < oNode.childNodes.length; i++) {//FF
                if (oNode.childNodes[i].hasChildNodes()) {
                    sText += getXmlText(oNode.childNodes[i]);
                }
                else {
                    sText += oNode.childNodes[i].nodeValue;
                }
            }
            return sText;
        },

        //获取节点及其子节点的字符串标示
        getXml: function (oNode) {
            if (!oNode) {
                return;
            }
            if (oNode.xml) {
                return oNode.xml; //IE
            }
            var oSerializer = new XMLSerializer();
            return oSerializer.serializeToString(oNode); //FF
        },
        version: "1.0.0"
    };

    window["$xml"] = $xml;

    //时间的格式化显示
    Date.prototype.Format = function (formatStr) {
        var str = formatStr;
        var Week = ['日', '一', '二', '三', '四', '五', '六'];

        str = str.replace(/yyyy|YYYY/, this.getFullYear());
        str = str.replace(/yy|YY/, (this.getYear() % 100) > 9 ? (this.getYear() % 100).toString() : '0' + (this.getYear() % 100));

        str = str.replace(/MM/, this.getMonth() > 8 ? (this.getMonth() + 1).toString() : '0' + (this.getMonth() + 1));
        str = str.replace(/M/g, this.getMonth());

        str = str.replace(/w|W/g, Week[this.getDay()]);

        str = str.replace(/dd|DD/, this.getDate() > 9 ? this.getDate().toString() : '0' + this.getDate());
        str = str.replace(/d|D/g, this.getDate());

        str = str.replace(/hh|HH/, this.getHours() > 9 ? this.getHours().toString() : '0' + this.getHours());
        str = str.replace(/h|H/g, this.getHours());
        str = str.replace(/mm/, this.getMinutes() > 9 ? this.getMinutes().toString() : '0' + this.getMinutes());
        str = str.replace(/m/g, this.getMinutes());

        str = str.replace(/ss|SS/, this.getSeconds() > 9 ? this.getSeconds().toString() : '0' + this.getSeconds());
        str = str.replace(/s|S/g, this.getSeconds());

        return str;
    }

})($);