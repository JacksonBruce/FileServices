using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Helper;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;

namespace Html5Uploader
{
    public class InvalidDataBlobException : Exception
    {
        public InvalidDataBlobException() { }
        public InvalidDataBlobException(string message):base(message) { }
        public InvalidDataBlobException(string message, Exception innerException) : base(message, innerException) { }
        public InvalidDataBlobException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    public class ValidateArguments {
        public ValidateArguments(HttpPostedFile file, string fileName, string[] accepts)
        {
            this.File = file;
            this.AcceptsTypes = accepts;
            this.Validation = true;
            this.FileName = fileName;
        }
        public HttpPostedFile File { get; private set; }
        public string FileName { get; private set; }
        public string[] AcceptsTypes { get; private set; }
        public bool Validation { get; set; }
    }
    public class CompleteArguments {
        public CompleteArguments(IEnumerable<FileUploadComplete> data)
        {
            FileUploadCompletes = data;
        }
        public bool Cancel { get; set; }
        public IEnumerable<FileUploadComplete> FileUploadCompletes { get;private set; }
    }
    public class FilesUploadHandler : IHttpHandler,IDisposable,IRequiresSessionState
    {
        public event Action<FilesUploadHandler, Exception> Error;
        public event Action<FilesUploadHandler, CompleteArguments> Complete;
        public event Action<FilesUploadHandler, ValidateArguments> Validate;
        #region 可续传处理块
        ResumableInfoProvider _provider;
        protected virtual ResumableInfoProvider Provider {
            get {
                if (_provider == null)
                {
                    _provider = new ResumableInfoXmlProvider();
                }
                return _provider;
            }
        }
        bool? _sliced;
        protected bool Sliced { 
            get {
                if (!_sliced.HasValue)
                {
                    bool t;
                    _sliced = bool.TryParse(Form["sliced"] ?? Request.QueryString["sliced"], out t) && t;
                }
                return _sliced.Value;
            }
        }
        long? _blobIndex;
        protected long BlobIndex
        {
            get
            {
                if (!_blobIndex.HasValue)
                {
                    long t;
                    _blobIndex = long.TryParse(Form["blobIndex"] ?? Request.QueryString["blobIndex"], out t) ? t : 0;
                }
                return _blobIndex.Value;
            }
        }
        long? _blobCount;
        protected long BlobCount
        {
            get
            {
                if (!_blobCount.HasValue)
                {
                    long t;
                    _blobCount = long.TryParse(Form["blobCount"] ?? Request.QueryString["blobCount"], out t) ? t : 1;
                }
                return _blobCount.Value;
            }
        }
        long? _blobSize;
        protected long BlobSize
        {
            get
            {
                if (!_blobSize.HasValue)
                {
                    long t;
                    _blobSize = long.TryParse(Form["blobSize"] ?? Request.QueryString["blobSize"], out t) ? t : 0;
                }
                return _blobSize.Value;
            }
        }
        long? _fileSize;
        protected long FileSize
        {
            get
            {
                if (!_fileSize.HasValue)
                {
                    long t;
                    if (Sliced)
                    {
                        _fileSize = long.TryParse(Form["fileSize"] ?? Request.QueryString["fileSize"], out t) ? t : 0;
                    }
                    else {
                        _fileSize = HasFiles ? Files[0].ContentLength : 0;
                    }
                }
                return _fileSize.Value;
            }
        }
        Guid? _key;
        protected Guid ResumableKey
        {
            get
            {
                if (!_key.HasValue)
                {
                    Guid t;
                    _key = Guid.TryParse(Form["resumableKey"] ?? Request.QueryString["resumableKey"], out t) ? t : Guid.Empty;
                }
                return _key.Value;
            }
        }
        string _fileName;
        protected string FileName {
            get
            {
                if (_fileName == null) {
                    _fileName = Sliced ? (Form["fileName"] ?? Request.QueryString["fileName"]) : HasFiles ? Files[0].FileName : string.Empty;
                }
                return _fileName;
            }
        }
        string _fileType;
        protected string FileType
        {
            get
            {
                if (_fileType == null)
                {
                    _fileType = Sliced ? (Form["fileType"] ?? Request.QueryString["fileType"]) : (HasFiles ? Files[0].ContentType : string.Empty);
                   
                }
                return _fileType;
            }
        }
        ResumableInfo _resumableInfo;
        protected virtual ResumableInfo ResumableInfo
        {
            get {
                if (_resumableInfo == null)
                {
                    _resumableInfo = ResumableKey == Guid.Empty ? Provider.GetResumableInfo(FileName, FileType, FileSize, BlobSize, BlobCount) : Provider.GetResumableInfo(ResumableKey);
                    if (_resumableInfo == null && Guid.Empty != ResumableKey)
                    {
                        throw new ArgumentException("无效的键值");
                    }
                    ///如果已经上传了部分文件，且部件文件已丢失，那么重新再传。
                    if (_resumableInfo != null && !string.IsNullOrWhiteSpace(_resumableInfo.StorePath) && !File.Exists(Context.Server.MapPath("~" + _resumableInfo.StorePath)))
                    {
                        Provider.DeleteResumableInfo(_resumableInfo.Key);
                        _resumableInfo = null;
                    }

                    if (_resumableInfo == null)
                    {
                        _resumableInfo = new ResumableInfo(FileName, FileType,null, FileSize, BlobSize, BlobCount);
                    }
                }
                return _resumableInfo;
            }
        }
        protected virtual void SaveResumableInfo(ResumableInfo info)
        {
            Provider.SaveResumableInfo(info);
        }
        protected virtual void RemoveResumableInfo(ResumableInfo info)
        {
            Provider.DeleteResumableInfo(info.Key);
        }
        #endregion

