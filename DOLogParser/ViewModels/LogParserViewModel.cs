using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using DOLogParser.DataStructures;
using DOLogParser.Models;
using DOLogParser.Services;
using DynamicData;
using ReactiveUI;

namespace DOLogParser.ViewModels;

public class LogParserViewModel : ViewModelBase
{
    private string _selectedServer = String.Empty;
    private string _dosid = String.Empty;


    public LogParserViewModel()
    {
        SelectedServer = ServersList[0];
        
        SearchCommand = ReactiveCommand.CreateFromTask((() =>
        {
            SearchInLogs();
            return Task.CompletedTask;
        }));
    }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }

    private async void SearchInLogs()
    {
        var userSettings = new UserSettings()
        {
            DoSid =  DoSID,
            Server = SelectedServer,
        };

        var logParserService = new LogParserService(userSettings);

        for (int currentPage = Convert.ToInt32(FirstPage); currentPage <= Convert.ToInt32(LastPage); currentPage++)
        {
            var logs = await logParserService.GetLogsByPage(currentPage);
            
            Thread.Sleep(1000);
            // Debug.WriteLine("New Row from SearchInLogs");
            MatchedLogRows.Add(logs);
        }

    }

    public ObservableCollection<LogRow> MatchedLogRows { get; set; } = new();

    public ObservableCollection<string> ServersList { get; } = new ObservableCollection<string>
        { "ru1", "ru2", "ru3", "ru4", "ru5" };

    public string FirstPage { get; set; } = "1";
    public string LastPage { get; set; } = 2.ToString();

    public string SelectedServer
    {
        get => _selectedServer;
        set => this.RaiseAndSetIfChanged(ref _selectedServer, value);
    }

    public string DoSID
    {
        get => _dosid;
        set => this.RaiseAndSetIfChanged(ref _dosid, value);
    }
}