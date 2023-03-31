// See https://aka.ms/new-console-template for more information
using CalendarSync.Cli.Dto;
using CalendarSync.Cli.FileLock;
using CalendarSync.Cli.PageObjects;
using CalendarSync.Cli.PageObjects.Auth;
using CalendarSync.Cli.PageObjects.CalendarEvent;
using CalendarSync.Cli.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.Text.Json;

var optionsFilePath = "LocalSecrets.json";
if(!File.Exists(optionsFilePath))
{
    throw new InvalidOperationException($"Please create '{optionsFilePath}', content structure should match '{typeof(CalendarSyncOptions).FullName}'.");
}

//Read options
using var optionsFileStream = File.OpenRead(optionsFilePath);
var options = await JsonSerializer.DeserializeAsync<CalendarSyncOptions>(optionsFileStream)
    ?? throw new InvalidOperationException($"Could not parse '{optionsFilePath}'");

var userDataDir = Path.Combine(Directory.GetCurrentDirectory(), "data", "source-cal-selenium-data");
Directory.CreateDirectory(userDataDir);

SeleniumProcessKiller.KillExistingSeleniumProcessesForProfile(userDataDir);

var webDriverOptions = new ChromeOptions();
//Opening the dev tools prevents 'tutorial tips' from popping up when opening the calendar
//webDriverOptions.AddArguments("--auto-open-devtools-for-tabs");
webDriverOptions.AddArguments(@"user-data-dir=" + userDataDir);
var webDriver = new ChromeDriver(webDriverOptions);

try
{
    IWebElementExtensions.WebDriver = webDriver;
    webDriver.Url = "https://outlook.office.com/calendar/view/week";

    var lang = Language.English;
    var calendarEventDateTimeSpanParser = new CalendarItemDateTimeSpanParser(lang);

    //Depending if the user is already logged in, or logged in because of their windows identity
    //the initial request might turn into a sign in page or not
    var cts = new CancellationTokenSource();

    var signInEnterEmailPageTask = Task.Run(() =>
    {
        var signInEnterEmailPage = new SignInEnterEmailPage(webDriver);
        signInEnterEmailPage.Initialize(cts.Token);
        return signInEnterEmailPage;
    });

    var calendarWeekViewPageTask = Task.Run(() =>
    {
        var calendarWeekViewPage = new CalendarWeekViewPage(webDriver, calendarEventDateTimeSpanParser);
        calendarWeekViewPage.Initialize(cts.Token);
        return calendarWeekViewPage;
    });
    var firstTaskToComplete = await Task.WhenAny(signInEnterEmailPageTask, calendarWeekViewPageTask);
    cts.Cancel(); //Cancel the other page wait

    CalendarWeekViewPage calendarWeekViewPage;

    if (firstTaskToComplete == signInEnterEmailPageTask)
    {
        var signInEnterEmailPage = signInEnterEmailPageTask.Result;
        signInEnterEmailPage.SubmitEmailAddress(options.Source.Username);
        Console.WriteLine("Sent source calendar username");

        var signInEnterPasswordPage = new SignInEnterPasswordPage(webDriver);
        signInEnterPasswordPage.Initialize();
        signInEnterPasswordPage.SubmitPassword(options.Source.Password);
        Console.WriteLine("Sent source calendar password");

        var waitingForMfaPage = new WaitingForMfaPage(webDriver);
        waitingForMfaPage.Initialize();
        Console.WriteLine("Waiting for source calendar MFA");
        var sw = new Stopwatch();
        sw.Start();
        waitingForMfaPage.WaitForMfaToCompleteOrTimeout();
        sw.Stop();
        Console.WriteLine($"Source calendar MFA completed or timed. Time taken: {sw.Elapsed.ToString()}");

        var keepMeSignedInPage = new KeepMeSignedInPage(webDriver);
        keepMeSignedInPage.Initialize();
        keepMeSignedInPage.SubmitYes();
        Console.WriteLine("Sent remind me yes for source calendar login");

        calendarWeekViewPage = new CalendarWeekViewPage(webDriver, calendarEventDateTimeSpanParser);
        calendarWeekViewPage.Initialize();
    }
    else
    {
        calendarWeekViewPage = calendarWeekViewPageTask.Result;
    }

    calendarWeekViewPage.EnsureSingleCalendarIsSelected(options.Source.CalendarName);
    calendarWeekViewPage.ReadDateOrder();
    var calendarItems = calendarWeekViewPage.GetCalendarItems();
    if (calendarItems.Any(ci => ci.CalendarName != options.Source.CalendarName))
    {
        var otherCalendarNames = calendarItems
            .Where(ci => ci.CalendarName != options.Source.CalendarName)
            .Select(ci => ci.CalendarName)
            .Distinct()
            .ToArray();
        throw new InvalidOperationException($"Found calendar items of another calendar than the one that should've been selected. (expected: '{options.Source.CalendarName}', unexpected findings: '{string.Join(", ", otherCalendarNames)}'");
    }

    var addCalendarDialog = calendarWeekViewPage.OpenAddCalendarItemDialog();

    if(addCalendarDialog.SelectedCalendarName != options.Source.CalendarName)
    {
        throw new InvalidOperationException($"Selected calendar {addCalendarDialog.SelectedCalendarName} did not match expected calendar name {options.Source.CalendarName}");
    }

    Console.WriteLine("Done");
}
finally
{
    webDriver.Dispose();
}
//Console.ReadLine();
