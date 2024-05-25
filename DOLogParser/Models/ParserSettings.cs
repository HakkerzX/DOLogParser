using DOLogParser.Enums;
using DOLogParser.ViewModels;

namespace DOLogParser.Models;

public class ParserSettings(UserSettings userSettings, LogType logType)
{
    public UserSettings UserSettings { get; set; } = userSettings;
    public LogType LogType { get; set; } = logType;
}