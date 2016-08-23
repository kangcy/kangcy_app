/***
* javascript通用方法的封装
* 依赖文件：jquery.js
* 2013-09-28
* kcy
*/


(function () {
    /***********************
    * 全局配置类(Config)
    * 2013-09-28
    ***********************/
    $c = {
        //站点根目录
        basePath: "/Web",
        guidEmpty: "00000000-0000-0000-0000-000000000000",

        //金额小数保留的位数
        decimalDigits: 2,

        //上传文件格式
        TxtExtensions: "doc,docx,docm,dotx,dotm,txt,xml,rft,htm,html,mht,mhtml,wps,wtf",
        XlsExtensions: "xls,xlsm,xlsb,xlsm,csv",
        ImageExtensions: "jpg,jpeg,jpe,jfif,png,tif,tiff,gif,bmp,dib",
        UpdateExtensions: "zip,rar",
        //编辑器工具栏配置
        UDArticleTool: [['FullScreen', 'Source', 'Preview', '|', 'Bold', 'Italic', 'Underline', 'ForeColor', 'BackColor', 'FontFamily', 'FontSize', 'Indent', 'LineHeight', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyJustify', 'RemoveFormat', 'AutoTypeSet', '|', 'Link', 'Unlink', 'InsertTable', 'InsertImage', 'Emotion', 'InsertVideo', 'map']],
        UDEasyTool: [['Bold', 'Italic', 'Underline', 'ForeColor', 'BackColor', 'FontFamily', 'FontSize', 'Indent', 'LineHeight', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyJustify', 'RemoveFormat', 'AutoTypeSet', '|', 'Link', 'Unlink', 'InsertImage', 'Emotion', 'InsertVideo']],
        UDUserTool: [['Preview', 'Undo', 'Redo', 'InsertImage', 'Emotion']]
    };
    window["$c"] = $c;


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
        * 功能：比较两个日期的大小(若开始时间小于或等于结束时间，返回true)
        * 返回值：如果startTime>=endTime返回false，否则返回true
        * startTime 开始时间(默认格式：2011-06-20 12:34:56)
        * endTime  结束时间(默认格式：2011-06-20 12:34:56)
        * format 时间格式
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
        * separator  GUID列表的分隔符
        */
        formatGuidList: function (IDList, separator) {
            separator = separator || ",";
            var arrIDList = IDList.split(separator);
            for (var i = 0, len = arrIDList.length; i < len; i++) {
                arrIDList[i] = "'" + arrIDList + "'";
            }
            return arrIDList.join(",");
        },
        //格式化url后面的参数 $t.formatUrlParas("/product.axd?t=ProductIndexReset", [{ key: "rnd"}])
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
        * 功能：判断用户是否已经登录
        * 参数： 
        */
        isLogin: function (option) {
            $.get("system.axd?t=IsLogin&rnd=" + Math.random(), function (data) {
                if (data.login && option.login && $.isFunction(option.login)) {
                    option.login();
                }
                else if (!data.login && option.unlogin && $.isFunction(option.unlogin)) {
                    option.unlogin();
                }
            }, "json");
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
                if (!enable) {
                    $(button).addClass("disabled");
                }
                else {
                    $(button).removeClass("disabled");
                }
            },
            //检查一个按钮是否可用，返回true为可用，返回false为不可以用
            isEnable: function (button) {
                if (typeof button == "string") {
                    button = "#" + button;
                }
                return !$(button).hasClass("disabled");
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

})($);