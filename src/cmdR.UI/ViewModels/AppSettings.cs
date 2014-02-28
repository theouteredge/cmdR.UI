using System.Collections.Generic;

namespace cmdR.UI.ViewModels
{
    public class AppSettings
    {
        public string Path { get; set; }
        public List<string> Last10Commands { get; set; }
    }
}