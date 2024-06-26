﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using DOLogParser.DataStructures;
using DOLogParser.Enums;
using DOLogParser.Models;
using DynamicData;

namespace DOLogParser.Services;

public class LogParserService
{
    public LogParserService(ParserSettings parserSettings)
    {
        _server = parserSettings.UserSettings.Server;
        _doSid = parserSettings.UserSettings.DoSid;

        _logType = parserSettings.LogType;

        _balanceLogUrl = $"https://{_server}.darkorbit.com/indexInternal.es?action=internalBalance&orderBy=&view=&dps=";
        _historyLogUrl = $"https://{_server}.darkorbit.com/indexInternal.es?action=internalHistory&orderBy=&view=&dps=";
    }

    private string? _server;
    private string _doSid;

    private LogType _logType;

    private string _balanceLogUrl;
    private string? _historyLogUrl;

    public async Task<List<LogRow>> GetLogsByPage(int currentPage)
    {
        string url;
        string htmlPage;

        switch (_logType)
        {
            case LogType.Balance:
                url = $"{_balanceLogUrl}{currentPage}";
                htmlPage = await GetHtmlPage(url);

                return await GetFormattedBalanceData(htmlPage, currentPage);
            case LogType.History:
                url = $"{_historyLogUrl}{currentPage}";
                htmlPage = await GetHtmlPage(url);

                return await GetFormattedHistoryData(htmlPage, currentPage);
            default:
                return new List<LogRow>() { new() { Description = "Не Найдено" } };
        }
    }

    private async Task<string> GetHtmlPage(string url)
    {
        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add(nameof(Cookie), $"dosid={_doSid}");
        httpClient.DefaultRequestHeaders.Add("Accept-Charset", "windows-1251,utf-8;q=0.7,*;q=0.3");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BigpointClient/1.6.9");

        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string htmlPage = await response.Content.ReadAsStringAsync();

        return htmlPage;
    }

    private async Task<List<LogRow>> GetFormattedBalanceData(string htmlPage, int currentPage)
    {
        List<LogRow> logRows = new();

        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        IDocument document = await context.OpenAsync(req => req.Content(htmlPage));

        IHtmlCollection<IElement> balanceRows = document.QuerySelectorAll("li.balance_item");

        if (balanceRows.Length > 0)
        {
            logRows.AddRange(balanceRows
                .Select(row => new LogRow()
                {
                    Date = row.QuerySelector("span.date")!.InnerHtml,
                    Description = row.QuerySelector("span.description")!.InnerHtml,
                    Amount = row.QuerySelector("span.amount")!.InnerHtml,
                    Page = $"pg.{currentPage}"
                }));

            logRows.RemoveAt(0);
        }

        return logRows;
    }

    private async Task<List<LogRow>> GetFormattedHistoryData(string htmlPage, int currentPage)
    {
        List<LogRow> logRows = new();

        IBrowsingContext context = BrowsingContext.New(Configuration.Default);
        IDocument document = await context.OpenAsync(req => req.Content(htmlPage));

        IElement? historyTable = document.QuerySelector("table.fliess11px-grey");

        var historyRows = historyTable?.QuerySelectorAll("tr");

        if (historyRows != null && historyRows.Length > 0)
        {
            foreach (var row in historyRows)
            {
                var elements = row.Children.Where(x => !x.InnerHtml.Contains("<img"));
                if (elements.Any())
                {
                    logRows.Add(new LogRow()
                    {
                        Date = row.FirstElementChild.InnerHtml,
                        Description = row.LastElementChild.InnerHtml,
                        Page = $"pg.{currentPage}"
                    });
                
                }
            }
            logRows.RemoveAt(0);
        }
        
        return logRows;
    }
}