using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cmdR.UI.CmdRModules
{
    public class PartitionModule : ModuleBase, ICmdRModule
    {
        public PartitionModule(CmdR cmdR)
        {
            _cmdR = cmdR;

            _cmdR.RegisterRoute("part match output parts", PartitionFile, "Partitions all files which match into a given number of sub files");
            _cmdR.RegisterRoute("top match output take", Top, "Takes the top n rows from all files matching the regex");
            _cmdR.RegisterRoute("tail match output take", Tail, "Takes the n bottom rows from all the files matching the regex");
            _cmdR.RegisterRoute("top-tail match output take", TopAndTail, "Takes the n rows from the top and bottom of all the files which match the regex\n\t/middle take n items from the middle as well");
        }



        private void PartitionFile(IDictionary<string, string> param, CmdR cmdR)
        {
            var pathRegex = new Regex(param["path-match"]);
            var replace = param["output"];


            throw new NotImplementedException();
        }


        private void TopAndTail(IDictionary<string, string> param, CmdR cmdR)
        {
            var pathRegex = new Regex(param["match"]);
            var output = param["output"];
            var take = int.Parse(param["take"]);

            foreach (var file in Directory.GetFiles((string)_cmdR.State.Variables["path"]))
            {
                if (pathRegex.IsMatch(file))
                {
                    var count = File.ReadLines(file).Count();
                    var top = File.ReadLines(file).Take(take).ToList();
                    var tail = File.ReadLines(file).Skip(count - take).Take(take);

                    File.WriteAllLines(pathRegex.Replace(file, output), top.Union(tail));
                }
            }
        }

        private void Tail(IDictionary<string, string> param, CmdR cmdR)
        {
            var pathRegex = new Regex(param["match"]);
            var output = param["output"];
            var take = int.Parse(param["take"]);

            foreach (var file in Directory.GetFiles((string)_cmdR.State.Variables["path"]))
            {
                if (pathRegex.IsMatch(file))
                {
                    var count = File.ReadLines(file).Count();
                    var tail = File.ReadLines(file).Skip(count - take).Take(take);

                    File.WriteAllLines(pathRegex.Replace(file, output), tail);
                }
            }
        }

        private void Top(IDictionary<string, string> param, CmdR cmdR)
        {
            var pathRegex = new Regex(param["match"]);
            var output = param["output"];
            var take = int.Parse(param["take"]);

            foreach (var file in Directory.GetFiles((string)_cmdR.State.Variables["path"]))
            {
                if (pathRegex.IsMatch(file))
                {
                    var lines = File.ReadLines(file).Take(take).ToArray();
                    File.WriteAllLines(pathRegex.Replace(file, output), lines);
                }
            }
        }
    }
}
