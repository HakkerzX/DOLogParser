namespace DOLogParser.Models;

public class UserSettings(string doSid, string? server)
{
    public string? Server { get; set; } = server;
    public string DoSid { get; set; } = doSid;
}