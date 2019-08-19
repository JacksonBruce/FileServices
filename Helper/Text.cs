using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Web;

namespace System.Helper
{
    public class Text
    {
        /// <summary>
        /// 截取指定长度的HTML
        /// </summary>
        /// <param name="html">被截取的HTML代码</param>
        /// <param name="textMaxLength">指定的长度</param>
        /// <param name="tags">要过滤的标签名称,例如:div </param>
        /// <returns>返回被截取后的html代码</returns>
        public static string InterceptHtml(string html, int textMaxLength, params string[] tags)
        {
            int o;
            return InterceptHtml(html, textMaxLength, out o, tags);
        }
        /// <summary>
        /// 截取指定长度的HTML
        /// </summary>
        /// <param name="html">被截取的HTML代码</param>
        /// <param name="textMaxLength">指定的长度</param>
        /// <param name="residualCount">输出剩余的数量</param>
        /// <param name="tags">要过滤的标签名称,例如:div </param>
        /// <returns>返回被截取后的html代码</returns>
        public static string InterceptHtml(string html, int textMaxLength, out int residualCount 
            ,params string[] tags )
        {
            residualCount = 0;
            if (string.IsNullOrEmpty(html) || html.Trim() == "")
            { return html; }
            string str = "";
            html = FilterTag(html, tags);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root></root>");
            XmlNode n = doc.DocumentElement;
            n.InnerXml = html;
            int len = 0;
            residualCount = n.InnerText.Length - textMaxLength;
            if (residualCount < 0) residualCount = 0;
            if (n.HasChildNodes)
            {
                XmlNodeList ns = GetNodes(n);
                foreach (XmlNode t in ns)
                {
                    if (t.InnerText.Trim() == "")
                    {
                        str += t.OuterXml;
                        continue;
                    }
                    if (t.InnerText.Trim().Length <= textMaxLength - len)
                    {
                        str += t.OuterXml;
                        len += t.InnerText.Trim().Length;
                    }
                    else
                    {

                        if (t.NodeType == XmlNodeType.Text || (t.HasChildNodes && t.ChildNodes.Count == 1 && t.ChildNodes[0].NodeType == XmlNodeType.Text))
                        {
                            string text = t.InnerText.Trim();
                            len = textMaxLength - len;
                            text = text.Length > len ? text.Substring(0, len) : text;
                            t.InnerText = text;
                            str += t.OuterXml;
                        }
                        else
                        {
                            t.InnerXml = InterceptHtml(t.InnerXml, textMaxLength - len);
                            str += t.OuterXml;
                        }
                        break; ;
                    }
                    if (len >= textMaxLength)
                    { break; }
                }
            }
            else
            {
                string text = n.InnerText.Trim();
                text = text.Length > textMaxLength ? text.Substring(0, textMaxLength) : text;
                n.InnerText = text;
                str = n.OuterXml;

            }

            return str;

        }
        public static string Replace(string html, Regex rgx, MatchEvaluator evaluator)
        { 
            if (string.IsNullOrEmpty(html) || html.Trim() == "")
            { return html; }
            html = FilterTag(html, null);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root></root>");
            XmlNode n = doc.DocumentElement;
            n.InnerXml = html;
            Replace(n, rgx, evaluator);
            return n.InnerXml;
        }
        private static void Replace(XmlNode n, Regex rgx, MatchEvaluator evaluator)
        {
           
            if (n.HasChildNodes)
            {
                XmlNodeList ns = GetNodes(n);
                foreach (XmlNode t in ns)
                {
                    if (t.NodeType == XmlNodeType.Text)
                    {
                        if (t.InnerText.Trim() == "")
                        {
                            continue;
                        }
                        t.InnerText = rgx.Replace(t.InnerText, evaluator);
                    }
                    else if(t.HasChildNodes){
                        Replace(t, rgx, evaluator);
                    }
                }
            }
            else
            {
                if (n.NodeType == XmlNodeType.Text)
                {
                    if (n.InnerText.Trim() != "")
                    {
                        n.InnerText = rgx.Replace(n.InnerText, evaluator);
                    }
                }
            }
        }
        private static XmlNodeList GetNodes(XmlNode e)
        {
            return e.ChildNodes;
        }
        private static string FilterTag(string htm, string[] tags)
        {

            Regex r;
            r = new Regex(@"</?\w+:\w+[^>]*>", RegexOptions.IgnoreCase);
            htm = r.Replace(htm, "");
            r = new Regex(@"<(\W)");
            htm = r.Replace(htm, m => m.Success && m.Groups[1].Value!="/" ? HttpUtility.HtmlEncode(m.Value) : m.Value);
            if (tags == null || tags.Length == 0)
                return htm;
            foreach (string t in tags)
            {
                r = new Regex(@"</?" + t + "[^>]*>", RegexOptions.IgnoreCase);
                htm = r.Replace(htm, "");
            }
            
            return htm;
        }
        /// <summary>
        /// 截取指定长度的文本
        /// </summary>
        /// <param name="text">原文本</param>
        /// <param name="textMaxLength">最大长度</param>
        /// <returns></returns>
        public static string InterceptText(string text, int textMaxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= textMaxLength) return text ?? "";
            return text.Substring(0, textMaxLength) + "...";
        }
        public static string Default(object text, params string[] DefaultValues) 
        {
            return Default(text as string, DefaultValues);
        }
        public static string Default(string text,params string[] DefaultValues)
        {
            if (DefaultValues == null || DefaultValues.Length <= 0) return text;
            var v = text;int i = 0;
            while (string.IsNullOrWhiteSpace(v) && i < DefaultValues.Length)
            {
                v = DefaultValues[i];
                i++;
            } 
            return v;
        }
        public static string Trim(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
        }
    }
    


}
