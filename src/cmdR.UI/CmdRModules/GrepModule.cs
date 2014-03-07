using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cmdR.IO;

namespace cmdR.UI.CmdRModules
{
    public class GrepModule : ModuleBase, ICmdRModule
    {
        public GrepModule(CmdR cmdR)
        {
            _cmdR = cmdR;

            cmdR.RegisterRoute("grep file-match content-match", Grep, "Reads any files matching the file-match regex and matches its contents against the content-match regex, each matching line is printed out to the console\n\t/r switch enables the search to match files in all subfolders");
            cmdR.RegisterRoute("grep-rep file output-file regex replace", GrepReplace, "Reads the file and matches its contents against the regex all matches will be replaced and the result will be written to the output-file\nUse /t to run without saving the results back to the file");
        }

        private int GrepInDirectories(string path, Regex filematch, Regex contentmatch)
        {
            var count = 0;
            foreach (var directory in Directory.GetDirectories(path))
            {
                try
                {
                    count += GrepInFiles(directory, filematch, contentmatch);
                    count += GrepInDirectories(directory, filematch, contentmatch);
                }
                catch (Exception e)
                {
                    WriteLineRed(string.Format("An error occurred while trying to access \\{0}", GetEndOfPath(path)));
                    WriteLineRed(string.Format("{0}", e.Message));

                    return count;
                }
            }

            return count;
        }

        private int GrepInFiles(string path, Regex filematch, Regex contentmatch)
        {
            var count = 0;

            base.WriteLineWhite(string.Format("Searching {0}", path));

            foreach (var file in Directory.GetFiles(path).Where(f => filematch.IsMatch(f)))
            {
                var lines = 0;
                count = 0;

                foreach (var line in File.ReadLines(file, Encoding.ASCII))
                {
                    lines++;
                    

                    if (contentmatch.IsMatch(line))
                    {
                        count++;

                        //todo: highlight the matched text

                        Encoding encoding;
                        if (IsText(out encoding, file))
                            _cmdR.Console.WriteLine(" {0} {1} ", lines.ToString().PadRight(3), line); // line);
                        else
                        {
                            _cmdR.Console.WriteLine(" Match found in {0} on line {1}, unable to display the data as the file apears to be a binary file", file, line);
                            break;
                        }

                    }
                }

                if (count > 0)
                    _cmdR.Console.WriteLine("{0} matches found in file {1}", count, file);
            }

            return count;
        }



        private void Grep(IDictionary<string, string> param, CmdR cmdR)
        {
            var filematch = new Regex(param["file-match"]);
            var contentmatch = new Regex(param["content-match"]);

            var path = (string) cmdR.State.Variables["path"];
            if (Directory.Exists(path))
            {
                var count = 0;
                if (param.ContainsKey("/r"))
                    count = GrepInDirectories((string)cmdR.State.Variables["path"], filematch, contentmatch);
                else
                    count = GrepInFiles((string)cmdR.State.Variables["path"], filematch, contentmatch);

                if (count == 0)
                    WriteLineYellow("No matches found");
                else
                    WriteLineYellow(string.Format("{0} matches found", count));
            }
            else cmdR.Console.WriteLine("{0} does not exist", path);
        }


        private void GrepReplace(IDictionary<string, string> param, CmdR cmdR)
        {
            var path = Path.Combine((string)cmdR.State.Variables["path"], param["file"]);
            if (File.Exists(path))
            {
                var count = 0;
                var lines = 0;
                var match = new Regex(param["regex"]);
                var content = "";

                foreach (var line in File.ReadLines(path))
                {
                    lines++;
                    if (match.IsMatch(line))
                    {
                        count++;

                        var newline = match.Replace(line, param["replace"]);

                        //todo: highlight the matched text
                        cmdR.Console.WriteLine(" {0} {1} ", lines.ToString().PadRight(3), line);
                        cmdR.Console.WriteLine("     {0} ", newline);

                        content = string.Format("{0}{2}{1}", content, newline, (lines == 0) ? "" : "\r\n");
                    }
                    //else content = string.Format("{0}{2}{1}", content, line, (lines == 0) ? "" : "\r\n");
                }

                if (count > 0 && !param.ContainsKey("/t"))
                    File.WriteAllText(path, content);

                cmdR.Console.WriteLine("{0} matches found", count);
            }
            else cmdR.Console.WriteLine("{0} does not exist", path);
        }



        /// <summary>
        /// Detect if a file is text and detect the encoding.
        /// </summary>
        /// <param name="encoding">
        /// The detected encoding.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="windowSize">
        /// The number of characters to use for testing.
        /// </param>
        /// <returns>
        /// true if the file is text.
        /// </returns>
        public static bool IsText(out Encoding encoding, string fileName, int windowSize = 5)
        {
            using (var fileStream = File.OpenRead(fileName))
            {
                var rawData = new byte[windowSize];
                var text = new char[windowSize];
                var isText = true;

                // Read raw bytes
                var rawLength = fileStream.Read(rawData, 0, rawData.Length);
                fileStream.Seek(0, SeekOrigin.Begin);

                // Detect encoding correctly (from Rick Strahl's blog)
                // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
                if (rawData[0] == 0xef && rawData[1] == 0xbb && rawData[2] == 0xbf)
                {
                    encoding = Encoding.UTF8;
                }
                else if (rawData[0] == 0xfe && rawData[1] == 0xff)
                {
                    encoding = Encoding.Unicode;
                }
                else if (rawData[0] == 0 && rawData[1] == 0 && rawData[2] == 0xfe && rawData[3] == 0xff)
                {
                    encoding = Encoding.UTF32;
                }
                else if (rawData[0] == 0x2b && rawData[1] == 0x2f && rawData[2] == 0x76)
                {
                    encoding = Encoding.UTF7;
                }
                else
                {
                    encoding = Encoding.Default;
                }

                // Read text and detect the encoding
                using (var streamReader = new StreamReader(fileStream))
                {
                    streamReader.Read(text, 0, text.Length);
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var streamWriter = new StreamWriter(memoryStream, encoding))
                    {
                        // Write the text to a buffer
                        streamWriter.Write(text);
                        streamWriter.Flush();

                        // Get the buffer from the memory stream for comparision
                        var memoryBuffer = memoryStream.GetBuffer();

                        // Compare only bytes read
                        for (var i = 0; i < rawLength && isText; i++)
                        {
                            isText = rawData[i] == memoryBuffer[i];
                        }
                    }
                }

                return isText;
            }
        }
    }
}
