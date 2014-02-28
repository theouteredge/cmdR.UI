using System.Text.RegularExpressions;

namespace cmdR.UI.CmdRModules
{
    public class RegexMatchAndReplace
    {
        public string Match { get; set; }
        public string Replace { get; set; }

        public Regex GetRegex()
        {
            return new Regex(this.Match);
        }
    }
}