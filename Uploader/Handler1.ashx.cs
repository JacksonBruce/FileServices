using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace HTML5Uploader
{
    /// <summary>
    /// Handler1 的摘要说明
    /// </summary>
    public class Handler1 : Html5Uploader.FilesUploadHandler
    {
        //object LockObject = new object();
        //static List<byte[]> Data;

        //public void ProcessRequest(HttpContext context)
        //{
        //    ////bool sliced;
        //    ////bool.TryParse(context.Request.Form["sliced"], out sliced);
        //    ////var files = context.Request.Files;
        //    ////if (files != null && files.Count > 0)
        //    ////{
        //    ////    var file = files[0];
        //    ////    string name =sliced? Path.GetFileName(file.FileName):context.Request.Form["name"], tempFilePath = context.Server.MapPath("~/" + name);
        //    ////    if (sliced)
        //    ////    {
        //    ////        byte[] buffer = new byte[file.InputStream.Length];
        //    ////        file.InputStream.Read(buffer, 0, buffer.Length);
        //    ////        using (BinaryWriter bw = new BinaryWriter(new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
        //    ////        {
        //    ////            bw.Write(buffer, 0, buffer.Length);
        //    ////            bw.Flush();
        //    ////        }
        //    ////    }
        //    ////    else { 

        //    ////    }
        //    ////}
        //    context.Response.Write("{index:100,key:'" + Guid.NewGuid() + "'}");
        //}
        ////void v2()
        ////{

        ////    //var files = context.Request.Files;
        ////    //if (files != null && files.Count > 0)
        ////    //{
        ////    //    long count;
        ////    //    int index;
        ////    //    long.TryParse(context.Request.Form["count"], out count);
        ////    //    int.TryParse(context.Request.Form["index"], out index);
        ////    //    lock (LockObject)
        ////    //    {
        ////    //        if (Data == null)
        ////    //        {
        ////    //            Data = new List<byte[]>(new byte[count][]);
        ////    //        }
        ////    //    }
        ////    //    var file = files[0];
        ////    //    byte[] buffer = new byte[file.InputStream.Length];
        ////    //    file.InputStream.Read(buffer, 0, buffer.Length);
        ////    //    lock (Data)
        ////    //    {
        ////    //        Data[index] = buffer;
        ////    //    }
        ////    //    if (index >= count - 1)
        ////    //    {
        ////    //        string name = context.Request.Form["name"], tempFilePath = context.Server.MapPath("~/" + name);
        ////    //        lock (LockObject)
        ////    //        {
        ////    //            var arr = Data.ToArray();
        ////    //            for (int i = 0; i < arr.Length; i++)
        ////    //            {
        ////    //                var d = arr[i];
        ////    //                using (BinaryWriter bw = new BinaryWriter(new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
        ////    //                {
        ////    //                    bw.Write(d, 0, d.Length);
        ////    //                    bw.Flush();
        ////    //                }
        ////    //            }
        ////    //            Data = null;
        ////    //        }
        ////    //    }

        ////    //}
        ////}
        ////void temp(HttpContext context)
        ////{
        ////    string name = context.Request.Form["name"], tempFilePath = context.Server.MapPath("~/" + name);
        ////    var files = context.Request.Files;
        ////    long loaded, total, position;
        ////    long.TryParse(context.Request.Form["loaded"], out loaded);
        ////    long.TryParse(context.Request.Form["position"], out position);
        ////    if (files != null && files.Count > 0)
        ////    {
        ////        long.TryParse(context.Request.Form["total"], out total);
        ////        FileResumer fr = new FileResumer();
        ////        fr.m_FileSize = total;
        ////        fr.RangePos = position;
        ////        var file = files[0];
        ////        loaded += file.InputStream.Length;

        ////        fr.Resumer(ref file, tempFilePath);
        ////        //using (BinaryWriter bw = new BinaryWriter(new FileStream(tempFilePath, FileMode.Append, FileAccess.Write,FileShare.ReadWrite)))
        ////        //{
        ////        //    using (BinaryReader br = new BinaryReader(files[0].InputStream))
        ////        //    {
        ////        //        var arr = new byte[br.BaseStream.Length];
        ////        //        br.Read(arr, 0, arr.Length);
        ////        //        bw.Write(arr, 0,arr.Length);
        ////        //        //var arr = new byte[1024];
        ////        //        //while (br.Read(arr, 0, arr.Length) > 0)
        ////        //        //{
        ////        //        //    bw.Write(arr, 0, arr.Length);
        ////        //        //}
        ////        //    } 
        ////        //    bw.Flush();
        ////        //}
        ////    }
        ////    context.Response.Write("{loaded:" + loaded + "}");
        ////    //context.Response.Write(Convert.ToBase64String(Encoding.UTF8.GetBytes("kkkkkkkk")));
        ////    //context.Response.Write(Encoding.UTF8.GetString(Convert.FromBase64String("a2tra2tra2s=")));
        ////}
        ////async void Write(byte[] data, string tempFilePath)
        ////{
        ////    long sPosstion;
        ////    FileStream FStream;
        ////    if (File.Exists(tempFilePath))
        ////    {
        ////        FStream = File.OpenWrite(tempFilePath);
        ////        sPosstion = FStream.Length;
        ////        FStream.Seek(sPosstion, SeekOrigin.Current);//移动文件流中的当前指针
        ////    }
        ////    else
        ////    {
        ////        FStream = new FileStream(tempFilePath, FileMode.Create);
        ////        sPosstion = 0;
        ////    }
        ////    await FStream.WriteAsync(data, 0, data.Length);
        ////    FStream.Close();
        ////    FStream.Dispose();
        ////}

    }





    /// <summary>
    /// 文件续传类
    /// </summary>
    public class FileResumer
    {
        public long m_FileSize;		//文件总大小。
        string m_FileMD5;	//
        long m_RangePos;		//文件块起始位置
        public long RangePos
        {
            set { this.m_RangePos = value; }
        }
        int m_RangeSize;	//文件块大小

        //文件读写锁，防止多个用户同时上传相同文件时，出现创建文件的错误
        static ReaderWriterLock m_writeLock = new ReaderWriterLock();

        public FileResumer()
        {
        }

        /// <summary>
        /// 根据文件大小创建文件。
        /// 注意：多个用户同时上传相同文件时，可能会同时创建相同文件。
        /// </summary>
        public void CreateFile(string filePath)
        {
            //文件不存在则创建
            if (!File.Exists(filePath))
            {
                //创建文件
                //这里存在多个线程同时创建文件的问题。
                FileStream fs = File.OpenWrite(filePath);
                BinaryWriter w = new BinaryWriter(fs);
                for (long i = 0; i < this.m_FileSize; ++i)
                {
                    w.Write((byte)0);
                }
                w.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// 续传文件
        /// </summary>
        /// <param name="fileRange">文件块</param>
        /// <param name="fileRemote">远程文件完整路径。d:\www\web\upload\201204\10\md5.exe</param>
        public void Resumer(ref HttpPostedFile fileRange, string fileRemote)
        {
            //存在多个用户同时创建相同文件的问题。
            m_writeLock.AcquireWriterLock(1000);
            this.CreateFile(fileRemote);
            m_writeLock.ReleaseWriterLock();

            //上传的文件大小不为空
            if (fileRange.InputStream.Length > 0)
            {
                //文件已存在，写入数据
                //可能会有多个线程同时写文件。
                FileStream fs = new FileStream(fileRemote, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                fs.Seek(this.m_RangePos, SeekOrigin.Begin);
                byte[] ByteArray = new byte[fileRange.InputStream.Length];
                fileRange.InputStream.Read(ByteArray, 0, (int)fileRange.InputStream.Length);
                fs.Write(ByteArray, 0, (int)fileRange.InputStream.Length);
                fs.Flush();
                fs.Close();
            }

        }
    }


}