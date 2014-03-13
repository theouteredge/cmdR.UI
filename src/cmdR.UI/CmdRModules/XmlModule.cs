using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using cmdR.IO;
using cmdR.UI.Properties;
using Microsoft.Web.XmlTransform;

namespace cmdR.UI.CmdRModules
{
    public class XmlModule : ModuleBase
    {
        public XmlModule(CmdR cmdR)
        {
            _cmdR = cmdR;

            cmdR.RegisterRoute("xslt xml xslt output?", XsltTransform, "Transforms an Xml using an Xslt transform file.\n\t/t tests the transform by printing it out on the screen");
            cmdR.RegisterRoute("xdt xml xdt output?", XdtTransform, "Transforms an Xml using an Xslt transform file.\n\t/t tests the transform by printing it out on the screen");
        }

        private void XdtTransform(IDictionary<string, string> param, CmdR arg2)
        {
            var xmlPath = GetPath(param["xml"]);
            if (!IsFile(xmlPath))
            {
                WriteErrorLine("The xml file path does not exists, {0}", xmlPath);
                return;
            }

            var xdtPath = GetPath(param["xdt"]);
            if (!IsFile(xdtPath))
            {
                WriteErrorLine("The xdt file path does not exists, {0}", xdtPath);
                return;
            }


            var xml = new XmlTransformableDocument();
            xml.PreserveWhitespace = true;
            xml.Load(xmlPath);

            var xdt = new XmlTransformation(xdtPath, true, new XdtLogger(_cmdR.Console));
            if (xdt.Apply(xml))
            {
                if (param.ContainsKey("/t"))
                    WriteLineWhite(xml.OuterXml);
                else
                    xml.Save(GetPath(param["output"]));
            }
            else WriteErrorLine("The XDT Failed! O_o");
        }


        private void XsltTransform(IDictionary<string, string> param, CmdR cmdR)
        {
            var xmlPath = GetPath(param["xml"]);
            if (!IsFile(xmlPath))
            {
                WriteErrorLine("The xml file path does not exists, {0}", xmlPath);
                return;
            }

            var xsltPath = GetPath(param["xslt"]);
            if (IsFile(xsltPath))
            {
                WriteErrorLine("The xslt file path does not exists, {0}", xsltPath);
                return;
            }


            var xml = File.ReadAllBytes(xmlPath);

            var xd = new XmlDocument();
            xd.Load(new MemoryStream(xml));

            var xslt = CreateXsltTransform(xsltPath);
            var stm = new MemoryStream();

            xslt.Transform(xd, null, stm);
            stm.Position = 0;


            if (param.ContainsKey("/t"))
                WriteLineWhite(Encoding.UTF8.GetString(TrimTrailingNulls(stm.GetBuffer())));
            else
                File.WriteAllBytes(param["output"], TrimTrailingNulls(stm.GetBuffer()));
        }



        private XslCompiledTransform CreateXsltTransform(string xsltPath)
        {
            var xslt = new XslCompiledTransform();

            var xsltfile = File.ReadAllBytes(xsltPath);
            xslt.Load(new XmlTextReader(new MemoryStream(xsltfile)));

            return xslt;
        }

        private byte[] TrimTrailingNulls(byte[] b)
        {
            var i = b.Length - 1;
            while (b[i] == 0)
                --i;

            var result = new byte[i + 1];
            Array.Copy(b, result, i + 1);

            return result;
        }
    }

    public class XdtLogger : IXmlTransformationLogger
    {
        private readonly ICmdRConsole _console;

        public XdtLogger(ICmdRConsole console)
        {
            _console = console;
        }


        private void Write(string message, params object[] messageArgs)
        {
            _console.Write("<Run FontWeight=\"Bold\" Foreground=\"Yellow\">{0}</Run>", string.Format(message, messageArgs).XmlEscape());
        }

        private void WriteLine(string message, params object[] messageArgs)
        {
            _console.WriteLine("<Run FontWeight=\"Bold\" Foreground=\"Yellow\">{0}</Run>", string.Format(message, messageArgs).XmlEscape());
        }

        public void LogMessage(string message, params object[] messageArgs)
        {
            WriteLine(message, messageArgs);
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            WriteLine(type + ": " + message, messageArgs);
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            WriteLine("WARNING: " + message, messageArgs);
        }

        public void LogWarning(string file, string message, params object[] messageArgs)
        {
            WriteLine(string.Format("WARNING: file: {0}\nMessage: ", file) + message, messageArgs);
        }

        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            WriteLine(string.Format("WARNING: file: {0}, line: {1}, position: {2}\nMessage: ", file, lineNumber, linePosition) + message, messageArgs);
        }

        public void LogError(string message, params object[] messageArgs)
        {
            WriteLine("ERROR: " + message, messageArgs);
        }

        public void LogError(string file, string message, params object[] messageArgs)
        {
            WriteLine(string.Format("ERROR: file: {0}\nMessage: ", file) + message, messageArgs);
        }

        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            WriteLine(string.Format("ERROR: file: {0}, line: {1}, position: {2}\nMessage: ", file, lineNumber, linePosition) + message, messageArgs);
        }

        public void LogErrorFromException(Exception ex)
        {
            WriteLine("ERROR: {0}", ex.Message);
        }

        public void LogErrorFromException(Exception ex, string file)
        {
            WriteLine("ERROR: File: {1}\nMessage: {0}", ex.Message, file);
        }

        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition)
        {
            WriteLine("ERROR: file: {0}, line: {1}, position: {2}\nMessage: {3}", file, lineNumber, linePosition, ex.Message);
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            WriteLine(message, messageArgs);
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            WriteLine("");
            WriteLine(type + ": " + message, messageArgs);
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            WriteLine(message, messageArgs);
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            WriteLine(type + ": " + message, messageArgs);
        }
    }
}
