using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Avalonia.Remote.Protocol;
using DOLogParser.DataStructures;
using DOLogParser.Models;

namespace DOLogParser.Services;

public class LogParserService
{
    public LogParserService(UserSettings userSettings)
    {
        _server = userSettings.Server;
        _doSid = userSettings.DoSid;
    }

    private static string _server;
    private string _doSid;
    private int _pageNumber;

    private string _balanceLogUrl =
        $"https://{_server}.darkorbit.com/indexInternal.es?action=internalBalance&orderBy=&view=&dps=";

    private string? _journalLogUrl = String.Empty;

    public async Task<List<LogRow>> GetLogsByPage(int currentPage)
    {
        string htmlPage = String.Empty;


        var httpClient = new HttpClient();
        string url =
            $"https://{_server}.darkorbit.com/indexInternal.es?action=internalBalance&orderBy=&view=&dps={currentPage}";

        httpClient.DefaultRequestHeaders.Add(nameof(Cookie), $"dosid={_doSid}");
        httpClient.DefaultRequestHeaders.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.3");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BigpointClient/1.6.9");

        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        htmlPage = await response.Content.ReadAsStringAsync();

        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        IDocument document = await context.OpenAsync(req => req.Content(htmlPage));

        IHtmlCollection<IElement> balanceRows = document.QuerySelectorAll("li.balance_item");

        List<LogRow> logRows = new();
        LogRow logRow;
        foreach (var row in balanceRows)
        {
            logRow = new LogRow()
            {
                Date = row.QuerySelector("span.date")!.InnerHtml,
                Description = row.QuerySelector("span.description")!.InnerHtml,
                Amount = row.QuerySelector("span.amount")!.InnerHtml,
                Page = $"Страница {currentPage}"
            };
            
            logRows.Add(logRow);
        }

        // var result = new List<string>();

        return logRows;
    }
}