"use strict";
(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(["jquery"], factory);
    }
    else if (typeof layui === "object" && typeof layui.define === "function") {
        layui.define(['jquery'], function (e) {
            factory(layui.jquery);
            e('Uploader', window.Uploader);
        })
    }
    else {
        factory(jQuery);
    }
}(function ($) {
    function Uploader(settings,events) {

        if (!(this instanceof Uploader)) { return new Uploader(settings, events) };
        this.help = Uploader.Help;
        this.version = Uploader.Version;
        this.support = Uploader.Support;
        if (!Uploader.Support) return false;
        var options = $.extend({}, defaultSettings, settings);
        var context = {
            events: {},
            files: [],
            options: options,
            uploader: this
        }
        if (options.types) { context.types = typeof (options.types) == "string" ? options.types.split(';') : options.types; }
        this.settings = function (name) {
            if (arguments.length == 0) return options;
            return options[name];
        }
        this.on = function (name, func) {
            function subscribe(key, fn) {
                if (!$.isFunction(fn) || !key) return;
                var ev = context.events[key];
                if (!ev) { ev = context.events[key] = $.Callbacks('memory stopOnFalse'); }
                ev.add(fn)
            }
            if ($.isPlainObject(name)) {
                $.each(name, subscribe)
                return;
            }
            subscribe(name, func);
        }
        //上传文件
        this.upload = function (files) {
            for (var i = 0; i < files.length; i++) {
                Selecting(files[i], context)
            }
        }
        //帮事件
        bindClick(context);
        bindDrag(context);
        if (events) {this.on(events);}
       


    }
 
    //绑定click事件
    function bindClick(context) {
        var options = context.options;
        if (options.placeholder) {
            var btn = $(options.placeholder), fileInput;
            var fileSelector = btn.find("[type='file']");
            if (fileSelector.length) { fileInput = fileSelector[0] }
            else { fileSelector = $(fileInput = document.createElement("input")).attr("type", "file").css({ display: 'none', opacity: 0 }).appendTo(btn) } 
            if (options.multiple) { fileSelector.attr("multiple", "multiple") }
            if (options.accept) { fileSelector.attr("accept", options.accept) }
            btn.click(function () { fileInput.click() });
            fileSelector.change(function () {
                context.uploader.upload(this.files);
                fileInput.value = "";
            })
        }
    }
    //绑定拖拽事件
    function bindDrag(context) {
        var uploader = context.uploader, options = context.options;
        if (options.dragable || !options.placeholder) {
            var drp = options.dragContainer, defaultDragContainerFlag = false;
            if (!drp) {
                defaultDragContainerFlag = true;
                drp = $(document.createElement("div"))
                    .attr("data-default-dragContainer", defaultDragContainerFlag)
                    .css({ display: "none", position: "fixed", "z-index": 9999999, top: 0, left: 0, bottom: 0, right: 0, opacity: 0 })
                    .appendTo("body")
            }
            else if (!(drp instanceof jQuery))
            { drp = $(drp) }
            if (drp.length) {
                var dr = drp[0]
                    , dragEnterfn = function (e) {
                        e.preventDefault();
                        if (defaultDragContainerFlag) {
                            clearTimeout(dr.time);
                            dr.time = null;
                            drp.show()
                        }
                        else { drp.addClass("over") }
                    }
                    , dragLeavefn = function (e) {
                        e.preventDefault();
                        if (defaultDragContainerFlag) {
                            if (!dr.time) {
                                dr.time = setTimeout(function () { drp.hide() }, 10);
                            }
                        }
                        else { drp.removeClass("over") }
                    };


                $(document).on({
                    dragleave: dragLeavefn,
                    drop: function (e) {
                        e.preventDefault();
                        drp.removeClass("over")
                    },
                    dragenter: dragEnterfn,
                    dragover: dragEnterfn
                });
                dr.addEventListener("dragleave", function (e) {
                    Publish(uploader, context, "dragleave", e);
                }, false);
                dr.addEventListener("dragover", function (e) {
                    e = e || window.event;
                    e.stopPropagation();
                    dragEnterfn(e);
                    Publish(uploader, context, "dragover", e);
                    //e.dataTransfer.dropEffect = 'copy' //指定拖放视觉效果
                }, false);
                dr.addEventListener("drop", function (e) {
                    e = e || window.event;
                    e.stopPropagation();
                    dragLeavefn(e);
                    //获取文件列表
                    var files = e.dataTransfer ? e.dataTransfer.files : null;
                    e.cancel = false;
                    Publish(uploader, context, "drop", e);
                    if (!e.cancel && files && files.length) { uploader.upload(files); }
                }, false)


            }
        }
    }
    //发布事件
    function Publish() {
        var sender = Array.prototype.shift.call(arguments);
        var context = Array.prototype.shift.call(arguments);
        var name = Array.prototype.shift.call(arguments);
        var event = context.events[name];
        if (event) {
            event.fireWith(sender, arguments);
        }       
    }
    //验证文件
    function Validate(file, context) {
        var n = file.name,
            limitSize = context.options.limitSize,
            args = { invalid: (limitSize && !isNaN(limitSize) && file.size > limitSize), accept: context.options.accept, types: context.types };
        if (!args.invalid) {
            if (context.types) {
                args.invalid = true
                for (var i = 0; i < context.types.length; i++) {
                    var o = $.trim(context.types[i]);
                    if (!o) continue;
                    if (new RegExp(escape(o) + "$", "i").test(n)) {
                        args.invalid = false;
                        file.message = "无效文件类型";
                        break;
                    }
                }
            }
        } else {
            file.limit = true;
            file.message = "超过上传限制的大小";
        }
        Publish(context.uploader, context, "validate", file, args);
        return !args.invalid    
    }
    //错误
    function SendError(prog, file, context, args, msg) {
        prog.hasError = true;
        prog.error = args;
        Publish(prog, context,"error", file, args);
        if (!args.cancel) {
            if (msg = msg || args.message) { prog.view.append("<span class='err-info'>" + msg + "</span>") }
            setTimeout(function () { prog.view.remove() }, 5000)
        }
        Complete(prog, file, context);
    }
    //选择了文件
    function Selected(file, context) {
        if (!context.progress) { context.progress = [] }
        var prog = new Progress(context.uploader, file, context);
        Publish(context.uploader, context, "selected", file);
        if (file.invalid) {
            SendError(prog, file, context,
                {
                    type: file.limit ? Uploader.ErrorType.UpperLimit : Uploader.ErrorType.InvalidType,
                    view: prog.view,
                    message: file.message
                });
            return;
        }
        context.progress.push(prog);
        Upload(file, context);
    }
    ///开始选择文件
    function Selecting(file, context) {
        var args = { cancel: false, invalid: !Validate(file, context) };
        if (args.invalid) { file.invalid = true }
        Publish(context.uploader, context, "selecting", file, args);
        if (!args.cancel) {
            context.files.push(file);
            Selected(file, context)
        }
    }
    //开始上传并创建进度条
    function Upload(file, context) {
        var args = {
                cancel: false,
                maxQueue: context.options.maxQueue,
                queue: context.queue,
                lastIndex: context.lastIndex || 0
            },
            prog = context.progress[args.lastIndex];
        if (typeof (args.queue) !== "number" || args.queue < 0) { args.queue = 0; }
        if (args.lastIndex < 0) { args.lastIndex = 0; }
        if (typeof (args.maxQueue) !== "number" || args.maxQueue < 1) { args.maxQueue = 2; }
        if (args.queue >= args.maxQueue) { return }

        args.progress = prog;
        Publish(context.uploader, context, "upload", file, args);
        if (!args.cancel) {
            context.queue = args.queue + 1;
            context.lastIndex = args.lastIndex + 1;
            prog.proceed()
        }
    }
    //创建进度条视图
    function CreateProgress(prog, file, context) {
        var options = context.options;
        var args = { view: null };
        Publish(prog, context,"createProgress", file, args);
        if (args.view == null) {
            //创建默认视图
            var p = options.progress;
            if (!p) {
                p = options.dragable && (p = options.dragContainer) && (p = p instanceof jQuery ? p : $(p)).length > 0 ? p : $("body")
            }
            var v = args.view = $(document.createElement("div")).appendTo(p);
            v.append("<h2>" + file.name + "</h2><p>size:" + Uploader.SizeToString(file.size) + "</p><progress></progress>")
        }
        return prog.view = args.view
    }
    //更新进度条
    function ProgressBar(prog, file, context, st) {
        var percent, args = { cancel: false, size: prog.size, sizeString: Uploader.SizeToString(prog.size), view: prog.view };
        if (prog.sliced === true) {
            args.loaded = prog.loaded + Math.min(st.currentBlobLoaded, prog.blobSize)
        }
        else {
            args.loaded = Math.min(st.currentBlobLoaded, prog.size)
        }
        percent = (args.loaded / args.size) * 100;
        args.percent = percent;
        args.loadedString = Uploader.SizeToString(args.loaded);
        Publish(prog, context,"progress", file, args)
        if (!args.cancel) {
            prog.bar.attr("value", args.percent)
        }
    }
    //文件上传成功
    function Success(prog,file, context, result) {
        var args = {
            view: prog.view,
            cancel: false,
            result: result,
            req: prog.req,
        };
        Publish(prog, context, "success", file, args);
        if (!args.cancel) {
            setTimeout(function () { prog.view.remove(); }, 5000)
        }
    }
    //文件上传完成
    function Complete(prog, file, context) {
        var args = { view: prog.view, req: prog.req };
        if (!file.invalid) {
            context.queue--;
        }     
        Publish(prog, context,"complete", file, args); 
        Next(context);
    }
    //发送下一个文件
    function Next(context) {
        var i = context.lastIndex;
        if (i < context.progress.length) {
            Upload(context.progress[i].file, context);
        }
    }
    function GetResumableInfo(prog,file, context, back) {
        var data = prog.resumableKey ?
            //获取续传信息
            { key: prog.resumableKey } :
            //创建续传信息
            $.extend({}, context.options.params, {
                fileType: file.type,
                fileName: file.name,
                fileSize: file.size,
                blobSize: context.options.blobSize,
                blobCount: prog.count
            });

        prog.req = $.get({
            url: context.options.url,
            headers: context.options.headers,
            data: data,
            cache: false
        })
            .done(function (info) {
                if (info == null) {
                    if (prog.resumableKey) {
                        //无效的续传key，重新创建一个
                        prog.resumableKey = null;
                        GetResumableInfo(prog, file, context, back);
                        return
                    }
                    SendError(prog, file, context, { type: Uploader.ErrorType.ServerType, message: "创建续传信息失败", view: prog.view });
                    return;
                }
                if (!info.key || !('index' in info)) {
                    SendError(prog, file, context, { type: Uploader.ErrorType.ServerType, message: "返回对象必须包含key和index属性", view: prog.view });
                    console.log(info);
                    return;
                }
                prog.resumableKey = info.key
                back(info);
            })
            .fail(function (req, txt) {
                SendError(prog, file, context, { type: Uploader.ErrorType.ServerType, message: txt || "failed to initialize", view: prog.view });
            })
            .always(function () {
                delete prog.req;
            });
    }
    //上传进度
    function Progress(owner, file, context) {
        this.owner = owner;
        var self = this,
            //设置
            options = context.options,
            //暂停开关
            paused,
            //取消开关
            canceled = false,
            //文件大小
            size = self.size = file.size,
            //数据块的大小
            blobSize = self.blobSize = options.blobSize,
            //是否要切片上传
            sliced = self.sliced = ((sliced = options.sliced) === Uploader.Sliced.Enabled || (sliced === Uploader.Sliced.Auto && size > blobSize) ? true : false),
            //进度视图
            view = (self.view = CreateProgress(self, file, context) || $()),
            //进度条
            bar = (self.bar = view.find("progress").attr({ "max": 100, "value": 0 })),
            //服务端处理地址
            url = options.url,
            //当前数据块索引
            index = 0,
            //数据块总数
            count = sliced === true ? (self.count = Math.ceil(size / blobSize)) : 1,
            //向发送包添加参数
            appendParams = function (d) {

                if (sliced) {
                    d.append("blobIndex", index);
                    d.append("key", self.resumableKey);
                }
                if ($.isPlainObject(options.params)) {
                    $.each(options.params, function (k, v) { d.append(k, v) })
                }

            },
            //发送数据
            send = function (f) {
                if (canceled === true) return;
                var d = new FormData();
                d.append("file", f, file.name);
                appendParams(d);
                return self.req = $.ajax({
                    type: "POST",
                    url: url,
                    headers: options.headers ,
                    contentType: false,
                    processData: false,
                    xhr: function () {
                        var xhr = new XMLHttpRequest();
                        if (typeof (options.timeout) === "number") { xhr.timeout = options.timeout }
                        xhr.upload.addEventListener("progress",
                            function (e) {
                                ProgressBar(self, file, context, { currentBlobLoaded: e.loaded, currentBlobSize: e.total, index: index })
                            },
                            false);
                        return xhr;
                    },
                    data: d
                })
                    .done(function (result) {

                        if (sliced && index < count - 1) {
                            if (paused === true) {
                                delete file.uploading;
                            }
                            else {
                                if (result === true) {
                                    sendBlob(index + 1);
                                } else {
                                    SendError(self, file, context, { message: "服务器返回：" + result });
                                }

                            }
                        }
                        else {
                            Success(self, file, context, result);
                            file.uploaded = true
                        }


                    }).fail(function (req, txt, status) {
                        SendError(self, file, context,
                            {
                                type: Uploader.ErrorType.HttpType,
                                code: status,
                                view: view,
                                message: txt
                            });
                    }).always(function () {
                        if (self.blob) delete self.blob;
                        if (sliced && index >= count - 1 || !sliced) {
                            if (paused === false && canceled === false) {
                                Complete(self, file, context);
                                self.paused = paused = true;
                                delete file.uploading
                            }
                        };
                        delete self.req;
                    });
            },
            //文件切片并发送数据块
            sendBlob = function (i) {
                var start = i * blobSize, end = start + blobSize;
                index = i;
                self.loaded = start;
                self.blob = function (names) {
                    for (var i = 0; i < names.length; i++) {
                        if (file[names[i]]) { return file[names[i]](start, end); }
                    }
                    return null;
                }(['slice', 'webkitSlice', 'mozSlice']);
                return send(self.blob);
            };

        self.file = file;
        self.loaded = 0;
        self.blobSize = blobSize = (sliced ? blobSize : size);
        //暂停操作
        self.pause = function () {
            if (!self.hasError && paused !== true) {
                self.paused = paused = true;
                Publish(self, context, "pause", file, { view: view })
                //context.queue--;
                Next(context);
            }
        };
        //继续传送
        self.proceed = function () {
            if (self.hasError || file.uploading === true) { return; }
            if (file.uploaded === true || canceled===true || file.canceling === true) {
                SendError(self,
                    file,
                    context,
                    {
                        type: Uploader.ErrorType.InvalidOperation,
                        message: file.canceling ? "the file is canceling" : file.uploaded ? "the file was uploaded" : "invalid operation",
                        view: view
                    });
                return
            }

            if (paused === true) {
                var args = { view: view, cancel: false };
                Publish(self, context, "proceed", file, args);
                if (args.cancel) { return; }
            }
            file.cancel = self.paused = paused = false;
            file.uploading = true;
            if (!sliced) {
                send(file);
                return
            };
            //获取续传信息
            GetResumableInfo(self, file, context, function (info) {
                if (file.cancel !== true) {
                    sendBlob(!isNaN(info.index) ? info.index : 0);
                }
            });
       
        };
        //取消上传
        self.cancel = function () {
            if (!self.hasError && file.uploaded !== true && file.cancel !== true && file.canceling !== true) {
                var doit = function (o) {
                    if (o && !(o.err || o.error)) {
                        canceled= file.cancel = true;
                        delete file.canceling;
                        bar.attr("value", 0);
                        Publish(self, context, "cancel", file, { view: view })
                        Complete(self, file, context);
                    }
                };
                self.pause();
                file.canceling = true;
                if (sliced) {
                    self.req= $.ajax({
                        url: url,
                        headers: options.headers,
                        type: "DELETE",
                        data: { key: self.resumableKey },
                    }).done(doit).always(function () {
                        delete self.req;
                    })
                }
                else { doit(true); }
            }
            //
        };
       
    }

    //暴露接口
    window.Uploader = Uploader;
    //静态属性和方法
    Uploader.Version = { major: 2, minor: 0, revision: 0 };
    Uploader.Version.toString = function () { return this.major + "." + this.minor + "." + this.revision };
    //检查浏览器支持
    Uploader.Support = typeof (window.File) !== "undefined" && typeof (window.FileList) !== "undefined" && (typeof (window.Blob) === "function" &&
        (!!window.Blob.prototype.webkitSlice || !!window.Blob.prototype.mozSlice || !!window.Blob.prototype.slice || false));
    Uploader.Sliced = { Auto: 0, Enabled: 1, Disabled: 2 };
    Uploader.ErrorType = {
        //无效的文件类型
        InvalidType: 0
        ,
        //超过文件上限
        UpperLimit: 1
        ,
        HttpType: 2
        ,
        ServerType: 3
        ,
        UserAbort: 4
        ,
        InvalidOperation: 5
    };
    Uploader.SizeToString = function (size, num) {
        if (typeof (size) !== "number") return size;
        var unit = "byte", units = ["KB", "MB", "GB", "TB"], l = 1024, fn = function (n) {
            if (size > l) {
                size = size / l;
                unit = n;
                return true;
            }
            return false;
        };
        for (var i = 0; i < units.length; i++) { if (!fn(units[i])) break }
        l = Math.pow(10, (typeof (num) !== "number" || num <= 0 ? 3 : num));
        return (Math.round(size * l) / l) + unit
    };
    //设置默认配置
    Uploader.Config = function (settings) {
        if ($.isPlainObject(settings)) {
            defaultSettings = $.extend(defaultSettings, settings);
        }
    }
    //默认配置
    var defaultSettings = {
        url: location.href,
        multiple: true,
        ///默认是3MB
        blobSize: 1024 * 1024 * 3,
        sliced: Uploader.Sliced.Auto,
        dragable: false,
        params: {}
    };
    //打印帮助信息
    Uploader.Help = function () {
        var str = "new Uploader(settings,events) 实例化一个上传器，参数 settings 说明："
            + "\n  类型是 object：{...}"
            + "\n  placeholder:\"#btnSelectFiles\"//打开文件选择器的占位符，可选的类型：jQuery对象或者字符串类型的jQuery选择器"
            + "\n  multiple:true //文件选择器对话框是否支持多选,默认值true"
            + "\n  accept:\"image/*,text/xml\" //接受的文件类型，默认值是空的，即是接受全部类型"
            + "\n  dragable:false 可以拖拽文件上传，默认是未开启的"
            + "\n  dragContainer:\"#container\" 拖拽入的容器，可选的类型：jQuery对象或者字符串类型的jQuery选择器"
            + "\n  progress:\"#progressBox\" //文件上传进度列表容器，可选的类型：jQuery对象或者字符串类型的jQuery选择器"
            + "\n  blobSize:number //文件切片上传时，单个数据块的大小，单位是字节"
            + "\n  sliced:number //是否支持切片上传，可用值：Uploader.Sliced.Auto(0),Uploader.Sliced.Enabled(1),Uploader.Sliced.Disabled(2) 默认值 Uploader.Sliced.Auto"
            + "\n  limitSize:number //上传文件大小限制，单位是字节，默认值0 表示没有限制"
            + "\n  url:\"/fileUpload/handler\" //服务端的处理程序，默认值是当前浏览器的地址（location.href）"
            + "\n  url get方法是获取续传信息"
            + "\n  params:{...} //和文件一起提交到服务端的自定义参数，object类型 "
            + "\n 参数 events 说明："
            + "\n selecting 选择文件 function(file,args) args:{ cancel: false,invalidType:false||true }"
            + "\n selected 已经选择了文件 function(file)"
            + "\n validate 验证文件类型时触发 function(file,args) args:{invalid:true||false,accept:\"image/*,text/xml\"} "
            + "\n upload 开始上传文件 function(file,args) args:{cancel: false}"
            + "\n createProgress 创建进度视图 function(file,args) args:{view:null} args.view：返回已经创建的视图"
            + "\n progress 更新进度视图 function(file,args) args: {view:当前视图,cancel: false,size :文件大小,loaded:已经上传的大小,percent:0 ~ 100}"
            + "\n complete 文件上传完成 function(file,args) args:{view:当前视图, req: XMLHttpRequest, status:XMLHttpRequest.status}"
            + "\n success  文件上传成功 function(file,args) args:{view:当前视图,responseText: XMLHttpRequest.responseText,cancel: false, req:XMLHttpRequest}"
            + "\n error 错误处理 function(file,args) args:{ view: 当前视图,type:Uploader.ErrorType,code:number,message:string }"
            + "\n drop 启动拖拽上传时（dragable=true）在拖拽容器上拖拽时触发的事件"
            + "\n dragover 启动拖拽上传时（dragable=true）在拖拽容器上拖拽时触发的事件"
            + "\n dragleave 启动拖拽上传时（dragable=true）从拖拽容器上拽出时触发的事件"
            ;
        //console.log(str);
        return str;
    }

}));