using System.Text.RegularExpressions;
using System.Xml;

namespace cmdR.UI
{
    public static class StringExtensions
    {
        public static string XmlEscape(this string text)
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("root");
            node.InnerText = text;
            return node.InnerXml;
        }
    }
}