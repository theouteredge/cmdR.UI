using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cmdR.IO;


namespace cmdR.UI.CmdRModules
{
    public class FileModule : ModuleBase, ICmdRModule
    {
        public void Initalise(CmdR cmdR, bool overwriteRoutes)
        {
            _cmdR = cmdR;

            cmdR.RegisterRoute("head file rows?", Head, "Previews the first n lines of a file, n defaults to 10, but you can specify the number of rows to read");
            cmdR.RegisterRoute("rn match replace", Rename, "Renames a file using a regex match and replace\n/t if you want to run a test first");
            
            // doesnt work
            //cmdR.RegisterRoute("handles path?", Handles, "Lists the Open Handles on a directory", overwriteRoutes);

            cmdR.RegisterRoute("touch regex? date?", Touch, "Updates the Last Modified Date of all files in the current directory\n/modifed sets the modified date (default)\n/created modifies the file created date\n/accessed modifies the last accessed datetime\n/t to show which files would be touched without actually touching them");
            
            cmdR.RegisterRoute("mkf file", MakeFile, "Creates a file");
            cmdR.RegisterRoute("rmf match", RemoveFiles, "Deletes all files matching the regex\n/t to run a test without modifying the system");

            cmdR.RegisterRoute("find match", Find, "Finds all file names matching a given regex\n/r to search in sub directories");
        }

        private void Find(IDictionary<string, string> param, CmdR cmdR)
        {
            var match = new Regex(param["match"]);
            var count = 0;

            if (param.ContainsKey("/r"))
                count = FindInDirectories((string)cmdR.State.Variables["path"], match);
            else
                count = FindFilesIn((string)cmdR.State.Variables["path"], match);

            if (count == 0)
                WriteLineYellow("No files found");
            else
                WriteLineYellow(string.Format("{0} files found", count));
        }

        private int FindInDirectories(string path, Regex match)
        {
            var count = 0;
            foreach (var directory in Directory.GetDirectories(path))
            {
                try
                {
                    count += FindFilesIn(directory, match);
                    count += FindInDirectories(directory, match);
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

        private int FindFilesIn(string path, Regex match)
        {
            var count = 0;
            foreach (var file in Directory.GetFiles(path))
            {
                if (match.IsMatch(file))
                {
                    WriteLinePink(file);
                    count++;
                }
            }

            return count;
        }


        private void RemoveFiles(IDictionary<string, string> param, CmdR cmdR)
        {
            var match = new Regex(param["match"]);
            RemoveFiles((string)_cmdR.State.Variables["path"], param.ContainsKey("/r"), param.ContainsKey("/t"), match);
        }

        private void RemoveFiles(string path, bool recurse, bool test, Regex match)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                if (match.IsMatch(file))
                {
                    if (test)
                        _cmdR.Console.WriteLine(file);
                    else
                        try
                        {
                            File.Delete(file);
                            WriteLineYellow(string.Format("\tdeleted {0}", file));
                        }
                        catch (Exception e)
                        {
                            WriteLineOrange(string.Format("\tunable to delete the file: {0}", file));
                            WriteLineOrange(string.Format("\terror message: {0}", e.Message));
                        }
                }
            }

            if (recurse)
            {
                foreach (var directory in Directory.GetDirectories(path))
                    RemoveFiles(directory, true, test, match);
            }
        }

        private void MakeFile(IDictionary<string, string> param, CmdR cmdR)
        {
            File.Create(Path.Combine((string)cmdR.State.Variables["path"], param["file"]));
        }


        private void Touch(IDictionary<string, string> param, CmdR cmdR)
        {
            var path = cmdR.State.Variables["path"].ToString();
            if (Directory.Exists(path))
            {
                var modified = param.ContainsKey("/modified");
                var created = param.ContainsKey("/created");
                var accessed = param.ContainsKey("/accessed");

                var date = DateTime.Now;

                if (param.ContainsKey("date"))
                    date = DateTime.Parse(param["date"]);

                if (!modified && !created && !accessed)
                    modified = true;

                Regex match = null;
                if (param.ContainsKey("regex"))
                    match = new Regex(param["regex"]);

                foreach (var file in Directory.GetFiles(path).Where(x => match != null && match.IsMatch(x)))
                {
                    if (param.ContainsKey("/t"))
                    {
                        cmdR.Console.WriteLine("would touch {0}", file);
                    }
                    else
                    {
                        try
                        {
                            if (modified)
                                File.SetLastWriteTime(file, date);

                            if (created)
                                File.SetLastAccessTime(file, date);

                            if (accessed)
                                File.SetCreationTime(file, date);

                            cmdR.Console.WriteLine("touched {0}", file);
                        }
                        catch (Exception ex)
                        {
                            WriteLineRed(string.Format("An exception was thrown while trying to touch {0}", file));
                            WriteLineRed(string.Format("Exception: {0}", ex.Message));
                        }
                    }
                }
            }
            else cmdR.Console.WriteLine("{0} does not exist", path);
        }

        private void Handles(IDictionary<string, string> param, CmdR cmdR)
        {
            var path = param.ContainsKey("path") ? GetPath(param["path"]) : (string)cmdR.State.Variables["path"];

            if (!Directory.Exists(path))
            {
                cmdR.Console.WriteLine("{0} doesnt exist", path ?? param["path"]);
                return;
            }

            cmdR.Console.WriteLine("looking for process with files handles open within {0}", path);

            var count = 0;
            foreach (var process in Win32Processes.GetProcessesLockingFile(path))
            {
                cmdR.Console.WriteLine(" {0}", process.ProcessName);
                count++;
            }

            cmdR.Console.WriteLine("{0} process with files handles open within {1}\n", count, path);
        }


        private void Rename(IDictionary<string, string> param, CmdR cmdR)
        {
            var match = new Regex(param["match"]);
            var count = 0;
            var errors = 0;

            foreach (var file in Directory.GetFiles((string)cmdR.State.Variables["path"]).Where(file => match.IsMatch(file)))
            {
                if (param.ContainsKey("/t"))
                {
                    count++;
                    cmdR.Console.WriteLine(" {0} to {1}", file, match.Replace(file, param["replace"]));
                }
                else
                {
                    try
                    {
                        File.Move(file, match.Replace(file, param["replace"]));
                        Console.WriteLine("{0}", match.Replace(file, param["replace"]));

                        count++;
                    }
                    catch (Exception e)
                    {
                        cmdR.Console.WriteLine("An exception occurred while renaming {0}", file);
                        errors++;
                    }
                }
            }

            if (param.ContainsKey("/t"))
                cmdR.Console.WriteLine("{0} files matched", count);
            else
                cmdR.Console.WriteLine("{0} files renames with {1} errors", count, errors);
        }

        private void Head(IDictionary<string, string> param, CmdR cmdR)
        {
            var path = Path.Combine((string)cmdR.State.Variables["path"], param["file"]);
            var take = param.ContainsKey("rows") ? int.Parse(param["rows"]) : 10;

            if (File.Exists(path))
            {
                var count = 0;
                foreach (var line in File.ReadLines(path).Take(take))
                {
                    cmdR.Console.WriteLine(" {1}.  {0}", line, ++count);
                }
            }
            else cmdR.Console.WriteLine("{0} doesn't exist", param["file"]);
        }
    }
}
