<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="HTML5Uploader.index" %>

<%@ Register Assembly="Html5Uploader" Namespace="Html5Uploader.Controls" TagPrefix="cc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <script src="Scripts/jquery-2.1.1.min.js"></script>
    <%--<script src="Scripts/Uploader.release.min.js"></script>--%>
    <%--<script src="Scripts/Uploader.min.js"></script>--%>
    <%--<script src="Scripts/Uploader.js"></script>--%>
<script>
    var prog;
    function errorhandler(file, args)
    {
        alert("args.message=" + args.message + ",invalidType=" + file.invalidType)
        //if (args.type == Uploader.ErrorType.UserAbort)
        //{
          
        //}
        args.cancel = true;
    }
    function createProgress(file, args) {
        var s = prog = this;
        prog.file = file;
        args.view = $(document.createElement("div")).appendTo("body").append("<h2>" + file.name + "</h2><p>size:" + Uploader.SizeToString(file.size) + " <span></span></p> <progress></progress><a href='javascript:' class='pause'>pause</a> <a href='javascript:' class='proceed'>proceed</a> <a href='javascript:' class='cancel'>cancel</a>");
        args.view.find("a").click(function () {
            var a = $(this);
            if (a.hasClass("pause")) {
                s.pause();
            }
            else if (a.hasClass("proceed"))
            { s.proceed();}
            else if (a.hasClass("cancel"))
            { s.cancel(); }
        });
    }
</script>
</head>
<body>
       
    <form id="form1" runat="server">
       
    
    
     <cc1:Html5UploaderClient ID="Html5Uploader1" RegisterScript="true" Url="Handler1.ashx" Dragable="true" BlobSize="500kb"
             MaxQueue="3" Placeholder="#btnSeletor" runat="server">
        <ViewTemplate>
            <a href="javascript:" id="btnSeletor" >select files</a>
        </ViewTemplate>
        <ClientEvents>
            <cc1:ClientEvent EventName="error" Handle="errorhandler" />
            <cc1:ClientEvent EventName="createProgress" Handle="createProgress" />
        <%--    <cc1:ClientEvent  EventName="validate" Handle="function(sender,arg){/* code ....*/}" />--%>
        </ClientEvents>
        </cc1:Html5UploaderClient>
    </form>
</body>
</html>
