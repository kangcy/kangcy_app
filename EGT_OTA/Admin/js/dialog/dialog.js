/************************************************************
* 弹出对话框
* 2010-12-02
* 依赖文件 jquery.js common.js
* 注：customOption的属性不能存在option及dialog
* CopyRight (c) NanJing DiJing Software Co., Ltd.
************************************************************/

//加载主题样式css文件
$(function () {
    if ($("head:first link[dialog]").length == 0) {
        $("head:first").append("<link rel=\"stylesheet\" type=\"text/css\" href=\"/Admin/js/dialog/dialog.css\" dialog=\"1\" />");
    }
});
(function () {

    var Dialog = function () {

        var currentDialog = null;                               // 当前显示的对话框
        var dialogs = {};                                            // 管理已打开的对话框的对象
        var baseZIndex = 10000;                             // 对话框所处的层
        var cover = { zIndex: 0, layer: undefined };    // 遮罩层
        var topWindow = window;                           // 顶部window对象
        var topDocument = document;                   // 顶部document对象

        // 获取顶级窗口及文档对象
        topWindow = window.parent;
        while (topWindow.parent && topWindow.parent != topWindow) {
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
        topDocument = topWindow.document;

        //对话框对象
        var Dialog = function (option) {
            var opt = $.extend(true, {}, { isTopDialog: false, container: undefined, zIndex: 0, preDialog: undefined, nextDialog: undefined, Id: "" }, option);
            this.isTopDialog = opt.isTopDialog; //是否为顶级的对话框
            this.container = opt.container;
            this.zIndex = opt.zIndex;
            this.preDialog = opt.preDialog;
            this.nextDialog = opt.nextDialog;
            this.Id = opt.Id;
        }

        dialogs = {
            arrDialog: [],
            newId: function () {
                var id = "dialog-" + Math.round(Math.random() * 10000);
                while (this.isExists(id)) {
                    id = "dialog-" + Math.round(Math.random() * 10000);
                }
                return id;
            },
            getDialog: function (id) {
                for (var i = 0, len = this.arrDialog.length; i < len; i++) {
                    if (id == this.arrDialog[i].Id) {
                        return this.arrDialog[i];
                    }
                }
                return undefined;
            },
            removeDialog: function (id) {
                if (typeof id == "object") {
                    id = id.Id;
                }
                for (var i = 0, len = this.arrDialog.length; i < len; i++) {
                    if (id == this.arrDialog[i].Id) {
                        this.arrDialog.splice(i, 1);
                        break;
                    }
                }
            },
            isExists: function (id) {
                for (var i = 0, len = this.arrDialog.length; i < len; i++) {
                    if (id == this.arrDialog[i].Id) {
                        return true;
                    }
                }
                return false;
            }
        };

        //以递增的方式获取对话框的z轴的zIndex值
        var getZIndex = function () {
            return ++baseZIndex;
        }

        // 对话框拖动管理对象
        var dragAndDrop = function () {
            var lastCoords;
            var currentPos;
            var maxWidth;
            var maxHeight;
            //鼠标移动时的处理方法
            var onMouseMove = function (evt) {
                var xPos = 0, yPos = 0;
                if (!lastCoords)
                    return;
                var currentCoords = {
                    x: evt.screenX,
                    y: evt.screenY
                };
                xPos = currentPos.x + (currentCoords.x - lastCoords.x);
                if (xPos < 0) { xPos = 0; }
                else if (xPos + currentDialog.option.width > maxWidth) { xPos = currentPos.x; }
                yPos = currentPos.y + (currentCoords.y - lastCoords.y);
                if (yPos < 0) { yPos = 0; }
                else if (yPos + currentDialog.option.height > maxHeight) { yPos = currentPos.y; }
                currentPos = {
                    x: xPos,
                    y: yPos
                };
                lastCoords = currentCoords;
                $(currentDialog.container).css({ "left": currentPos.x, "top": currentPos.y });
                return false;
            }
            //鼠标松开时的处理方法
            var onMouseUp = function (evt) {
                if (!lastCoords)
                    return;
                $(topWindow.document).unbind("mousemove", onMouseMove);
                $(topWindow.document).unbind("mouseup", onMouseUp);
                $(currentDialog.container).css({ "left": currentPos.x, "top": currentPos.y });
                currentDialog.top = currentPos.y;
                currentDialog.left = currentPos.x;
                lastCoords = null;
                if ($.browser.msie) {
                    currentDialog.container.get(0).releaseCapture();
                }
                else {
                    window.releaseEvents(currentDialog.container.mousemove);
                }
                return false;
            }

            return {
                //鼠标按下时的处理方法
                onMouseDown: function (evt) {
                    var dialog = evt.data.dialog;
                    var zIndex = dialog.zIndex;
                    dialog.zIndex = currentDialog.zIndex;
                    currentDialog.zIndex = zIndex;
                    currentDialog.container.css({ "z-index": currentDialog.zIndex });
                    dialog.container.css({ "z-index": dialog.zIndex });
                    currentDialog = dialog;
                    lastCoords = {
                        x: evt.screenX,
                        y: evt.screenY
                    };
                    currentPos = {
                        x: parseInt($(currentDialog.container).offset().left, 10),
                        y: parseInt($(currentDialog.container).offset().top, 10)
                    };
                    maxWidth = $(document).width();
                    maxHeight = $(document).height();
                    //添加事件监听
                    $(topWindow.document).bind("mousemove", onMouseMove);
                    $(topWindow.document).bind("mouseup", onMouseUp);
                    if ($.browser.msie) {
                        currentDialog.container.get(0).setCapture();
                    } else {
                        window.captureEvents(Event.mousemove);
                    }
                    return false;
                }
            }
        } ();
        //参数调整
        var justifyParam = function (width, height, left, top) {
            var viewSize = $t.getViewportSize(topWindow);
            var scrollPosition = $t.getScrollPosition(topWindow);
            var scrollSize = $t.getPageSize(topWindow);
            var newWidth = width > viewSize.width ? viewSize.width : width;
            var newHeight = height > scrollSize.height ? scrollSize.height : height;
            var newLeft = left, newTop = top;
            if (newWidth + left > scrollPosition.x + viewSize.width) newLeft = scrollPosition.x + viewSize.width - newWidth;
            if (newHeight + top > scrollSize.height) newTop = scrollSize.height - newHeight;
            return { width: newWidth, height: newHeight, left: newLeft, top: newTop }
        }
        //调整窗口大小
        var resizeDialog = {

            //调整尺寸方法
            resize: function (dialog, width, height, left, top) {
                var option = dialog.option;
                var length = arguments.length;
                var content = dialog.container.find(".dialogContent");
                var para = justifyParam(width, height, (length == 5 ? arguments[3] : dialog.left), (length == 5 ? arguments[4] : dialog.top));

                var centerWidth = para.width - 22;
                var centerHeight = para.height;
                var iLeft = para.left;
                var iTop = para.top;

                if (dialog.option.showTitle) { centerHeight -= option.titleHeight; dialog.container.find(".dialogTitle-center").width(centerWidth); }
                if (dialog.option.showBottom) { centerHeight -= option.bottomHeight; dialog.container.find(".dialogBottom-center").width(centerWidth); }
                dialog.container.css({ top: iTop, left: iLeft });
                dialog.container.width(width).height(para.height);
                dialog.container.find(".dialogContent").height(centerHeight - 2);
            },
            max: function (dialog) {
                var isMax = dialog.isMax;
                var dialogTitle = dialog.container.children(".dialogTitle");
                var maxBtn = dialogTitle.find(".dialogMaximize:first");
                if (isMax) {
                    this.resize(dialog, dialog.option.width, dialog.option.height);
                    dialogTitle.bind("mousedown", { dialog: dialog }, dragAndDrop.onMouseDown).css("cursor", "move");
                    dialog.isMax = false;
                    maxBtn.removeClass("dialogNormal"); //.removeClass("dialogNormalHover")
                }
                else {
                    var viewSize = $t.getViewportSize(topWindow);
                    var scrollPosition = $t.getScrollPosition(topWindow);
                    var scrollSize = $t.getPageSize(topWindow);
                    var iTop = Math.max(scrollPosition.y - 10, 0);
                    var iLeft = Math.max(scrollPosition.x - 10, 0);
                    this.resize(dialog, ($(document).width() - scrollPosition.x), scrollSize.height, iLeft, iTop);
                    maxBtn.addClass("dialogNormal"); //.removeClass("dialogMaximizeHover")
                    dialogTitle.unbind("mousedown").css("cursor", "default");
                    dialog.isMax = true;
                }
            }

        }

        var taskBar = function () {
            this.length = 0;
            this.tasks = [];
            return this.init();
        }
        taskBar.prototype = {
            init: function () {
                var bar = this;
                this.container = $("<div class='dialog-taskbar'></div>").hide().appendTo("body");

            },
            add: function (id) {
                var bar = this;
                bar.length += 1;
                bar.tasks.push(id);
                var newTask = $("<a rel='" + id + "' class='dialog-task'></a>").click(function () {
                    var dialog = bar.get(id);

                });

            },
            get: function () {
                return dialogs.getDialog(id);
            },
            remove: function () { }
        }

        //选项默认值
        var defaultOption = {
            name: "", //对话框的名称，若未设置则生成随机的名称
            title: "对话框", //对话框标题
            url: "",   //从iframe框架中打开页面时页面的url
            width: 500, //宽度
            height: 400, //高度
            titleHeight: 32,
            bottomHeight: 30,
            content: "",
            left: 0,
            top: 0,
            theme: "default", //主题
            dataSource: "local", //内容来源，若为#id，则取当前页面内id为id的元素内的内容
            showTitle: true,
            showBottom: true,
            showCover: true,
            showWaiting: false,
            parentWindow: window, //父级窗口
            curWindow: window, //当前窗口（打开对话框的窗口）
            objectIDs: "",
            currentTabIndex: 0,
            okButton: { value: "确 定", isShow: true, callback: undefined }, //确定按钮设置
            cancelButton: { value: "取 消", isShow: true, callback: undefined }, //取消按钮设置
            closeButton: { isShow: true, callback: undefined }, //右上角关闭按钮设置
            maximizeButton: { isShow: true, callback: undefined },
            //customButtons自定义按钮：{id:"btnReport",value:"报 告",click:function(){},callBack:function(){}}
            //callBack属性为当click事件处理方法处于iframe页面内而需要执行一个打开窗口页面内的方法时的传值
            customButtons: [],
            //用以向iframe对话框传值，可以直接设置为option的属性，但是一般集中到option.data属性中来
            data: {}
        }

        //返回窗口中唯一的对话框管理对象
        return {
            /**function******************************
            * 打开新的窗口
            ******************************************/
            open: function (option) {
                var t = this;
                option = $.extend(true, {}, defaultOption, option);
                if (option.objectIDs) {//隐藏父级页面的object元素，防止其出现在对话框之上
                    t.setObjectVisible(option);
                }
                if (option.showCover && !cover.layer) {
                    this.displayCover();
                }

                //计算对话框的位置，将其放置在屏幕的中间
                var viewSize = $t.getViewportSize(topWindow);
                var scrollPosition = $t.getScrollPosition(topWindow);
                var scrollSize = $t.getPageSize(topWindow);
                if (option.width == "max") option.width = $(document).width() - scrollPosition.x;
                if (option.height == "max") option.height = scrollSize.height;

                var iTop = option.top ? option.top : Math.max(scrollPosition.y + (viewSize.height - option.height - 20) / 2, 0);
                var iLeft = option.left ? option.left : Math.max(scrollPosition.x + (viewSize.width - option.width - 20) / 2, 0);

                var para = justifyParam(option.width, option.height, iLeft, iTop)
                option.width = para.width;
                option.height = para.height;
                iLeft = para.left;
                iTop = para.top;

                var centerWidth = option.width - 22;
                var strBuilder = new StringBuilder();

                strBuilder.appendFormat("<div class='dialog-{0}-container'>", option.theme); //容器

                if (option.showTitle) {  //标题
                    strBuilder.appendFormat("<div class='dialogTitle' style='height:{0}px;'>", option.titleHeight);
                    strBuilder.appendFormat("<div class='dialogTitle-left' style='height:{0}px;'></div>", option.titleHeight);
                    strBuilder.appendFormat("<div class='dialogTitle-center' style='width:{0}px;'>", centerWidth);
                    if (typeof (option.title) == "string") {
                        strBuilder.appendFormat("<span class='dialogTitle-text'>{0}</span>", option.title);
                    }
                    else {
                        for (var h = 0, flag = 0; h < option.title.length; h++) {
                            if (option.title[h].current) { flag = h; break; }
                        }
                        for (var i = 0, item; item = option.title[i++]; ) {
                            strBuilder.appendFormat("<span class='dialogTitle-text " + (i == flag ? "dialogTitle-current " : "") + (i == option.title.length ? "dialogTitle-last" : "") + "'>{0}</span>", item.title);
                        }
                    }

                    if (option.closeButton.isShow) strBuilder.append("<span class='dialogClose' name='closeButton'>&#215;</span>");
                    if (option.maximizeButton.isShow) strBuilder.append("<span class='dialogMaximize' name='maximizeButton'></span>");
                    strBuilder.append("</div>");

                    //strBuilder.appendFormat("<div class='dialogTitle-center' style='width:{0}px;'><span class='dialogTitle-text'>{1}</span>{2}{3}</div>",
                    // centerWidth, option.title, option.closeButton.isShow ? "<span class='dialogClose' name='closeButton'>&#215;</span>" : "", option.maximizeButton.isShow ? "<span class='dialogMaximize' name='maximizeButton'></span>" : "");
                    strBuilder.appendFormat("<div class='dialogTitle-right' style='height:{0}px;'></div>", option.titleHeight);
                    strBuilder.append("</div>");
                }

                strBuilder.append("<div class='dialogContent'></div>");  //内容

                if (option.showBottom) {   //底部
                    strBuilder.appendFormat("<div class='dialogBottom' style='height:{0}px'>", option.bottomHeight);
                    strBuilder.append("<div class='dialogBottom-left'>&nbsp;</div>");
                    strBuilder.appendFormat("<div class='dialogBottom-center' style='width:{0}px;'></div>", centerWidth);
                    strBuilder.append("<div class='dialogBottom-right'>&nbsp;</div>");
                    strBuilder.append("</div>");
                }
                strBuilder.append("</div>");

                var dialog = new Dialog();
                dialog.option = option;
                dialog.zIndex = getZIndex();
                dialog.top = iTop;
                dialog.left = iLeft;
                dialog.isMax = false;
                dialog.container = $(strBuilder.toString()).css({ "z-index": dialog.zIndex, top: iTop, left: iLeft, width: option.width/*, height: option.height*/ }).appendTo(topDocument.body).focus();
                dialog.Id = option.Id || dialogs.newId();
                //对话框顶部标题
                var dialogTitle = dialog.container.children(".dialogTitle");
                var dialogBottom = dialog.container.find(".dialogBottom-center");

                var okButton; //确定按钮
                if (option.showBottom && option.okButton.isShow) {
                    okButton = $("<a  class='normal dialog-ok' name='okButton'>" + option.okButton.value + "</a>").appendTo(dialogBottom);
                }

                var arrCustomButtons = [];
                if (option.showBottom && option.customButtons && option.customButtons.length > 0) { //自定义按钮
                    $.each(option.customButtons, function (i, n) {
                        this.button = $("<a  class='normal' name='" + (n.id ? n.id : "customButton") + "'>" + this.value + "</a>").appendTo(dialogBottom);
                    });
                }

                var cancelButton; //取消按钮
                if (option.showBottom && option.cancelButton.isShow) {
                    cancelButton = $("<a  class='normal dialog-cancel' name='cancelButton' name='cancelButton'>" + option.cancelButton.value + "</a>").appendTo(dialogBottom);
                }

                var contentHeigth = option.height - (option.showTitle ? dialogTitle.height() : 0);
                contentHeigth = contentHeigth - (option.showBottom ? dialogBottom.height() : 0);
                var dialogContent = dialog.container.children(".dialogContent").height(contentHeigth - 14);

                if (option.showWaiting) {//显示加载的图标
                    dialogContent.append("<div class='dialogLoading'></div>");
                }

                //加载对话框的内容
                if (option.dataSource == "local") {
                    dialogContent.html(option.content.replace(/\n/g, "<br/>"));
                }
                else if (option.dataSource == "iframe") {
                    dialogContent.css({ "padding": 0 }).height(dialogContent.height() + 10);
                    var iframe = $("<iframe  src='" + option.url + "' width='100%' height='100%' scrolling='auto' frameborder='0'  allowtransparency='true' class='dialogIframe'></iframe>").appendTo(dialogContent);
                    dialog.iframeWindow = iframe.get(0).contentWindow || iframe.get(0).contentDocument.parentWindow;
                    if (option.okButton && option.okButton.callBack) {//使用短名称引用回调函数
                        dialog.onClose = option.okButton.callBack;
                    }
                    if (option.showWaiting) {
                        $(dialog.iframeWindow).bind("load", function () {
                            dialogContent.children(".dialogLoading").remove();
                            $(this).find("body").focus();
                        });
                    }
                    $(iframe.get(0).contentDocument).bind("keydown", { dialog: dialog }, function (event) {
                        if (event.target.tagName.toLowerCase() == "textarea") {
                            return;
                        }
                        if (event.keyCode == 13 && dialog.iframeWindow["okButtonClick"] && $.isFunction(dialog.iframeWindow["okButtonClick"])) {
                            dialog.iframeWindow["okButtonClick"](event);
                        }
                        if (event.keyCode == 27) {
                            if (dialog.iframeWindow["cancelButtonClick"] && $.isFunction(dialog.iframeWindow["cancelButtonClick"])) {
                                dialog.iframeWindow["cancelButtonClick"](event);
                            }
                            t.close(event.data.dialog);
                        }
                    })
                    iframe.get(0).dialog = dialog;
                }
                else if (option.dataSource == "ajax") {
                    dialogContent.load(option.url, function (data) {
                        if (option.showWaiting) {
                            dialogContent.children(".dialogLoading").remove();
                        }
                        data = data.replace(/\n/g, "<br/>");
                        dialogContent.html(data);
                    });
                }
                else if (option.dataSource.indexOf("#") != -1) {
                    dialogContent.html($(option.dataSource).html());
                }
                //设置对话框的关系
                if (currentDialog) {//已经存在打开的窗口
                    dialog.preDialog = currentDialog;
                    if (currentDialog.nextDialog) {
                        currentDialog.nextDialog.preDialog = dialog;
                        dialog.nextDialog = currentDialog.nextDialog;
                    }
                    dialog.preDialog.nextDialog = dialog;

                    if (option.showCover && cover.layer) {//将不是当前对话框的对话框移动到蒙层的下面
                        $(dialog.preDialog.container).css({ "z-index": cover.zIndex - 1 });
                    }
                }
                currentDialog = dialog; //将当前窗口设置为新打开的窗口
                dialogs.arrDialog.push(dialog); //将dialog添加到已打开对话框的数组中

                /******************************************
                *  绑定事件处理方法
                ******************************************/
                //绑定标签的点击事件
                if (option.title && typeof (option.title) != "string") {
                    dialogTitle.find(".dialogTitle-text").each(function (i, n) {
                        $(n).bind("click", { dialog: dialog }, function (event) {
                            if ($(this).attr("enabled")) return false; //已经被禁用
                            var dialog = event.data.dialog;
                            var option = dialog.option;
                            if ($(n).hasClass("dialogTitle-current")) return false;
                            dialogTitle.find(".dialogTitle-current").removeClass("dialogTitle-current");
                            $(this).addClass("dialogTitle-current");
                            option.currentTabIndex = i;
                            if (dialog.iframeWindow && dialog.iframeWindow.tabClick) {
                                dialog.iframeWindow.tabClick[i](dialog);
                            }
                            else if (option.title[i].click && $.isFunction(option.title[i].click)) {//执行回调函数
                                option.title[i].click(dialog);
                            }
                            return false;
                        });
                    }).mousedown(function () { return false; });
                    dialogTitle.find(".dialogTitle-current").trigger("click");
                }
                //绑定对话框的点击事件，将对话框设置为当前对话框并在最前面显示
                $(dialog.container).bind("click", { option: option, dialog: dialog }, function (event) {
                    var dialog = event.data.dialog;
                    var option = event.data.option;
                    var zIndex = dialog.zIndex;
                    dialog.zIndex = currentDialog.zIndex;
                    currentDialog.zIndex = zIndex;
                    currentDialog.container.css({ "z-index": currentDialog.zIndex });
                    dialog.container.css({ "z-index": dialog.zIndex });
                    currentDialog = dialog;
                });

                //绑定标题栏鼠标按下时的处理方法，以使对话框能够拖动
                dialogTitle.bind("mousedown", { dialog: dialog }, dragAndDrop.onMouseDown);

                //绑定【确定】按钮单击事件处理方法
                if (okButton && okButton.length > 0) {
                    okButton.bind("click", { dialog: dialog }, function (event) {
                        if ($(this).attr("enabled")) return false; //已经被禁用
                        var dialog = event.data.dialog;
                        var option = dialog.option;
                        if (dialog.iframeWindow && dialog.iframeWindow.okButtonClick && $.isFunction(dialog.iframeWindow.okButtonClick)) {
                            if (dialog.iframeWindow.okButtonClick(option.currentTabIndex)) {
                                t.close(dialog);
                            }
                        }
                        else {
                            if (option.okButton.callBack && $.isFunction(option.okButton.callBack)) {//执行回调函数
                                option.okButton.callBack();
                            }
                            t.close(dialog, option);
                        }
                        return false;
                    });
                }

                //绑定【自定义按钮】单击事件处理方法
                $.each(option.customButtons, function (i, n) {
                    n.button.bind("click", { dialog: dialog }, function (event) {
                        if ($(this).attr("enabled")) return false; //已经被禁用
                        var dialog = event.data.dialog;
                        if (n.click && $.isFunction(n.click)) {
                            n.click(event, dialog);
                            if (n.closeDialog) {
                                t.close(dialog);
                            }
                        }
                        else if (n.click && typeof n.click == "string" &&
                                dialog.iframeWindow && dialog.iframeWindow[n.click] && $.isFunction(dialog.iframeWindow[n.click])) {
                            if (dialog.iframeWindow[n.click]()) {
                                t.close(dialog);
                            }
                        }
                        return false;
                    });
                });

                //绑定【取消】按钮单击事件处理方法
                if (cancelButton && cancelButton.length > 0) {
                    cancelButton.bind("click", { dialog: dialog }, function (event) {
                        if ($(this).attr("enabled")) return false; //已经被禁用
                        var dialog = event.data.dialog;
                        t.close(dialog);
                        if (option.cancelButton.callBack && $.isFunction(option.cancelButton.callBack)) {//执行回调函数
                            option.cancelButton.callBack(dialog);
                        }
                        return false;
                    });
                }

                //绑定标题栏右上角【关闭按钮】单击及鼠标移入移出事件
                if (option.closeButton && option.closeButton.isShow) {
                    dialogTitle.find(".dialogClose:first").bind("click", { dialog: dialog }, function (event) {
                        var dialog = event.data.dialog;
                        t.close(dialog);
                        if (option.closeButton.callBack && $.isFunction(option.closeButton.callBack)) {//执行回调函数
                            option.closeButton.callBack(dialog);
                        }
                        return false;
                    }).hover(function () {
                        $(this).addClass("dialogCloseHover");
                    }, function () {
                        $(this).removeClass("dialogCloseHover");
                    });
                }
                //绑定标题栏右上角【最大化按钮】单击及鼠标移入移出事件
                //t.resize(dialog);
                if (option.maximizeButton && option.maximizeButton.isShow) {
                    var maxBtn = dialogTitle.find(".dialogMaximize:first");
                    maxBtn.bind("click", { dialog: dialog }, function (event) {
                        var dialog = event.data.dialog;
                        resizeDialog.max(dialog);
                        if (option.maximizeButton.callBack && $.isFunction(option.maximizeButton.callBack)) {//执行回调函数
                            option.maximizeButton.callBack(dialog, dialog.isMax);
                        }
                        return false;
                    })
                    //                    .hover(function () {
                    //                    if(dialog.isMax) $(this).addClass("dialogNormalHover");
                    //                    else $(this).addClass("dialogMaximizeHover");
                    //                    }, function () {
                    //                    if(dialog.isMax) $(this).removeClass("dialogNormalHover");
                    //                    else $(this).removeClass("dialogMaximizeHover");
                    //                    });
                }
                //定时关闭对话框
                if (option.autoClose) {
                    var closeOption = option.autoClose;
                    if (isNaN(parseInt(closeOption.delay))) return;
                    dialog.delay = setTimeout(function () {
                        if (closeOption.showConfirm) {
                            t.confirm({
                                confirm: function () {
                                    t.close();
                                    t.close(dialog);
                                },
                                content: '确定关闭对话框' + closeOption.confirmTitle + '？'
                            });
                        }
                        else { t.close(dialog); }
                    }, parseInt(closeOption.delay));
                }
            },
            /**function******************************
            * 缩放弹出窗口
            ******************************************/
            resize: function (dialog, width, height) {
                dialog = dialog || currentDialog;
                resizeDialog.resize(dialog, width, height);
            },
            /**function******************************
            * 弹出信息提示窗口
            ******************************************/
            alert: function (option) {
                var opt = $.extend(true, {}, {
                    title: "提示信息",
                    width: 350,
                    height: 210,
                    theme: "gray",
                    cancelButton: { isShow: false },
                    closeButton: { isShow: false },
                    okButton: {}
                },
                 option);
                opt.okButton.callBack = opt.onOkButtonClick;
                this.open(opt);
            },

            /**function******************************
            * 弹出确认窗口
            ******************************************/
            confirm: function (option) {
                var opt = $.extend(true, {}, { title: "确认信息", theme: "gray", width: 350, height: 210, closeButton: { isShow: false }, okButton: {}, cancelButton: {} }, option);
                opt.okButton.callBack = opt.confirm;
                opt.cancelButton.callBack = opt.cancel;
                this.open(opt);
            },

            /**function******************************
            * 关闭对话框
            ******************************************/
            close: function (dialog) {
                dialog = dialog || currentDialog;
                var option = dialog.option;
                var closeCurrentDialog = dialog.Id == (currentDialog && currentDialog.Id); //是否为关闭当前窗口

                if (option && option.objectIDs) {
                    this.setObjectVisible(option);
                }

                //更新对话框的关系
                if (dialog.preDialog && dialog.nextDialog) {
                    dialog.preDialog.nextDialog = dialog.nextDialog;
                    dialog.nextDialog.preDialog = dialog.preDialog;
                }
                else if (dialog.preDialog) {
                    dialog.preDialog.nextDialog = undefined;
                }
                else if (dialog.nextDialog) {
                    dialog.nextDialog.preDialog = undefined;
                }

                //如果关闭的为当前对话框且存在其他的对话框，默认设置其前一个窗口（优先）或后一个窗口为当前对话框
                if (closeCurrentDialog && dialogs.arrDialog.length > 1) {
                    currentDialog = dialog.preDialog || dialog.nextDialog;
                    currentDialog.zIndex = dialog.zIndex;
                    currentDialog.container.css({ "z-index": dialog.zIndex });
                }

                dialog.container.remove(); //移除需要关闭的窗口
                dialogs.removeDialog(dialog); //从已打开对话框数组中移除关闭的对话框

                //按照实际情况关闭蒙层
                if (dialogs.arrDialog.length == 0) {//如果已不存在对话框，关闭蒙层
                    this.removeCover();
                    currentDialog = null;
                    return;
                }
                var removeCover = true;
                $.each(dialogs.arrDialog, function (i, n) {//检测所有的打开窗口中是否存在需要蒙层的窗口
                    if (n.option.showCover) {
                        removeCover = false;
                        return false;
                    }
                });
                if (removeCover) {
                    this.removeCover();
                }
            },

            /**function******************************
            * 添加蒙层
            ******************************************/
            displayCover: function (doc) {
                cover.layer = topDocument.createElement("div");
                cover.zIndex = getZIndex();

                var relElement = $t.isStrictMode ? topDocument.documentElement : topDocument.body;
                var size = {
                    'width': Math.max(relElement.scrollWidth, relElement.clientWidth, topDocument.scrollWidth || 0) - 1 + 'px',
                    'height': Math.max(relElement.scrollHeight, relElement.clientHeight, topDocument.scrollHeight || 0) - 1 + 'px'
                };

                $(cover.layer).addClass("divCover").css({
                    "z-index": cover.zIndex,
                    "width": size.width,
                    "height": size.height
                });

                //创建一个IFRAME覆盖页面上的select等元素，仅当浏览器为IE6时
                if ($.browser.msie && !($.browser.version > 6)) {
                    var iframe = topDocument.createElement('iframe');
                    iframe.hideFocus = true;
                    iframe.frameBorder = 0.1;
                    $(iframe).addClass("iframeCover").css({
                        "width": size.width,
                        "height": size.height
                    }).attr("src", "about:blank");
                    cover.layer.appendChild(iframe);
                }

                topDocument.body.appendChild(cover.layer);
            },

            /**function******************************
            * 移除遮罩层
            ******************************************/
            removeCover: function () {
                if (cover && cover.layer) {
                    $(cover.layer).remove();
                    cover = { layer: null, zIndex: 0 };
                }
            },

            /**function******************************
            * 控制object元素的隐藏和显示(项目专用)
            ******************************************/
            setObjectVisible: function (option) {
                var arrObjectID = option.objectIDs.split(",");
                for (var i = 0, len = arrObjectID.length; i < len; i++) {
                    var officeObject = option.parentWindow.document.getElementById(arrObjectID[i]);
                    if (!officeObject) {
                        return;
                    }

                    if (officeObject.style.display == "none") {
                        officeObject.style.display = "";
                    }
                    else {
                        officeObject.style.display = "none";
                    }
                }
            },

            /**function******************************
            * 设置按钮的可用性
            ******************************************/
            setButtonEnabled: function (id, enabled, dialog) {
                dialog = dialog || currentDialog;
                if (!dialog) {
                    return;
                }
                var buttons = (id == "ALL" ? $(".dialogBottom a", dialog.container) : $("a[name='" + id + "']", dialog.container));

                if (enabled) {
                    buttons.removeAttr("enabled");
                }
                else {
                    buttons.attr("enabled", "enabled");
                }
            },

            version: "1.3.0"

        };
    }

    window["Dialog"] = Dialog();

})();