using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace System.Xml.Linq
{
    public static class XmlLinqExtensionMethods

    {
        public static XElement Attr(this XElement e, string name, object value)
        {
            if (e == null||string.IsNullOrEmpty(name)) return e;
            XAttribute attr = e.Attribute(name);
            if (attr == null)
            {
                if (value != null)
                {
                    e.Add(new XAttribute(name, value));
                }
            }
            else {
                if (value == null) {
                    attr.Remove();
                }
                else
                {
                    attr.Value = Convert.ToString(value);
                }
            }
            return e;
        }
        public static string Attr(this XElement e, string name)
        {
            XAttribute attr = e == null || string.IsNullOrEmpty(name) ? null : e.Attribute(name);
            return attr == null ? string.Empty : HttpUtility.HtmlDecode(attr.Value);
        }
        public static T Attr<T>(this XElement e, string name) where T : struct
        {
            return e.AttrStruct<T>(name, default(T));
        }
        public static T AttrStruct<T>(this XElement e, string name, T defaultValue = default(T)) where T : struct
        {
            string v = e.Attr(name);
            if (string.IsNullOrWhiteSpace(v)) return defaultValue;
            Type t = typeof(T);
            try
            {
                if (t.IsEnum)
                {
                    return (T)Enum.Parse(t, v);
                }
                else
                {
                    return (T)Convert.ChangeType(v, t);
                }
            }
            catch {
                if (t == typeof(Guid))
                {
                    object o = new Guid(v);
                    return (T)o;
                }
            }
            return defaultValue;
        }

        public static string Attr(this HtmlNode e, string name)
        {
            var attr = e == null || !e.HasAttributes || string.IsNullOrEmpty(name) ? null : e.Attributes[name];
            return attr == null ? string.Empty : HttpUtility.HtmlDecode(attr.Value);
        }
        public static T Attr<T>(this HtmlNode e, string name) where T : struct
        {
            return e.AttrStruct<T>(name, default(T));
        }
        public static T AttrStruct<T>(this HtmlNode e, string name, T defaultValue = default(T)) where T : struct
        {
            string v = e.Attr(name);
            if (string.IsNullOrWhiteSpace(v)) return defaultValue;
            Type t = typeof(T);
            try
            {
                if (t.IsEnum)
                {
                    return (T)Enum.Parse(t, v);
                }
                else
                {
                    return (T)Convert.ChangeType(v, t);
                }
            }
            catch
            {
                if (t == typeof(Guid))
                {
                    object o = new Guid(v);
                    return (T)o;
                }
            }
            return defaultValue;
        }
        public static string Text(this HtmlNode e)
        {
            return e != null ? e.InnerText : string.Empty;
        }
        public static string Html(this HtmlNode e)
        {
            return e != null ? e.InnerHtml : string.Empty;
        }

        public static bool HasClass(this HtmlNode node,string className)
        {
            string es = Regex.Escape(className);
            var rgx = new Regex(@"(\s+" + es + @"\s*$)|(^\s*" + es + @"\s+)|(\s+" + es + @"\s+)");
            var v = node.Attr("class").Trim();
            return v != "" && (v == className || rgx.IsMatch(v));
        }
        public static HtmlNode GetNodeByClasName(this HtmlNode node, string className)
        {
            if (!node.HasChildNodes) return null;
            var nodes = node.ChildNodes;
            foreach (var item in nodes)
            {
                if (item.NodeType != HtmlNodeType.Element) continue;
                if (item.HasClass(className))
                {
                    return item;
                }
                var o = item.GetNodeByClasName(className);
                if (o != null) return o;
            }
            return null;
        }

        public static HtmlNode GetNodeByTagName(this HtmlNode node, string tagName)
        {
            if (!node.HasChildNodes||string.IsNullOrWhiteSpace(tagName)) return null;
            var nodes = node.ChildNodes;
            foreach (var item in nodes)
            {
                if (item.NodeType != HtmlNodeType.Element) continue;
                if (string.Equals(tagName,item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                var o = item.GetNodeByTagName(tagName);
                if (o != null) return o;
            }
            return null;
        }
        public static HtmlNode GetNodeById(this HtmlNode node, string id)
        {
            if (!node.HasChildNodes || string.IsNullOrWhiteSpace(id)) return null;
            var nodes = node.ChildNodes;
            foreach (var item in nodes)
            {
                if (item.NodeType != HtmlNodeType.Element) continue;
                if (string.Equals(id, item.Attr("id"), StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                var o = item.GetNodeById(id);
                if (o != null) return o;
            }
            return null;
        }

        public static IEnumerable<HtmlNode> GetNodesByClasName(this HtmlNode node, string className)
        {
            if (!node.HasChildNodes) return null;
            var nodes = node.ChildNodes;
            var list = new List<HtmlNode>();
           
            foreach (var item in nodes)
            {
                if (item.NodeType != HtmlNodeType.Element) continue;
                if (item.HasClass(className))
                {
                    list.Add(item);
                }
                list.AddRange(item.GetNodesByClasName(className));
            }
            return list;
        }

        public static IEnumerable<HtmlNode> GetNodesByTagName(this HtmlNode node, string tagName)
        {
            if (!node.HasChildNodes || string.IsNullOrWhiteSpace(tagName)) return null;
            var nodes = node.ChildNodes;
            var list = new List<HtmlNode>();
            foreach (var item in nodes)
            {
                if (item.NodeType != HtmlNodeType.Element) continue;

                if (string.Equals(tagName, item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(item);
                }
                list.AddRange(item.GetNodesByTagName(tagName));
            }
            return list;
        }
        public static IEnumerable<HtmlNode> AncestorNodes(this HtmlNode node)
        {
            var n = node;
            do {
                if (n != null)
                {
                    n = n.ParentNode;
                    if (n != null && n.NodeType== HtmlNodeType.Element) yield return n;
                }
            } while (n != null);


        }
        public static bool Is(this HtmlNode node, string[] selectors)
        {
            if (node == null || node.NodeType != HtmlNodeType.Element || selectors == null || selectors.Length == 0) return false;
            return selectors.Where(s => node.Is(s)).Any();
        }
        static bool IsMatch(HtmlNode e, string s)
        {

            var tagRgx = new Regex(@"^\w+", RegexOptions.IgnoreCase);
            var classRgx = new Regex(@"\.[a-zA-Z_\-]+");
            var idRgx = new Regex(@"#[a-zA-Z_\-]+");

            var css = e.Attr("class");
            var tagMatch = tagRgx.Match(s);
            var idMatch = idRgx.Match(s);
            var classMatches = classRgx.Matches(s);
            var matched = (tagMatch == null || !tagMatch.Success || string.Equals(tagMatch.Value, e.Name, StringComparison.OrdinalIgnoreCase))
                && (idMatch == null || !idMatch.Success || string.Equals(idMatch.Value.TrimStart('#'), e.Attr("id"), StringComparison.Ordinal))
                && (classMatches == null || classMatches.Count == 0 || (
                from n in classMatches.OfType<Match>()
                join c in css.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(o => "." + o)
                on n.Value equals c
                select c
                ).Count() == classMatches.Count);
            return matched;
        }
        public static bool Is(this HtmlNode node, string selector)
        { 
            if(node==null || node.NodeType!= HtmlNodeType.Element || string.IsNullOrWhiteSpace(selector)) return false;
            //string[] arr = selector.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries).Reverse().ToArray();
            selector = Regex.Replace(selector, @"\s*\>\s*", ">");
            Func<string, Tuple<string[], bool>> lastSplit = s => {
                int i1 = s.LastIndexOf('>'), i2 = s.LastIndexOf(' ');
                if (i1 < 0 && i2 < 0)
                {
                   return null;
                }
                else
                {
                    int i = Math.Max(i1, i2);
                    return new Tuple<string[], bool>(new string[] { s.Substring(i + 1).Trim(), s.Substring(0, i).Trim() }, i1 > 0 && i1 > i2);
                }
              
            }; 
            
            var current = node;
            string sltrs = selector;
            do
            {
                var tu = lastSplit(sltrs);
                string sltr;
                if (tu == null)
                {
                    sltr = sltrs;
                    sltrs = null;
                }
                else {
                    sltr = tu.Item1[0];
                    sltrs = tu.Item1[1];
                }
                var m = IsMatch(current,sltr);
                if (!m) return false;
                if (tu != null && !tu.Item2)
                {
                    bool nearMatched = false;
                    tu = lastSplit(sltrs);
                    sltr = tu == null ? sltrs : tu.Item1[0];
                    foreach (var e in current.AncestorNodes())
                    {
                        nearMatched = IsMatch(e, sltr);
                        if (nearMatched)
                        {
                            current = e.ParentNode;
                            sltrs = tu == null ? null : tu.Item1[1];
                            break;
                        }
                    }
                    if (!nearMatched) return false;
                }
                else {
                    current = current.ParentNode;
                }
               
            } while (sltrs != null);
            return true;
        }
        public static IEnumerable<HtmlNode> GetNodes(this HtmlNode node, string selectors)
        {
            return string.IsNullOrWhiteSpace(selectors) ? node.Descendants() : from n in node.Descendants()
                                                                               where n.Is(selectors.Split(','))
                                                                               select n;

        }
        public static IEnumerable<HtmlNode> Before(this HtmlNode node, Func<HtmlNode, bool> assert=null)
        {
            var n =node==null?null: node.PreviousSibling;
            do {
                if (n != null)
                {
                    if (assert == null || assert(n)) yield return n;
                    n = n.PreviousSibling;
                }
            }
            while (n != null);

        }
        public static IEnumerable<HtmlNode> Behind(this HtmlNode node, Func<HtmlNode, bool> assert = null)
        {
            var n = node == null ? null : node.NextSibling;
            do
            {
                if (n != null)
                {
                    if (assert == null || assert(n)) yield return n;
                    n = n.NextSibling;
                }
            }
            while (n != null);

        }
        public static HtmlNode Prev(this HtmlNode node)
        {
            do
            {
                node = node != null ? node.PreviousSibling : null;
            } while (node != null && node.NodeType != HtmlNodeType.Element);
            return node;
        }
        public static HtmlNode Next(this HtmlNode node)
        {
            do
            {
                node = node != null ? node.NextSibling : null;
            } while (node != null && node.NodeType != HtmlNodeType.Element);
            return node;
        }
        public static HtmlNode First(this IEnumerable<HtmlNode> nodes, string tagName = null)
        {
            if (nodes == null) return null;
            return string.IsNullOrWhiteSpace(tagName) ? nodes.FirstOrDefault()
                : (from n in nodes
                   where n.NodeType == HtmlNodeType.Element && string.Equals(n.Name, tagName, StringComparison.OrdinalIgnoreCase)
                   select n).FirstOrDefault();
        }


        public static HtmlNode First(this HtmlNodeCollection nodes, string tagName = null)
        {
            if (nodes == null || nodes.Count == 0) return null;
            return nodes.Cast<HtmlNode>().First(tagName);
        }
        public static HtmlNode Last(this IEnumerable<HtmlNode> nodes, string tagName = null)
        {
            if (nodes == null) return null;
            return string.IsNullOrWhiteSpace(tagName) ? nodes.LastOrDefault()
                : (from n in nodes
                   where n.NodeType == HtmlNodeType.Element && string.Equals(n.Name, tagName, StringComparison.OrdinalIgnoreCase)
                   select n).LastOrDefault();
        }
        public static HtmlNode Last(this HtmlNodeCollection nodes,string tagName=null)
        {
            if (nodes == null || nodes.Count == 0) return null;
            return nodes.Cast<HtmlNode>().Last(tagName);
        }

    }
}
