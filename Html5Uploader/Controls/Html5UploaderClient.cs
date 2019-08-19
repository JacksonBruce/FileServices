using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Helper;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Html5Uploader.Controls
{
    [ParseChildren(true)]
    [PersistChildren(false)]
    [ToolboxData("<{0}:Html5UploaderClient runat=server></{0}:Html5UploaderClient>")]
    public class Html5UploaderClient : Control, INamingContainer
    {
        Dictionary<SettingsNames, object> settings;
        public Html5UploaderClient()
        {
            settings = new Dictionary<SettingsNames, object>();
            RegisterScript = true;
        }
        private ITemplate _viewTemplate;
        [DefaultValue("视图模板")]
        [TemplateContainer(typeof(ViewTemplate))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Browsable(false)]
        public ITemplate ViewTemplate
        {
            get { return _viewTemplate; }
            set { _viewTemplate = value; }
        }
        [Description("客户端事件")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ClientEventsCollection ClientEvents
        { get; set; }

        [Description("随文件一起提交的参数列表")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public PostParametersCollection PostParameters
        { get; set; }

        [Description("服务端处理程序")]
        [UrlProperty]
        public string Url
        {
            get
            {
                return settings.ContainsKey(SettingsNames.url) ? settings[SettingsNames.url] as string : null;
            }
            set
            {
                settings[SettingsNames.url] = value;
            }
        }
        string _token;
        public string Token {
            get { return _token; }
            set {
                if (value == null || value.Length <= 50)
                { _token = value; }
                else { throw new Exception("Token的长度不能超过50个字符。"); }
            }
        }
        public string Placeholder
        {
            get
            {
                return settings.ContainsKey(SettingsNames.placeholder) ? settings[SettingsNames.placeholder] as string : null;
            }
            set
            {
                settings[SettingsNames.placeholder] = value;
            }
        }
        public bool Multiple
        {
            get
            {
                return settings.ContainsKey(SettingsNames.multiple) ? (bool)settings[SettingsNames.multiple] : true;
            }
            set
            {
                settings[SettingsNames.multiple] = value;
            }
        }
        public string Accept
        {
            get
            {
                return settings.ContainsKey(SettingsNames.accept) ? settings[SettingsNames.accept] as string : null;
            }
            set
            {
                settings[SettingsNames.accept] = value;
            }
        }
        public string Types
        {
            get
            {
                return settings.ContainsKey(SettingsNames.types) ? settings[SettingsNames.types] as string : null;
            }
            set
            {
                settings[SettingsNames.types] = value;
            }
        }
        public string Timeout
        {
            get
            {
                return settings.ContainsKey(SettingsNames.timeout) ? settings[SettingsNames.timeout] as string : null;
            }
            set
            {
                settings[SettingsNames.timeout] = value;
            }
        }
        public int MaxQueue
        {
            get
            {
                return settings.ContainsKey(SettingsNames.maxQueue) ? (int)settings[SettingsNames.maxQueue] : 2;
            }
            set
            {
                settings[SettingsNames.maxQueue] = value;
            }
        }
        public bool Dragable
        {
            get
            {
                return settings.ContainsKey(SettingsNames.dragable) ? (bool)settings[SettingsNames.dragable] : true;
            }
            set
            {
                settings[SettingsNames.dragable] = value;
            }
        }
        public string DragContainer
        {
            get
            {
                return settings.ContainsKey(SettingsNames.dragContainer) ? settings[SettingsNames.dragContainer] as string : null;
            }
            set
            {
                settings[SettingsNames.dragContainer] = value;
            }
        }
        public string Progress
        {
            get
            {
                return settings.ContainsKey(SettingsNames.progress) ? settings[SettingsNames.progress] as string : null;
            }
            set
            {
                settings[SettingsNames.progress] = value;
            }
        }
        public string BlobSize
        {
            get
            {
                return settings.ContainsKey(SettingsNames.blobSize) ? settings[SettingsNames.blobSize] as string : null;
            }
            set
            {
                settings[SettingsNames.blobSize] = value;
            }
        }
        public UploaderSliceds Sliced
        {
            get
            {
                return settings.ContainsKey(SettingsNames.sliced) ? (UploaderSliceds)settings[SettingsNames.sliced] : UploaderSliceds.Auto;
            }
            set
            {
                settings[SettingsNames.sliced] = value;
            }
        }
        public string LimitSize
        {
            get
            {
                return settings.ContainsKey(SettingsNames.limitSize) ? settings[SettingsNames.limitSize] as string : null;
            }
            set
            {
                settings[SettingsNames.limitSize] = value;
            }
        }
        public string ParseResult
        {
            get
            {
                return settings.ContainsKey(SettingsNames.parseResult) ? settings[SettingsNames.parseResult] as string : null;
            }
            set
            {
                settings[SettingsNames.parseResult] = value;
            }
        }
        public bool RegisterScript { get; set; }
        int ParseTimeout(string v)
        {
            string[] units = new string[] { "ms", "ss", "mm", "hh" };
            Func<string, int> fn = u =>
            {
                for (int i = 0; i < units.Length; i++) { if (units[i] == u)return i; }
                return 0;
            };
            Regex rgx = new Regex(@"^\s*(\d+)\s*(ms|(ss)|(mm)|(hh))?\s*$", RegexOptions.IgnoreCase);
            var m = rgx.Match(v);

            if (m == null || !m.Success)
            { throw new ArgumentException("“" + v + "”无效的表达式,如：二十四小时“24hh”的格式"); }
            int time;
            int.TryParse(m.Groups[1].Value, out time);
            int index = fn(m.Groups[2].Success ? m.Groups[2].Value.ToLower() : "ss");
            while (index-- > 0)
            {
                time *= (index == 1 ? 1000 : 60);

            }
            return time;
        }
        long ParseSize(string v)
        {
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
            Func<string, int> fn = u =>
            {
                for (int i = 0; i < units.Length; i++) { if (units[i] == u)return i; }
                return 0;
            };
            Regex rgx = new Regex(@"^\s*(\d+)\s*(B|(KB)|(MB)|(GB)|(TB))?\s*$", RegexOptions.IgnoreCase);
            var m = rgx.Match(v);

            if (m == null || !m.Success)
            { throw new ArgumentException("“"+v+"”无效的表达式，请参照“232MB”的格式"); }
            long size;
            long.TryParse(m.Groups[1].Value, out size);
            int index = fn(m.Groups[2].Success ? m.Groups[2].Value.ToUpper() : "MB");
            while (index-- > 0)
            {
                size *= 1024;

            }
            return size;
        }
        string ParseSetting(SettingsNames key,object v)
        {
            if (!settings.ContainsKey(key)) return null; 
            string s;
            long size;
            switch (key)
            {
                case SettingsNames.url:
                    s= v as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    { s = "\"" + ConvertHelper.ConvertToClientString(ResolveUrl(s)) + "\""; }
                    return s;
                case SettingsNames.maxQueue:
                    return v == null ? null : v.ToString();
                case SettingsNames.blobSize:
                    size = ParseSize(v as string);
                    long max=(((HttpRuntimeSection)WebConfigurationManager.OpenWebConfiguration("/").GetSection("system.web/httpRuntime")).MaxRequestLength-2)*1024;
                    return (size > max ? max : size).ToString();
                case SettingsNames.limitSize:
                    size = ParseSize(v as string);
                    return size.ToString();
                case SettingsNames.timeout:
                    return ParseTimeout(v as string).ToString();
                case SettingsNames.dragable:
                case SettingsNames.multiple:
                    return ((bool)v) ? "true" : "false";
                case SettingsNames.sliced:
                    return ((UploaderSliceds)v) != UploaderSliceds.Auto ? ((UploaderSliceds)v).GetHashCode().ToString() : null;
                case SettingsNames.dragContainer:
                case SettingsNames.placeholder:
                case SettingsNames.progress:
                    s = (v + "").Trim();
                    if (s.StartsWith("$:"))
                    { s = s.Substring(2); }
                    else { s = "\"" + ConvertHelper.ConvertToClientString(s) + "\""; }
                    return s;
                case SettingsNames.parseResult:
                    s = (v + "").Trim();
                    if (s.StartsWith("javascript:", StringComparison.CurrentCultureIgnoreCase))
                    { s = "function(serverData){\n" + s.Substring(11) + "\n}"; }
                    return s;
                default:
                    s= v as string;
                    if (!string.IsNullOrWhiteSpace(s))
                    { s = "\"" + ConvertHelper.ConvertToClientString(s) + "\""; }
                    return s;

            }

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (RegisterScript) { RegisterClientScript(); }
        }
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            Dictionary<string, object> AddedKey;
            bool flag;
            StringBuilder sb = new StringBuilder();
            foreach (var s in settings)
            {
                string v = ParseSetting(s.Key, s.Value);
                if (!string.IsNullOrWhiteSpace(v))
                { sb.AppendFormat("{0}{1}:{2}", sb.Length > 0 ? "," : null, s.Key, v); }
            }
            if (!string.IsNullOrWhiteSpace(_token))
            {
                if (PostParameters == null) { PostParameters = new PostParametersCollection(); }
                PostParameters.Insert(0, new PostParameter("token", _token));
            
            }
            if (PostParameters != null && PostParameters.Count > 0)
            {
                AddedKey = new Dictionary<string, object>();
                flag = false;
                sb.AppendFormat("{0}params:{{", sb.Length > 0 ? "," : null);
                foreach (var s in PostParameters)
                {
                    AddedKey.Add(s.Key, null);
                    sb.AppendFormat("{0}\"{1}\":\"{2}\"", flag ? "," : null
                        , ConvertHelper.ConvertToClientString(s.Key).Trim()
                        , ConvertHelper.ConvertToClientString(s.Value).Trim());

                    flag = true;
                }
                sb.Append("}");
                AddedKey = null;
            }
            if (sb.Length > 0)
            {
                sb.Insert(0, "{\n");
                sb.Append("\n}");
            }
            if (ClientEvents != null && ClientEvents.Count > 0)
            {
                flag = false;
                AddedKey = new Dictionary<string, object>();
                foreach (var s in ClientEvents)
                {
                    string k = s.EventName.ToString(), v = s.Handle;
                    if (string.IsNullOrWhiteSpace(v)) continue;
                    AddedKey.Add(k, null);
                    if (v.StartsWith("javascript:", StringComparison.CurrentCultureIgnoreCase))
                    { v = "function(file,args){\n" + v.Substring(11) + "\n}"; }
                    sb.AppendFormat("{0}\"{1}\":{2}", flag ? "," : ",\n{\n", k, v);
                    flag = true;
                }
                if (flag)
                {
                    sb.Append("\n}");
                }

            }
            string js = "var " + this.ClientID + "=new Uploader(" + sb.ToString() + ")";

            
            writer.Write("<script>\n"+js+"\n</script>");
        }
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            if (ViewTemplate != null)
            {
                ViewTemplate item = new ViewTemplate();
                ViewTemplate.InstantiateIn(item);
                this.Controls.Add(item);
            }
        }
        string GetWebResourceUrl(string rn)
        {
            return Page.ClientScript.GetWebResourceUrl(this.GetType(), rn);
        } 
        void RegisterClientScript()
        {
            if (this.Page.Header != null)
            {
                string id = "Uploader.release.min.js";
                if (this.Page.Header.FindControl(id) == null)
                {
                    Control container = new Control();
                    container.ID = id;
                    HtmlGenericControl js = new HtmlGenericControl("script");
                    js.Attributes.Add("src", GetWebResourceUrl("Html5Uploader.Controls.Uploader.release.min.js"));
                    js.InnerHtml = " ";
                    container.Controls.Add(js);
                    Page.Header.Controls.Add(js);
                }
            }
        }
    }
}
