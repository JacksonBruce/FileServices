using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.IO;
using System.Web;
namespace System.Helper
{
    public class Url
    {
        public static string GetRelativePath(string path)
        {
            Image img = new Image();
            img.ImageUrl = path;
            StringWriter sw = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(sw);
            img.RenderControl(writer);
            Regex rgx = new Regex(@"src=""([^\""]+)""", RegexOptions.IgnoreCase);
            Match m = rgx.Match(sw.ToString());
            if (!m.Success)
            { return path; }
            return m.Groups[1].Value;
        }
    }
}
