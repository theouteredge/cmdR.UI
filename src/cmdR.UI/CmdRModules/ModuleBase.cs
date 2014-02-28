using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace cmdR.UI.CmdRModules
{
    public class ModuleBase
    {
        protected CmdR _cmdR;

        protected IDictionary<string, string> GetMarks()
        {
            if (!_cmdR.State.Variables.ContainsKey("marks"))
                _cmdR.State.Variables["marks"] = new Dictionary<string, string>();

            return _cmdR.State.Variables["marks"] as IDictionary<string, string>;
        }

        protected string ParseMarks(string input)
        {
            if (!_cmdR.State.Variables.ContainsKey("marks"))
                return input;


            var marks = _cmdR.State.Variables["marks"] as IDictionary<string, string>;
            if (marks == null)
                return input;


            foreach (var mark in marks)
            {
                var regex = string.Format("{{{0}}}", mark.Key);

                if (Regex.IsMatch(input, regex))
                    input = Regex.Replace(input, regex, mark.Value);
            }

            return input;
        }


        protected string GetEndOfPath(string path)
        {
            var getEndOfPath = new Regex(@"(?<=\\)[^\\]{1,}$");
            return getEndOfPath.IsMatch(path) ? getEndOfPath.Matches(path)[0].ToString() : path;
        }

        protected string GetPath(string path)
        {
            path = ParseMarks(path);

            var combinedPath = Path.Combine((string)_cmdR.State.Variables["path"], path);

            if (Directory.Exists(combinedPath))
                return new DirectoryInfo(combinedPath).FullName;

            if (Directory.Exists(path))
                return new DirectoryInfo(path).FullName;

            if (File.Exists(combinedPath))
                return new FileInfo(combinedPath).FullName;

            if (File.Exists(path))
                return new FileInfo(path).FullName;

            return null;
        }


        protected bool IsDirectory(string path)
        {
            path = ParseMarks(path);

            var combinedPath = Path.Combine((string)_cmdR.State.Variables["path"], path);
            return Directory.Exists(combinedPath) || Directory.Exists(path);
        }

        protected bool IsFile(string path)
        {
            path = ParseMarks(path);

            var combinedPath = Path.Combine((string)_cmdR.State.Variables["path"], path);
            return File.Exists(combinedPath) || File.Exists(path);
        }


        protected void WriteWhite(string output)
        {
            Write("#FFF2F2F2", output);
        }

        protected void WriteYellow(string output)
        {
            Write("Yellow", output);
        }

        protected void WriteOrange(string output)
        {
            Write("#FFFF9900", output);
        }

        protected void WriteBlue(string output)
        {
            Write("#FF1BA1E2", output);
        }

        protected void WriteRed(string output)
        {
            Write("#FFE51400", output);
        }

        protected void WriteGreen(string output)
        {
            Write("#FF339933", output);
        }

        protected void WritePink(string output)
        {
            Write("#FFE671B8", output);
        }

        protected void WriteMagenta(string output)
        {
            Write("#FFFF0097", output);
        }



        protected void WriteLineWhite(string output)
        {
            WriteLine("#FFF2F2F2", output);
        }

        protected void WriteLineYellow(string output)
        {
            WriteLine("Yellow", output);
        }

        protected void WriteLineOrange(string output)
        {
            WriteLine("#FFFF9900", output);
        }

        protected void WriteLineBlue(string output)
        {
            WriteLine("#FF1BA1E2", output);
        }

        protected void WriteLineRed(string output)
        {
            WriteLine("#FFE51400", output);
        }

        protected void WriteLineGreen(string output)
        {
            WriteLine("#FF339933", output);
        }

        protected void WriteLinePink(string output)
        {
            WriteLine("#FFE671B8", output);
        }

        protected void WriteLineMagenta(string output)
        {
            WriteLine("#FFFF0097", output);
        }


        protected void WriteLine(string colour, string output)
        {
            _cmdR.Console.WriteLine("<Run Foreground=\"{1}\">{0}</Run>", output.XmlEscape(), colour);
        }

        protected void Write(string colour, string output)
        {
            _cmdR.Console.Write("<Run Foreground=\"{1}\">{0}</Run>", output.XmlEscape(), colour);
        }
    }
}