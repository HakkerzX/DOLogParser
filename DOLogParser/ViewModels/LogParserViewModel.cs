using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using DOLogParser.DataStructures;
using DOLogParser.Enums;
using DOLogParser.Models;
using DOLogParser.Services;
using DynamicData;
using ReactiveUI;

namespace DOLogParser.ViewModels;

public class LogParserViewModel : ViewModelBase
{
    private string _selectedServer;
    private string _doSid;

    private string _firstPage;
    private string _lastPage;

    private bool _historyIsChecked;
    private bool _balanceIsChecked = true;

    private string _searchText;

    private LogType _logType;


    public LogParserViewModel()
    {
        MatchedLogRows = new ObservableCollection<LogRow>();

        LogType = LogType.Balance;
        FirstPage = "1";
        LastPage = "1";
        SearchText = "";

        this.WhenAnyValue(x => x.FirstPage)
            .Subscribe(text =>
            {
                if (!string.IsNullOrEmpty(text) && !text.All(char.IsDigit))
                {
                    FirstPage = new string(text.Where(char.IsDigit).ToArray());
                }
            });
        this.WhenAnyValue(x => x.LastPage)
            .Subscribe(text =>
            {
                if (!string.IsNullOrEmpty(text) && !text.All(char.IsDigit))
                {
                    LastPage = new string(text.Where(char.IsDigit).ToArray());
                }
            });

        SelectedServer = ServersList[0];
        var isValidObservable = this.WhenAnyValue(
            x => x.DoSID,
            x => !string.IsNullOrWhiteSpace(x));
        SearchCommand = ReactiveCommand.CreateFromTask(async () => { await Task.Run(SearchInLogs); }
            , isValidObservable);
        SelectHistoryCommand = ReactiveCommand.Create(SelectHistory);
        SelectBalanceCommand = ReactiveCommand.Create(SelectBalance);
    }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectHistoryCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectBalanceCommand { get; }


    private async void SearchInLogs()
    {
        var userSettings = new UserSettings(DoSID, SelectedServer);
        var parserSettings = new ParserSettings(userSettings, LogType);

        var logParser = new LogParserService(parserSettings);

        for (int currentPage = Convert.ToInt32(FirstPage); currentPage <= Convert.ToInt32(LastPage); currentPage++)
        {
            var logs = await logParser.GetLogsByPage(currentPage);

            if (logs.Count > 1)
            {
                var result = logs.Where(x => x.Description.Contains(SearchText));

                MatchedLogRows.Add(result);

                Thread.Sleep(1000);
            }

            else
            {
                MatchedLogRows.Add(new LogRow()
                {
                    Description = "Не найдено"
                });
                return;
            }
        }
    }

    private void SelectHistory()
    {
        HistoryIsChecked = true;
        BalanceIsChecked = false;

        LogType = LogType.History;
    }

    private void SelectBalance()
    {
        BalanceIsChecked = true;
        HistoryIsChecked = false;

        LogType = LogType.Balance;
    }

    public ObservableCollection<LogRow> MatchedLogRows { get; }

    public ObservableCollection<string> ServersList { get; } = new() { "ru1", "ru2", "ru3", "ru4", "ru5" };

    public string FirstPage
    {
        get => _firstPage;
        set => this.RaiseAndSetIfChanged(ref _firstPage, value);
    }

    public string LastPage
    {
        get => _lastPage;
        set => this.RaiseAndSetIfChanged(ref _lastPage, value);
    }

    public string SelectedServer
    {
        get => _selectedServer;
        set => this.RaiseAndSetIfChanged(ref _selectedServer, value);
    }

    public string DoSID
    {
        get => _doSid;
        set => this.RaiseAndSetIfChanged(ref _doSid, value);
    }

    public bool HistoryIsChecked
    {
        get => _historyIsChecked;
        set => this.RaiseAndSetIfChanged(ref _historyIsChecked, value);
    }

    public bool BalanceIsChecked
    {
        get => _balanceIsChecked;
        set => this.RaiseAndSetIfChanged(ref _balanceIsChecked, value);
    }

    public LogType LogType
    {
        get => _logType;
        private set => this.RaiseAndSetIfChanged(ref _logType, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }
}