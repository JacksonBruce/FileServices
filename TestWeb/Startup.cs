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
                opt.DefaultScheme = "Document";//Ĭ�����ĵ�������
                opt.AddAuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme);//�����֤��������
                //�ļ�����ʱ���ļ�����������
                opt.RuleOptions = new Ufangx.FileServices.Models.FileNameRuleOptions()
                {
                    Rule = Ufangx.FileServices.Models.FileNameRule.Custom,//�Զ����������򣬱����ṩ�Զ��巽��
                    Custom = originFileName => string.Format("{0:yyyyMMddHHmmss}_xx_{1}", DateTime.Now, originFileName),
                    Format = "xxx_{0:yyyyMMddHHmmss}"//������ú�Rule=FileNameRule.Dateһ��ʹ�ã�Ĭ���ǣ�{0:yyyyMMddHHmmss}
                };

            })
                //��Ƭ������
                .AddScheme("pictures", opt =>
                {
                    opt.StoreDirectory = "wwwroot/pictures";//ͼƬ�洢��Ŀ¼
                    opt.SupportExtensions = new string[] { ".jpg", ".png" };//֧�ֵ���չ��
                    opt.HandlerType = null;//�ϴ��ɹ�����ļ��������ͣ������ͱ���ʵ��IFileHandler�ӿ�
                    opt.LimitedSize = 1024 * 1024 * 4;//�ļ���С���������ֵ���ֽ�Ϊ��λ
                })
                .AddScheme("Document", opt => opt.StoreDirectory = "wwwroot/documents")//�ĵ�������
                //.AddScheme<VideoService>(name:"videos",storeDirectory:"",supportExtensions:new string[] { },LimitedSize:1024*1024*500)//��Ƶ������
                .AddLocalServices(o => o.StorageRootDir = hostEnvironment.ContentRootPath);//�����ļ�ϵͳ���ļ����ڱ���
                //��ţ�洢����
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
