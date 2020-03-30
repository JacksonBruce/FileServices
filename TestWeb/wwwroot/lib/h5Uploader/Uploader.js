
(function () {
    "use strict";


    //公开方法与构造函数
    //实例化一个上传器，参数settings说明：
    //object类型：{...}
    // placeholder:"#btnSelectFiles"//打开文件选择器的占位符，可选的类型：jQuery对象或者字符串类型的jQuery选择器
    // multiple:true //文件选择器对话框是否支持多选,默认值true
    // accept:"image/*,text/xml" //接受的文件类型，默认值是空的，即是接受全部类型
    // dragable:false 可以拖拽文件上传，默认是未开启的
    // dragContainer:"#container" 拖拽入的容器，可选的类型：jQuery对象或者字符串类型的jQuery选择器
    // progress:"#box" //文件上传进度列表容器，可选的类型：jQuery对象或者字符串类型的jQuery选择器
    // blobSize:number //文件切片上传时，单个数据块的大小，单位是字节
    // sliced:number //是否支持切片上传，可用值：Uploader.Sliced.Auto(0),Uploader.Sliced.Enabled(1),Uploader.Sliced.Disabled(2) 默认值 Uploader.Sliced.Auto
    // limitSize:number //上传文件大小限制，单位是字节，默认值0 表示没有限制
    // url:"/fileUpload/handler" //服务端的处理程序，默认值是当前浏览器的地址（location.href）
    // params:{...} //和文件一起提交到服务端的自定义参数，object类型 
    // parseResult //这个是个函数用来解析服务端返回的结果集 function(serverData) 返回值是object，如果服务端有错误应该返回{err:true,msg:“错误描述”}的对象
    var u = window.Uploader = function (settings, events) {

        ///将帮助信息输出到控制台，如果不要这些帮助信息，可以用 if (!(this instanceof u)){return new u(settings,events)}; 代替下面的if语句
        if (!(this instanceof u)) {
            console.log("实例化一个上传器，参数settings说明："
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
    + "\n  params:{...} //和文件一起提交到服务端的自定义参数，object类型 "
    + "\n  parseResult: function(serverData) //这个是个函数用来解析服务端返回的结果集,返回值是object，如果服务端有错误应该返回{err:true,msg:\"错误描述\"}的对象");
            alert("文件上传器实例化的参数说明已经输出到控制台");
            return;
        }
        var s = this
            , opts = settings || {}
            , context = { events: {}, files: [] }
            , sltfsfn = s.upload = function (files) {
                for (var i = 0; i < files.length; i++) {
                    selecting.apply(s, [files[i], context])
                }
            };

        s.version = u.Version;
        //检查浏览器支持
        s.support = u.Support;
        if (!s.support) return false;

        //读取设置
        s.settings = function (n) {
            var v = opts[n];
            return typeof (v) === udfs || v == null ? defaultSettings[n] : v;
        };
        if (typeof (opts.params) === udfs) { opts.params = {} }
        if (opts.types) { context.types = typeof (opts.types) == "string" ? opts.types.split(';') : opts.types; }

        //事件绑定 可用事件：
        //selecting 选择文件 function(file,args) args:{ cancel: false,invalidType:false||true }
        //selected 已经选择了文件 function(file)
        //upload 开始上传文件 function(file,args) args:{cancel: false}
        //createProgress 创建进度视图 function(file,args) args:{view:null} args.view：返回已经创建的视图 
        //getResumableInfoHandler 获取续传信息时触发 function(url,params,callback) url：服务端处理程序，params：文件参数 {fileType:string,fileName:string,fileSize:number,blobSize: number,blobCount:number} callback:function(ResumableInfo:{key,index}) 回调函数
        //progress 更新进度视图 function(file,args) args: {view:当前视图,cancel: false,size :文件大小,loaded:已经上传的大小,percent:0 ~ 100}
        //complete 文件上传完成 function(file,args) args:{view:当前视图, req: XMLHttpRequest, status:XMLHttpRequest.status}
        //success  文件上传成功 function(file,args) args:{view:当前视图,responseText: XMLHttpRequest.responseText,cancel: false, req:XMLHttpRequest,responseType: XMLHttpRequest.responseType, responseXML:XMLHttpRequest.responseXML}
        //error 错误处理 function(file,args) args:{ view: 当前视图,type:Uploader.ErrorType,code:number,message:string }
        //drop 启动拖拽上传时（dragable=true）在拖拽容器上拖拽时触发的事件
        //dragover 启动拖拽上传时（dragable=true）在拖拽容器上拖拽时触发的事件
        //dragleave 启动拖拽上传时（dragable=true）从拖拽容器上拽出时触发的事件

        s.on = function (a) {
            var args = arguments;
            if (args.length == 2) { setHandlers(args[0], context.events, args[1]) }
            else if (a) {
                for (var n in a) { setHandlers(n, context.events, a[n]) }
            }
            //这个if语快是可以移除的，它将事件绑定的说明输出到控制台
            if (args.length === 0) {
                console.log("\n selecting 选择文件 function(file,args) args:{ cancel: false,invalidType:false||true }"
                    + "\n selected 已经选择了文件 function(file)"
                    + "\n validate 验证文件类型时触发 function(file,args) args:{invalid:true||false,accept:\"image/*,text/xml\"} "
                    + "\n upload 开始上传文件 function(file,args) args:{cancel: false}"
                    + "\n createProgress 创建进度视图 function(file,args) args:{view:null} args.view：返回已经创建的视图"
                    + "\n getResumableInfoHandler 获取续传信息时触发 function(url,params,callback) url：服务端处理程序，params：文件参数 {fileType:string,fileName:string,fileSize:number,blobSize: number,blobCount:number} callback:function(ResumableInfo:{key,index}) 回调函数"
                    + "\n progress 更新进度视图 function(file,args) args: {view:当前视图,cancel: false,size :文件大小,loaded:已经上传的大小,percent:0 ~ 100}"
                    + "\n complete 文件上传完成 function(file,args) args:{view:当前视图, req: XMLHttpRequest, status:XMLHttpRequest.status}"
                    + "\n success  文件上传成功 function(file,args) args:{view:当前视图,responseText: XMLHttpRequest.responseText,cancel: false, req:XMLHttpRequest,responseType: XMLHttpRequest.responseType, responseXML:XMLHttpRequest.responseXML}"
                    + "\n error 错误处理 function(file,args) args:{ view: 当前视图,type:Uploader.ErrorType,code:number,message:string }"
                    + "\n drop 启动拖拽上传时（dragable=true）在拖拽容器上拖拽时触发的事件"
                    + "\n dragover 启动拖拽上传时（dragable=true）在拖拽容器上拖拽时触发的事件"
                    + "\n dragleave 启动拖拽上传时（dragable=true）从拖拽容器上拽出时触发的事件");
                alert("事件绑定的说明已经输出到控制台。");
            }

        };
        if (events) { s.on(events) }
        var multiple = s.settings("multiple"), accept = s.settings("accept"), fs;
        //
        if (opts.placeholder) {
            var btn = $(opts.placeholder), fileSelector;
            fs = btn.find("[type='file']");
            if (!fs.length) {
                fs = $(fileSelector = document.createElement("input")).attr("type", "file").css({ display: 'none', opacity: 0 }).appendTo(btn)
            } else { fileSelector = fs[0] }
            btn.click(function () { fileSelector.click() });
            if (multiple) { fs.attr("multiple", "multiple") }
            if (accept) { fs.attr("accept", accept) }
            fs.change(function () { sltfsfn(this.files) })
        }
        //else {
        //    fs = $("input[type='file']")//.on("change", function () { sltfsfn(this.files) });
        //}
        if (s.settings("dragable") || !opts.placeholder) {
            var drp = s.settings("dragContainer"), defaultDragContainerFlag = false;
            if (!drp) {
                defaultDragContainerFlag = true;
                drp = $(document.createElement("div")).attr("data-default-dragContainer", defaultDragContainerFlag).css({ display: "none", position: "fixed", "z-index": 9999999, top: 0, left: 0, bottom: 0, right: 0, opacity: 0 }).appendTo("body")
            }
            else if (!(drp instanceof jQuery)) { drp = $(drp) }
            if (drp.length) {
                var dr = drp[0]
                    , dragEnterfn = function (e) {
                        e.preventDefault();
                        if (defaultDragContainerFlag) { clearTimeout(dr.time); dr.time = null; drp.show() }
                        else { drp.addClass("over") }
                    }
                    , dragLeavefn = function (e) {
                        e.preventDefault();
                        if (defaultDragContainerFlag) {
                            if (!dr.time)
                            { dr.time = setTimeout(function () { drp.hide() }, 10) }
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
                    triggerEvents("dragleave", context, function (fn) { fn.call(s, e) })
                }, false);
                dr.addEventListener("dragover", function (e) {
                    e = e || window.event;
                    e.stopPropagation();
                    dragEnterfn(e);
                    triggerEvents("dragover", context, function (fn) { fn.call(s, e) });
                    //e.dataTransfer.dropEffect = 'copy' //指定拖放视觉效果
                }, false);
                dr.addEventListener("drop", function (e) {
                    e = e || window.event;
                    e.stopPropagation();
                    dragLeavefn(e);
                    //获取文件列表
                    var files = e.dataTransfer ? e.dataTransfer.files : null;
                    e.cancel = false;
                    triggerEvents("drop", context, function (fn) { fn.call(s, e) });
                    if (!e.cancel && files && files.length) { sltfsfn(files) }
                }, false)


            }
        }
    }
    , udfs = 'undefined';
    //静态属性和方法
    u.Version = { major: 1, minor: 0, revision: 0 };
    u.Version.toString = function () { return this.major + "." + this.minor + "." + this.revision };
    //检查浏览器支持
    u.Support = typeof (window.File) !== udfs && typeof (window.FileList) !== udfs && (typeof (window.Blob) === "function" &&
        (!!window.Blob.prototype.webkitSlice || !!window.Blob.prototype.mozSlice || !!window.Blob.prototype.slice || false));
    u.Sliced = { Auto: 0, Enabled: 1, Disabled: 2 };
    u.ErrorType = {
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
    u.SizeToString = function (size, num) {
        if (typeof (size) !== "number") return size;
        var unit = "byte", units = ["KB", "MB", "GB", "TB"], l = 1024, fn = function (n) {
            if (size > l) {
                size = size / l;
                unit = n;
                return true;
            }
            return false;
        };
        for (var i = 0; i < units.length; i++)
        { if (!fn(units[i])) break }
        l = Math.pow(10, (typeof (num) !== "number" || num <= 0 ? 3 : num));
        return (Math.round(size * l) / l) + unit
    };
    //默认配置
    var defaultSettings = {
        url: location.href
        , multiple: true
        ///默认是3MB
        , blobSize: 1024 * 1024 * 3
        , sliced: u.Sliced.Auto
        , dragable: false
    };
    //私有方法
    function isValidType(file, context) {
        var s = this, n = file.name, did = false, args = { invalid: true, accept: s.settings("accept"), types: context.types };
        triggerEvents("validate", context, function (fn) { fn.call(s, file, args); did = true });
        if (did) { return !args.invalid }
        if (!context.types) return true;
        for (var i = 0; i < context.types.length; i++) {
            var o = $.trim(context.types[i]);
            if (!o) continue;
            if (new RegExp(escape(o) + "$", "i").test(n)) {
                return true
            }
        }
        return false;
    }
    function setHandlers(n, e, fn) {
        if (typeof (n) !== "string" || typeof (fn) !== "function") return;
        var h = e[n = n.toUpperCase()];
        if (!h) { h = e[n] = [] }
        h.push(fn)

    }
    function getHandlers(n, e) {
        return typeof (n) === "string" && e ? e[n.toUpperCase()] : null
    }
    function triggerEvents(n, c, b) {
        var h = getHandlers(n, c.events);
        if (h) {
            for (var i = 0; i < h.length; i++) {
                var fn = h[i];
                if (typeof (fn) === "function") {
                    try { b(fn) } catch (x) { console.log(x, fn) }
                }
            }
        }
    }
    //事件
    function error(file, context, args, msg) {
        var s = this;
        s.hasError = true;
        s.error = args;
        triggerEvents("error", context, function (fn) { fn.call(s, file, args) });
        if (!args.cancel) {
            if (msg = msg || args.message) { s.view.append("<span class='err-info'>" + msg + "</span>") }
            setTimeout(function () { s.view.remove() }, 5000)
        }
        next(context);
    }
    function selected(file, context) {
        if (!context.progress) { context.progress = [] }
        var s = this, prog = new Progress(s, file, context);
        triggerEvents("selected", context, function (fn) { fn.call(s, file) });
        context.progress.push(prog);
        upload.call(s, file, context);
    }
    function selecting(file, context) {
        var s = this, args = { cancel: false, invalidType: !isValidType.call(s, file, context) };
        if (args.invalidType) { file.invalidType = true }
        triggerEvents("selecting", context, function (fn) { fn.call(s, file, args) });
        if (!args.cancel) {
            context.files.push(file);
            selected.apply(s, [file, context])
        }
    }

    function upload(file, context) {
        var s = this, prs = context.progress, args = { cancel: false, maxQueue: s.settings("maxQueue"), queue: context.queue, lastIndex: context.lastIndex || 0 }
            , prog = context.progress[args.lastIndex];
        if (typeof (args.queue) !== "number" || args.queue < 0) { args.queue = 0; }
        if (args.lastIndex < 0) { args.lastIndex = 0; }
        if (typeof (args.maxQueue) !== "number" || args.maxQueue < 1) { args.maxQueue = 2; }
        if (args.queue >= args.maxQueue)
        { return }
        context.queue = args.queue + 1;
        context.lastIndex = args.lastIndex + 1;
        args.progress = prog;
        triggerEvents("upload", context, function (fn) { fn.call(s, file, args) });
        if (!args.cancel) { prog.proceed() }
    }
    function createProgress(file, context) {
        var s = this, ow = s.owner, args = { view: null };
        triggerEvents("createProgress", context, function (fn) { fn.call(s, file, args) });
        if (args.view == null) {
            //创建默认视图
            var p = ow.settings("progress");
            if (!p) {
                p = ow.settings("dragable") && (p = ow.settings("dragContainer")) && (p = p instanceof jQuery ? p : $(p)).length > 0 ? p : $("body")
            }
            //else if (p instanceof jQuery) { p = $(p); }
            var v = args.view = $(document.createElement("div")).appendTo(p);
            v.append("<h2>" + file.name + "</h2><p>size:" + u.SizeToString(file.size) + "</p><progress></progress>")
        }
        return s.view = args.view
    }
    function progress(file, context, st) {
        var s = this, p, args = { cancel: false, size: s.size, sizeString: u.SizeToString(s.size), view: s.view };
        if (s.sliced === true) {
            args.loaded = s.loaded + Math.min(st.currentBlobLoaded, s.blobSize)
        }
        else {
            args.loaded = Math.min(st.currentBlobLoaded, s.size)
        }
        p = (args.loaded / args.size) * 100;
        args.percent = p;
        args.loadedString = u.SizeToString(args.loaded);
        triggerEvents("progress", context, function (fn) { fn.call(s, file, args) });
        if (!args.cancel) {
            s.bar.attr("value", args.percent)
        }
    }
    function success(file, context, result) {
        var s = this
            , args = {
                view: s.view, cancel: false, result: result
                , req: s.xhr, responseText: s.xhr.responseText, responseType: s.xhr.responseType, responseXML: s.xhr.responseXML
            };
        triggerEvents("success", context, function (fn) { fn.call(s, file, args) });
        if (!args.cancel) { setTimeout(function () { s.view.remove(); }, 5000) }
    }
    function complete(file, context) {
        var s = this, args = { view: s.view, req: s.xhr, status: s.xhr.status };
        context.queue--;
        triggerEvents("complete", context, function (fn) { fn.call(s, file, args) });
        next(context);
    }
    function next(context) {
        var i = context.lastIndex, s;
        if (i < context.progress.length) {
            s = context.progress[i];
            upload.call(s.owner, s.file, context);
        }
    }
    function Progress(owner, file, context) {
        this.owner = owner;
        var s = this, xhr, paused, index = 0, count = 1
            , size = s.size = file.size
            , blobSize = s.blobSize = owner.settings("blobSize")
            , sliced = s.sliced = ((sliced = owner.settings("sliced")) === u.Sliced.Enabled || (sliced === u.Sliced.Auto && size > blobSize) ? true : false)
            , view = (s.view = createProgress.call(s, file, context) || $())
            , bar = (s.bar = view.find("progress").attr({ "max": 100, "value": 0 }))
            , limitSize = owner.settings("limitSize")
            , url = owner.settings("url")
            , parseResult = owner.settings("parseResult")
            , appendParams = function (d) {
                var ps = owner.settings("params");
                if (sliced) {
                    d.append("blobIndex", index);
                    d.append("blobCount", count);
                    d.append("resumableKey", s.resumableKey);
                    d.append("sliced", sliced);
                    d.append("fileName", file.name);
                }
                d.append("types", owner.settings("types") || "");
                if (owner.settings("accept")) { d.append("accept", owner.settings("accept")) }
                for (var p in ps) { d.append(p, ps[p]) }
            }
            , send = function (f) {

                var d = new FormData();
                d.append("file", f, file.name);
                appendParams(d);
                s.req= $.ajax({
                    type:"POST",
                    url: url,
                    contentType: false,
                    processData: false,
                    xhr: function () { return xhr; },
                    data: d
                });
            }
            , sendBlob = function (i) {
                var start = i * blobSize, end = start + blobSize;
                index = i;
                s.loaded = start;
                if (file.slice) {
                    s.blob = file.slice(start, end);
                }
                else if (file.webkitSlice) {
                    s.blob = file.webkitSlice(start, end);
                } else if (file.mozSlice) {
                    s.blob = file.mozSlice(start, end);
                }


                send(s.blob);
            };

        if (typeof (parseResult) !== "function") {
            parseResult = function (d) {
                try {
                    return JSON.parse(d);
                } catch (e) {
                    return  { err: true, msg: "脚本解析错误。" }
                } 
            }
        }
        s.file = file;
        s.loaded = 0;
        s.blobSize = blobSize = (sliced ? blobSize : size);
        s.pause = function () {
            if (!s.hasError && paused !== true) {
                s.paused = paused = true;
                if (xhr && file.uploading === true) {
                    xhr.abort();
                    delete file.uploading;
                }
                triggerEvents("pause", context, function (fn) { fn.call(s, file, { view: view }); });
                context.queue--;
                next(context);
            }
        };
        s.proceed = function () {
            if (s.hasError || file.uploading === true) { return; }
            if (file.uploaded === true || file.canceling === true) {
                error.call(s, file, context
                    , {
                        type: u.ErrorType.InvalidOperation
                        , message: file.canceling ? "the file is canceling" : file.uploaded ? "the file was uploaded" : "invalid operation", view: view
                    });
                return
            }
            if (paused === true) {
                var a = { view: view, cancel: false };
                triggerEvents("proceed", context, function (fn) { fn.call(s, file, a); });
                if (a.cancel) { return; }
            }
            file.cancel = s.paused = paused = false; file.uploading = true;
            if (!sliced) { send(file); return };
            var gotit = false
                , args = { sliced: true, fileType: file.type, fileName: file.name, fileSize: size, blobSize: blobSize, blobCount: count }
                , b = function (info) {
                    if (info && info.key)
                    { s.resumableKey = info.key }
                    else { throw new Error(info.msg || info.message || "failed to initialize"); }
                    setTimeout(function () {
                        sendBlob(!isNaN(info.index) ? info.index : 0)
                    }, 0)
                }
                , errfn = function (a, x, p) {
                    if (!a) { a = { type: u.ErrorType.ServerType, message: "failed to initialize" } }
                    a.view = view;
                    error.call(s, file, context, a); if (x && x.message) console.log(x.message, p)
                };
            triggerEvents("getResumableInfoHandler", context, function (fn) { try { fn.call(s, url, args, b) } catch (x) { errfn(null, x, fn) } gotit = true });
            if (!gotit) {
                $.ajax({
                    url: url,
                    type: "GET"
                    , data: args
                    , success: function (d) {
                        b(d)
                    }
                    , error: function (req, txt, dd) {
                        //获取初始化信息失败
                        errfn({ type: u.ErrorType.HttpType, code: req.status, message: txt || errorThrown })
                    }
                })
            }
        };
        s.cancel = function () {
            if (!s.hasError && file.uploaded !== true && file.cancel !== true && file.canceling !== true) {
                var doit = function (o) {
                    if (o && !(o.err || o.error)) {
                        file.cancel = true; delete file.canceling; bar.attr("value", 0);
                        triggerEvents("cancel", context, function (fn) { fn.call(s, file, { view: view }) })
                    }
                };
                s.pause();
                file.canceling = true;
                if (sliced) {
                    $.ajax({
                        url: url,
                        type: "DELETE",
                        data: { key: s.resumableKey },
                        success: function (d) {
                            doit(d);
                        }
                    })
                }
                else { doit(true); }
            }
            //
        };
        if (file.invalidType || (limitSize && !isNaN(limitSize) && file.size > limitSize)) {
            error.call(s, file, context
                , { type: file.invalidType ? u.ErrorType.InvalidType : u.ErrorType.UpperLimit, view: view, message: file.invalidType ? "无效文件类型" : "超过上传限制的大小" }
                );
            return
        }
        xhr = s.xhr = new XMLHttpRequest();
        if (typeof (owner.settings("timeout")) === "number") { xhr.timeout = owner.settings("timeout") }

        //事件绑定 
        xhr.addEventListener("readystatechange", function (e) {
            //完成
            if (xhr.readyState === 4) {
                if (s.blob) delete s.blob;
                var status = xhr.status, isSuccess = status >= 200 && status < 300 || status === 304;
                if (!isSuccess) {
                    if (paused !== true) { error.call(s, file, context, { type: u.ErrorType.HttpType, code: status, view: view, message: xhr.statusText }) }
                }
                if (sliced && index >= count - 1 || !sliced) {
                    if (paused === false) {
                        complete.call(s, file, context);
                        s.paused = paused = true;
                        delete file.uploading
                    }
                }
            }

        }, false);
        xhr.upload.addEventListener("progress", function (e) {
            progress.call(s, file, context, { currentBlobLoaded: e.loaded, currentBlobSize: e.total, index: index })
        }, false);
        xhr.addEventListener("load", function (e) {
            var r;
            if (s.hasError) return;
            if (!(r = parseResult(e.target.responseText)) || (r.err || r.error) === true) {
                error.call(s, file, context, { type: u.ErrorType.ServerType, view: view, message: r ? (r.msg || r.message) : "unknown" });
                return
            }
            if (sliced && index < count - 1) {
                if (paused === true) { delete file.uploading; }
                else { setTimeout(function () { sendBlob(index + 1) }, 0) }
            }
                ///完全上传成功
            else {
                success.call(s, file, context, r); file.uploaded = true
            }
        }, false);
        xhr.upload.addEventListener("error", function (e) {
            if (paused !== true) {
                var status = xhr.readyState === 4 || xhr.readyState === 3 ? xhr.status : 0;
                error.call(s, file, context, { type: u.ErrorType.HttpType, code: status, view: view, message: status === 0 ? "unknown" : xhr.statusText })
            }
        }, false);
        if (sliced === true) {
            s.count = count = Math.ceil(size / blobSize);
        };
        //s.proceed()
    }

    ///扩展jQuery方法，为其所有实例添加asyncUploadFiles方法
    if (u.Support) {
        jQuery.prototype.asyncUploadFiles = (function (b) {
            return function (a, e) {
                if (typeof (b) === "function") { b.apply(this, arguments) }
                if (a) {
                    if (a.placeholder) delete a.placeholder;
                    if (a.dragable) delete a.dragable;
                    if (a.multiple) delete a.multiple
                }
                var up = this.uploader = new u(a, e);
                jQuery.each(this, function (i, n) {
                    var fs = n.files;
                    if (fs && fs.length) { up.upload(fs) }
                });
            }
        })(jQuery.prototype.asyncUploadFiles)
    }
})();