        #region 属性
        string[] _types;
        string[] AcceptsTypes
        {
            get
            {
                if (_types == null)
                {
                    _types = string.IsNullOrWhiteSpace(Form["types"]) ? new string[0] : Form["types"].Trim().Split(';');
                }
                return _types;
            }
        }

        string _token;
        protected string Token {

            get {
                if (_token == null)
                {
                    _token = Form["token"] ?? string.Empty;
                }
                return _token;
            
            }
        }

        HttpContext _context;
        protected HttpContext Context { get { return _context; } }
        protected HttpRequest Request { get { return _context != null ? _context.Request : null; } }
        protected NameValueCollection Form { get { return Request == null ? null : Request.Form; } }
        protected HttpResponse Response { get { return _context != null ? _context.Response : null; } }
        protected IPrincipal User { get { return _context == null ? null : _context.User; } }
        protected HttpFileCollection Files { get { return Request != null ? Request.Files : null; } }
        protected bool HasFiles { get { return Files != null && Files.Count > 0; } }
        protected bool IsAuthenticated { get { return User != null && User.Identity != null && User.Identity.IsAuthenticated; } }
        #endregion
     
        #region IHttpHandler 成员
        public bool IsReusable
        {
            get { return false; }
        }
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                _context = context;
                Init();
                ////////验证用户的合法身份
                //if (!IsAuthenticated)
                //{ throw new HttpException(503, null); }
                string method = Request.QueryString["method"] ?? Form["method"];
                switch (method)
                {
                    case "getResumableInfo":
                        Response.Clear();
                        Response.Write(string.Format("{{key:\"{0}\",index:{1}}}", ResumableInfo.Key, ResumableInfo.BlobIndex));
                        SaveResumableInfo(ResumableInfo);
                        return;
                    case "deleteResumable":
                        Response.Clear();
                        DeleteResumable();
                        Response.Write("true");
                        return;

                }
               
               
                var outputs=Save();
                if (Sliced)
                {
                    if (BlobIndex >= BlobCount - 1)
                    {
                        RemoveResumableInfo(ResumableInfo);
                    }
                    else
                    {
                        SaveResumableInfo(ResumableInfo);
                    }
                }
                bool completed = false;
                if (Complete != null && (!Sliced||(Sliced && BlobIndex >= BlobCount - 1))) 
                {
                    CompleteArguments arg = new CompleteArguments(outputs);
                    Complete(this, arg);
                    completed = arg.Cancel;
                }
                if (!completed)
                {
                    if (outputs == null)
                    {
                        Response.Write("null");
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var fuc in outputs)
                        {
                            sb.Append((sb.Length > 0 ? "," : "") + GetJSON(fuc));
                        }
                        if (outputs.Count() > 1)
                        {
                            sb.Insert(0, "[");
                            sb.Append("]");
                        }
                        Response.Write(sb);
                    }
                }
            }
            catch (Exception exp) {
                if (exp is InvalidDataBlobException || exp.InnerException is InvalidDataBlobException)
                { DeleteResumable(); }
                if (Error != null)
                {
                    Error(this, exp);
                }
                else {
                    Response.Clear();
                    Response.Write("{\"err\":true,\"msg\":\"" + ConvertHelper.ConvertToClientString(exp.Message) + "\"}");
                }
            }
            finally
            {
                Dispose();
            }
        }
        #endregion
        string _storeDirectory;
        protected virtual string StoreDirectory
        {
            get
            {
                if (_storeDirectory == null)
                {
                    string p = (WebConfigurationManager.AppSettings["Html5Uploader:FilesStoreDirectory"] ?? "").Trim().Replace('\\', '/');
                    if (0 == p.Length)
                    {
                        p = "/files";
                    }
                    else
                    {
                        if (p.StartsWith("~/"))
                        {
                            p = p.TrimStart('~');
                        }
                        else if (!p.StartsWith("/"))
                        {
                            p = "/" + p;
                        } 
                    }
                    _storeDirectory = p;
                }
                return _storeDirectory;
            }
        }
        public virtual void Dispose()
        {
            if (_provider != null)
            { _provider.Dispose(); }
        }
        protected virtual string CreateStorePath(string directory)
        {
            string AbsoluteUploadDirectory = Context.Server.MapPath("~" + directory)
                    , n = Path.GetFileNameWithoutExtension(FileName)
                    , ext = Path.GetExtension(FileName)
                    , p = Path.Combine(AbsoluteUploadDirectory, n + ext);
            int i = 0;
            while (File.Exists(p))
            {
                p = Path.Combine(AbsoluteUploadDirectory, n + "(" + (++i) + ")" + ext);
            }
            return Path.Combine(directory, Path.GetFileName(p)).Replace("\\", "/");
        }
        protected virtual string CreateStorePath()
        {
            return CreateStorePath(StoreDirectory);
        }
        protected virtual void Init() {}
        protected virtual string GetJSON(FileUploadComplete fuc)
        {
            bool err = false;
            if (fuc.Error != null)
            {
                err = true;
                if (string.IsNullOrWhiteSpace(fuc.Message)) { fuc.Message = fuc.Error.Message; }
            }
            return string.Format("{{{0}\"msg\":\"{1}\",\"filePath\":\"{2}\",\"name\":\"{3}\"{4}}}"
                , err ? "\"err\":true," : null
                , err ? fuc.Message : string.IsNullOrWhiteSpace(fuc.Message) ? "文件保存成功" : ConvertHelper.ConvertToClientString(fuc.Message)
                , ConvertHelper.ConvertToClientString(fuc.FilePath), ConvertHelper.ConvertToClientString(fuc.FileName)
                , fuc.Sliced ? string.Format(",\"sliced\":true,\"blobIndex\":{0},\"blobCount\":{1}", fuc.BlobIndex, fuc.BlobCount) : null);
        }
        protected bool IsValidation(HttpPostedFile file)
        {
            if (Sliced)
            {
                if (file != null && file.InputStream.Length > ResumableInfo.BlobSize)
                {
                    throw new InvalidDataBlobException("无效的数据包。");
                }
                if (!string.IsNullOrWhiteSpace(ResumableInfo.StorePath))
                {
                    string physicalPath = Context.Server.MapPath("~" + ResumableInfo.StorePath);
                    if (File.Exists(physicalPath) && new FileInfo(physicalPath).Length + file.InputStream.Length > ResumableInfo.FileSize)
                    {
                        throw new InvalidDataBlobException("无效的数据包。");
                    }
                }
            }



            bool validation = false;
            string name = Sliced ? FileName : file.FileName;
            if (AcceptsTypes == null || AcceptsTypes.Length == 0) { validation = true; }
            else
            {
                foreach (var accept in AcceptsTypes)
                {
                    if (string.IsNullOrWhiteSpace(accept)) continue;
                    if (Regex.IsMatch(name, Regex.Escape(accept.Trim()) + "$", RegexOptions.IgnoreCase))
                    {
                        validation = true;
                        break;
                    }
                }
            }
            if (Validate != null)
            {
                string fileName=Sliced?FileName:Path.GetFileName(file.FileName);
                ValidateArguments args = new ValidateArguments(file, fileName, AcceptsTypes);
                args.Validation = validation;
                Validate(this, args);
                validation = args.Validation;
            }
            return validation;
        }
        protected virtual IEnumerable<FileUploadComplete> Save() {
            FileUploadComplete fuc = null;
            if (HasFiles)
            {
                if (Sliced)
                {
                   
                    try
                    {
                        for (int i = 0; i < Files.Count; i++)
                        {
                            HttpPostedFile f = Files[i];
                            fuc = SaveFile(f);
                        }
                    }
                    catch (Exception exp) { fuc = new FileUploadComplete(this.FileName,FileSize,true, ResumableInfo.BlobIndex, ResumableInfo.BlobCount, null, exp); }
                    return new FileUploadComplete[] { fuc };
                }
                else
                {
                    List<FileUploadComplete> list = new List<FileUploadComplete>();
                    for (int i = 0; i < Files.Count; i++)
                    {
                        HttpPostedFile f = Files[i];
                        try
                        {
                            fuc = SaveFile(f);
                        }
                        catch (Exception exp) {
                            fuc = new FileUploadComplete(f.FileName,f.ContentLength,null, exp);
                        }
                        list.Add(fuc);
                       
                    }
                    return list;
                }
            }
            return null;
        }
        protected virtual FileUploadComplete SaveFile(HttpPostedFile file)
        {
            if (file == null || file.ContentLength <= 0)
            {
                throw new ArgumentException("文件为空的。");
            }
            if (!IsValidation(file))
            { throw new ArgumentException("无效的文件类型。"); }

            string storepath = Sliced ? (string.IsNullOrWhiteSpace(ResumableInfo.StorePath) ? (ResumableInfo.StorePath = CreateStorePath()) : ResumableInfo.StorePath)
                : CreateStorePath()
                , physicalPath = Context.Server.MapPath("~" + storepath),dir = Path.GetDirectoryName(physicalPath);
            if (!Directory.Exists(dir)){Directory.CreateDirectory(dir);}
            if (Sliced)
            {
                
                using (BinaryWriter bw = new BinaryWriter(new FileStream(physicalPath, FileMode.Append, FileAccess.Write, FileShare.Read)))
                {
                    byte[] buffer = new byte[1024];
                    while (0 < file.InputStream.Read(buffer, 0, buffer.Length))
                    {
                        bw.Write(buffer, 0, buffer.Length);
                    }
                    bw.Flush();
                }
                ResumableInfo.BlobIndex = ResumableInfo.BlobIndex + 1;
            }
            else
            {
                file.SaveAs(physicalPath);
            }
            return Sliced ? new FileUploadComplete(storepath, FileSize, Sliced, ResumableInfo.BlobIndex, ResumableInfo.BlobCount)
                : new FileUploadComplete(storepath, file.ContentLength, null);
        }
        protected virtual void DeleteResumable(Guid? key = null)
        {
            var k = key ?? ResumableKey;
            var info = Provider.GetResumableInfo(k);
            if (info != null)
            {
                if (!string.IsNullOrWhiteSpace(info.StorePath))
                {
                    string physicalPath = Context.Server.MapPath("~" + info.StorePath);
                    if (File.Exists(physicalPath)) { File.Delete(physicalPath); }
                }
                RemoveResumableInfo(info);
            }
        }
    }
}
