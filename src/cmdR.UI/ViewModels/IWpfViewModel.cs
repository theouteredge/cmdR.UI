namespace cmdR.UI.ViewModels
{
    public interface IWpfViewModel
    {
        string Command { get; set; }
        string Output { get; set; }
        string Prompt { get; set; }

        void RaiseOutputChanged();
    }
}