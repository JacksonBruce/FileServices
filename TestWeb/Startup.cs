using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestWeb
{
    public class Startup
    {
        private readonly IHostEnvironment hostEnvironment;

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            this.hostEnvironment = hostEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddAuthentication().AddCookie();
            services.AddHttpContextAccessor();
            services.AddFileServices(opt =>
            {
                opt.DefaultScheme = "Document";//默认是文档处理方案
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
                .AddScheme("pictures", opt =>
                {
                    opt.StoreDirectory = "wwwroot/pictures";//图片存储的目录
                    opt.SupportExtensions = new string[] { ".jpg", ".png" };//支持的扩展名
                    opt.HandlerType = null;//上传成功后的文件处理类型，该类型必须实现IFileHandler接口
                    opt.LimitedSize = 1024 * 1024 * 4;//文件大小的最大限制值，字节为单位
                })
                .AddScheme("Document", opt => opt.StoreDirectory = "wwwroot/documents")//文档处理方案
                //.AddScheme<VideoService>(name:"videos",storeDirectory:"",supportExtensions:new string[] { },LimitedSize:1024*1024*500)//视频处理方案
                .AddLocalServices(o => o.StorageRootDir = hostEnvironment.ContentRootPath);//本地文件系统，文件存在本地
                //七牛存储服务
                //.AddQiniuFileService(opt => {
                //    opt.AccessKey = "";//
                //    opt.SecretKey = "";
                //    opt.BasePath = "";
                //    opt.Bucket = "";
                //    opt.Domain = "";
                //    opt.ChunkUnit = Qiniu.Storage.ChunkUnit.U1024K;
                //    opt.Zone = "ZoneCnEast";
                
                //});
            services.AddCors(options =>
            {
                options.AddPolicy("cors", builder =>
                {
                    builder.AllowCredentials().AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:9000");
                });
            });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("cors");
            app.UseRouting();

            app.UseAuthorization();
            //app.Use(next => async ctx =>
            //{
            //    try
            //    {
            //        await next(ctx);
            //    }
            //    catch (Exception ex) {
            //        throw ex;
            //    }
            //});
            app.UseFileServices();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
