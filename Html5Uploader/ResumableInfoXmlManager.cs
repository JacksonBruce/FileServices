using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.IO;
using System.Xml.Linq;
namespace Html5Uploader
{
    public class ResumableInfoXmlProvider : ResumableInfoProvider
    {
        static object lockObject = new object();
        public ResumableInfoXmlProvider() {}
        protected virtual string ElementName{
            get {
                return "resumable";
            }
        }
        protected virtual string GetStorePath()
        {
            string p = (WebConfigurationManager.AppSettings["Html5Uploader:StorePath"] ?? "").Trim().Replace('\\','/');
            if (0==p.Length)
            {
                p = "~/config/ResumableFilesStore.xml";
            }
            else {
                if (p.StartsWith("/"))
                {
                    p = "~" + p;
                }
                else if(!p.StartsWith("~/")) {
                    p = Path.Combine("~/", p);
                }
            }

            p = HttpContext.Current.Server.MapPath(p);
            return p;
        
        }
        protected virtual void UpdateElement(ResumableInfo info,XElement e)
        {
             
            e.Attr("key", info.Key);
            e.Attr("fileName",info.FileName);
            e.Attr("fileType",info.FileType);
            e.Attr("storePath",info.StorePath);
            e.Attr("fileSize",info.FileSize);
            e.Attr("blobSize",info.BlobSize);
            e.Attr("blobCount",info.BlobCount);
            e.Attr("blobIndex",info.BlobIndex);
            e.Attr("createDate", info.CreateDate);

        }
          
        IEnumerable<ResumableInfo> _resumableInfos;
        public override IEnumerable<ResumableInfo> ResumableInfos
        {
            get {
                if (_resumableInfos == null)
                { 
                    string key= "Html5Uploader.ResumableInfoXmlManager.ResumableInfos";
                    _resumableInfos = HttpRuntime.Cache[key] as IEnumerable<ResumableInfo>;
                    if (_resumableInfos == null)
                    {
                        XDocument doc;
                        string store=GetStorePath();
                        lock (lockObject)
                        {
                            if (File.Exists(store))
                            {
                                doc = XDocument.Load(store);
                            }
                            else
                            {
                                doc = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement(ElementName + "s"));
                                string dir = Path.GetDirectoryName(store);
                                if (!Directory.Exists(dir))
                                { Directory.CreateDirectory(dir); }
                                doc.Save(store);
                            }
                        }
                        _resumableInfos = (from n in doc.Root.Elements(ElementName)
                                           let item = new ResumableInfo(n.Attr("fileName")
                                               , n.Attr("fileType")
                                               , n.Attr("storePath")
                                               , n.Attr<long>("fileSize")
                                               , n.Attr<long>("blobSize")
                                               , n.Attr<long>("blobCount")
                                               , n.AttrStruct<Guid>("key")
                                               , n.Attr<long>("blobIndex")
                                               , n.Attr<DateTime>("createDate"))
                                           where item.Key != Guid.Empty
                                           select item).ToArray();
                        HttpRuntime.Cache.Insert(key, _resumableInfos
                            , new CacheDependency(store)
                            , Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
                    }
                }
                return _resumableInfos;
            }

        }
        public override ResumableInfo GetResumableInfo(Guid key)
        {
            foreach (var e in ResumableInfos)
            {
                if (e.Key == key)
                {
                    return e;
                }
            }
            return null;
        }
        public override ResumableInfo GetResumableInfo(string fileName, string fileType, long fileSize, long blobSize, long blobCount)
        {
            foreach (var e in ResumableInfos)
            {
                if (string.Equals(e.FileName, fileName, StringComparison.CurrentCultureIgnoreCase)
                        && e.FileSize == fileSize
                        && e.BlobSize == blobSize
                        && e.BlobCount == blobCount
                    //如果类型是空的，那么智能忽略，否则同时比较类型
                    && (string.IsNullOrWhiteSpace(e.FileType) || string.IsNullOrWhiteSpace(fileType) || string.Equals(e.FileType, fileType, StringComparison.CurrentCultureIgnoreCase))
                    )
                {
                    return e;
                }
            }
            return null;
        }
        public override void SaveResumableInfo(ResumableInfo info)
        {
            XDocument doc;
            XElement e =null;
            //启动线程保护，防止多个线程同时写入文件
            lock (lockObject)
            {
                string store = GetStorePath();
                if (File.Exists(store))
                {
                    doc = XDocument.Load(store);
                    var xe = from x in doc.Root.Elements(ElementName) select x;
                    foreach (var x in xe)
                    {
                        if (x.Attr<Guid>("key") == info.Key)
                        {
                            e = x;
                            break;
                        }
                    }
                    if (e == null)
                    {
                        e = new XElement(ElementName);
                        doc.Root.Add(e);
                    }
                    UpdateElement(info, e);
                }
                else
                {
                    e = new XElement(ElementName);
                    UpdateElement(info, e);
                    doc = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement(ElementName + "s", e));
                    string dir = Path.GetDirectoryName(store);
                    if (!Directory.Exists(dir))
                    { Directory.CreateDirectory(dir); }
                }
                doc.Save(store);
            }
        }
        public override void DeleteResumableInfo(Guid key)
        {
            string store = GetStorePath();
            //启动线程保护，防止多个线程同时写入文件
            lock (lockObject)
            {
                if (File.Exists(store))
                {
                    XDocument doc = XDocument.Load(store);
                    var xe = (from x in doc.Root.Elements(ElementName) select x).ToArray();
                    foreach (var x in xe)
                    {
                        if (x.Attr<Guid>("key") == key)
                        {
                            x.Remove();
                            break;
                        }
                    }
                    doc.Save(store);
                }
            }
        }

        public override void Dispose()
        {
        }
    }
}
