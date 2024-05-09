namespace DOLogParser.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public LogParserViewModel LogParserViewModel { get; } = new();
}