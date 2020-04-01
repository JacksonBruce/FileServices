# FileServices
文件管理，超大文件上传，服务端支持断点续传，客户端h5上传器，支持多文件同时上传，支持断点续传

使用方法
-------

1. 注册服务
```C#
        public void ConfigureServices(IServiceCollection services)
        {
                        services.AddHttpContextAccessor();
            services.AddFileServices(opt => {
                opt.DefaultScheme = "documents";//默认是文档处理方案
                opt.AddAuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme);//身份认证方案名称
                //文件保存时新文件的命名规则
                opt.RuleOptions = new Ufangx.FileServices.Models.FileNameRuleOptions()
                {
                    Rule = Ufangx.FileServices.Models.FileNameRule.Custom,//自定义命名规则，必须提供自定义方法
                    Custom = originFileName => string.Format("{0:yyyyMMddHHmmss}_xx_{1}", DateTime.Now, originFileName),
                    Format = "xxx_{0:yyyyMMddHHmmss}"//这个配置和Rule=FileNameRule.Date一起使用，默认是：{0:yyyyMMddHHmmss}
                };

            })
                //照片处理方案
                .AddScheme("pictures", opt => {
                    opt.StoreDirectory = "wwwroot/pictures";//图片存储的目录，使用本地文件系统时一般不放这里
                    opt.SupportExtensions = new string[] { ".jpg", ".png" };//支持的扩展名
                    opt.HandlerType = null;//上传成功后的文件处理类型，该类型必须实现IFileHandler接口
                    opt.LimitedSize = 1024 * 1024 * 4;//文件大小的最大限制值，字节为单位
                })
                .AddScheme("documents",opt => opt.StoreDirectory = "wwwroot/documents")//文档处理方案
                .AddScheme<VideoService>(name:"videos",
                storeDirectory:"wwwroot/videos",//视频存储目录，使用本地文件系统时一般不放这里
                supportExtensions: new string[] { ".avi", ".wmv", ".mpg", ".mpeg", ".mov", ".rm", ".swf", ".flv", ".mp4" }
                LimitedSize:1024*1024*500)//视频处理方案
                //.AddLocalServices(o => o.StorageRootDir = hostEnvironment.ContentRootPath)//本地文件系统，文件存在本地
                //七牛存储服务
                .AddQiniuFileService(opt => {
                    opt.AccessKey = "";//
                    opt.SecretKey = "";
                    opt.BasePath = "";
                    opt.Bucket = "";
                    opt.Domain = "";
                    opt.ChunkUnit = Qiniu.Storage.ChunkUnit.U1024K;
                    opt.Zone = "ZoneCnEast";
                
                });
        }
        
```
2.使用中间件

```C#
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
              //。。。。其他代码
              
                 app.UseFileServices("/FileServices");
                 //上传文件的url是：/FileServices/uploader
                 //创建续传信息是：
                 //GET /FileServices/uploader?fileSize=125829120&blobSize=4194304&blobCount=30&fileName=xxx.zip&fileType=xxxx
                 //request headers
                 //scheme:documents
                 //response
                 //{"index":0,"key":"续传信息key"}
                 //获取续传信息是：
                  //GET /FileServices/uploader?key=续传信息key
                 //request headers
                 //scheme:documents
                 //response
                 //{"index":12,"key":"续传信息key"}
                 //删除续传信息是：
                 //DELETE /FileServices/uploader?key=续传信息key
                 //response
                 //true
                 //上传切片数据块
                 //POST  /FileServices/uploader
                 //Form
                 //blobIndex:12 当前切片的索引
                 //key:续传信息key
                 //Form.Files==1 必须有一个表单文件，表示当前切片数据
                 //request headers
                 //scheme:documents
                 //上传文件
                 //POST  /FileServices/uploader
                 //Form.Files>0 必须包含有一个文件以上
                 //request headers
                 //scheme:documents
                         
              //。。。。其他代码
        
        }

```

