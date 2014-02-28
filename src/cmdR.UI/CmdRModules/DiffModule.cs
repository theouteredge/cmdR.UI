using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdR.UI.CmdRModules
{
    public class DiffModule : ModuleBase, ICmdRModule
    {
        public void Initalise(CmdR cmdR, bool overwriteRoutes)
        {
            _cmdR = cmdR;

            cmdR.RegisterRoute("diff left right", DiffFiles, "");
        }

        private void DiffFiles(IDictionary<string, string> param, CmdR cmdR)
        {
            if (base.IsDirectory(param["left"]) && base.IsDirectory(param["right"]))
            {
                DiffDirectories(param["left"], param["right"]);
            }
            else if (base.IsFile(param["left"]) && base.IsFile(param["right"]))
            {
                Diff(param["left"], param["right"]);
            }
            else WriteLineYellow("The left and right params should both be files or directories");
        }

        private void DiffDirectories(string leftPath, string rightPath)
        {
            var leftFiles = Directory.GetFiles(GetPath(leftPath));
            var rightFiles = Directory.GetFiles(GetPath(rightPath));

            //match up the files in the directories and then run Diff on each one
            foreach (var leftfile in leftFiles)
            {
                var rightfile = rightFiles.FirstOrDefault(x => GetEndOfPath(x) == GetEndOfPath(leftfile));
                if (rightfile != null)
                    Diff(leftfile, rightfile);
                else
                    WriteLineWhite(string.Format("No matching file found in the right folder to diff with {0}", leftfile));

                WriteLineWhite("");
            }
        }

        private void Diff(string left, string right)
        {
            var leftEnum = File.ReadLines(GetPath(left)).GetEnumerator();
            var rightEnum = File.ReadLines(GetPath(right)).GetEnumerator();
            var eoLeft = false;
            var eoRight = false;
            var count = 0;
            var differences = 0;

            WriteLineWhite(string.Format("Compairing {0} and {1}", left, right));

            while (!eoLeft && !eoRight)
            {
                eoLeft = ! leftEnum.MoveNext();
                eoRight = ! rightEnum.MoveNext();

                count++;

                if (!eoLeft && !eoRight)
                {
                    if (DiffLines(count, leftEnum.Current, rightEnum.Current))
                        differences++;
                }
                else if (eoLeft && !eoRight)
                    WriteLineWhite("The LEFT file is shorter than the right");

                else if (eoRight && !eoLeft)
                    WriteLineWhite("The RIGHT file is shorter than the left");
            }

            WriteLineYellow(differences == 0
                                ? "The two files are identical"
                                : string.Format("{0} differences found between the files", differences));

            leftEnum.Dispose();
            leftEnum = null;

            rightEnum.Dispose();
            rightEnum = null;
        }

        private bool DiffLines(int lineNo, string left, string right)
        {
            var leftEnum = left.GetEnumerator();
            var rightEnum = right.GetEnumerator();

            var eoLeft = false;
            var eoRight = false;
            var count = 0;
            var result = false;


            while (!eoLeft && !eoRight)
            {
                count++;

                eoLeft = !leftEnum.MoveNext();
                eoRight = !rightEnum.MoveNext();

                if (!eoLeft && !eoRight)
                {
                    if (leftEnum.Current != rightEnum.Current)
                    {
                        WriteLinePink(string.Format(" {0} {1}", string.Format("{0},{1}", lineNo, count).PadRight(7), left));
                        WriteLinePink(string.Format("         {0}", right));
                        WriteLineYellow(string.Format("{0}{1}", "".PadRight(8 + count), "^"));

                        result = true;
                        break;
                    }
                }
                else if (eoLeft && !eoRight)
                {
                    WriteLineWhite("The LEFT line is shorter than the right");
                    result = true;
                }
                else if (eoRight && !eoLeft)
                {
                    WriteLineWhite("The RIGHT line is shorter than the left");
                    result = true;
                }
            }

            return result;
        }
    }
}
