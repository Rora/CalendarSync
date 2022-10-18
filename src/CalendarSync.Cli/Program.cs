// See https://aka.ms/new-console-template for more information
using CalendarSync.Cli.Dto;
using CalendarSync.Cli.PageObjects;
using CalendarSync.Cli.PageObjects.Auth;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Text.Json;

var optionsFilePath = "LocalSecrets.json";
if(!File.Exists(optionsFilePath))
{
    throw new InvalidOperationException($"Please create '{optionsFilePath}', content structure should match '{typeof(CalendarSyncOptions).FullName}'.");
}

using var optionsFileStream = File.OpenRead(optionsFilePath);
var options = await JsonSerializer.DeserializeAsync<CalendarSyncOptions>(optionsFileStream);

var webDriverOptions = new ChromeOptions();
//Opening the dev tools prevents 'tutorial tips' from popping up when opening the calendar
webDriverOptions.AddArguments("--auto-open-devtools-for-tabs");

var webDriver = new ChromeDriver(webDriverOptions);
webDriver.Url = "https://outlook.office.com/calendar/view/week";

var signInEnterEmailPage = new SignInEnterEmailPage(webDriver);
signInEnterEmailPage.Initialize();
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

var calendarWeekViewPage = new CalendarWeekViewPage(webDriver);
calendarWeekViewPage.Initialize();
calendarWeekViewPage.EnsureSingleCalendarIsSelected(options.Source.CalendarName);

Console.WriteLine("Done");
Console.ReadLine();