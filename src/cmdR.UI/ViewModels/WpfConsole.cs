using System;
using cmdR.IO;

namespace cmdR.UI.ViewModels
{
    public class WpfConsole : ICmdRConsole
    {
        private readonly IWpfViewModel _viewmodel;

        public WpfConsole(IWpfViewModel viewModel)
        {
            _viewmodel = viewModel;

            Clear();
        }

        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _viewmodel.Output = "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\" LineHeight=\"1\">\n</Section>";
        }

        private string WrapText(string text, params object[] param)
        {
            text = text.StartsWith("<Run") ? string.Format(text, param) : string.Format("<Run>{0}</Run>", string.Format(text, param).XmlEscape());
            return text + "</Paragraph>";
        }

        public void Write(string line, params object[] param)
        {
            // we don't have a paragraph to append any text too, so just do a write line
            if (!_viewmodel.Output.Contains("<Paragraph>"))
                WriteLine(line, param);

            _viewmodel.Output = _viewmodel.Output.Replace("</Paragraph>\n</Section>", string.Format("{0}\n</Section>", WrapText(line, param)));
            _viewmodel.RaiseOutputChanged();
        }

        public void Write(string line)
        {
            // we don't have a paragraph to append any text too, so just do a write line
            if (!_viewmodel.Output.Contains("<Paragraph>"))
                WriteLine(line);

            _viewmodel.Output = _viewmodel.Output.Replace("</Paragraph>\n</Section>", string.Format("{0}\n</Section>", WrapText(line)));
            _viewmodel.RaiseOutputChanged();
        }

        public void WriteLine(string line, params object[] param)
        {
            _viewmodel.Output = _viewmodel.Output.Replace("</Section>", string.Format("\t<Paragraph>{0}\n</Section>", WrapText(line, param)));
            _viewmodel.RaiseOutputChanged();
        }

        public void WriteLine(string line)
        {
            _viewmodel.Output = _viewmodel.Output.Replace("</Section>", string.Format("\t<Paragraph>{0}\n</Section>", WrapText(line)));
            _viewmodel.RaiseOutputChanged();
        }
    }
}