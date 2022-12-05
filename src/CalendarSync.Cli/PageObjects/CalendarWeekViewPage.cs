using CalendarSync.Cli.PageObjects.CalendarEvent;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects
{
    internal class CalendarWeekViewPage : PageBase
    {
        private const string ToggleLeftPaneButtonSelector = "button[data-automation-type=\"RibbonButton\"] i[data-icon-name=\"LineHorizontal3Regular\"]";
        private const string ShowAllOrSelectedCalendarsToggleSelector = "#leftPaneContainer div[role=\"complementary\"] div[role=\"listbox\"] > button";
        private const string CalendarButtonsSelector = "#leftPaneContainer div[role=\"complementary\"] div[role=\"listbox\"] button[role=\"option\"]";
        private const string CalanderWeekNavigationDropdownIconSelector = "div[data-app-section=\"CalendarModuleNavigationBar\"] > button.ms-Button.ms-Button--action.ms-Button--command i[data-icon-name=\"ChevronDown\"]";


        private const string CalendarEventSelector = "div[data-app-section=\"calendar-view-0\"] div.calendar-SelectionStyles-resizeBoxParent";
        private const string CalendarEventColorMarkerSelector = "div[role=\"button\"] div:first-child";
        private const string CalendarHeaderDayNrSelector = "div[data-app-section=\"calendar-view-header-0\"] div[data-tabid=\"surfaceHeader_{0}\"] time";

        private const string CalendarEventCardTimeSelector = "div[data-app-section=\"CalendarItemPeek\"] span[aria-label=\"Time\"]";
        private const string CalendarEventCardFullScreenButtonSelector = "div[data-app-section=\"CalendarItemPeek\"] i[data-icon-name=\"FullScreen\"]";
        private const string CalendarEventModalSelector = ".ms-Dialog-main div[data-app-section=\"ReadingPane\"]";
        private const string CalendarEventModalTimeIconSelector = "i[data-icon-name=\"ClockRegular\"]";
        private const string CalendarEventModalDescriptionIconSelector = "i[data-icon-name=\"TextboxRegular\"]";
        private readonly CalendarEventDateTimeSpanParser _calendarEventDateTimeSpanParser;
        private string? _calendarColor;
        private DateOrderEnum? _dateOrder;

        private string CalendarColor
        {
            get => _calendarColor ?? throw new InvalidOperationException($"Call {EnsureSingleCalendarIsSelected} first to set this property");
            set => _calendarColor = value;
        }

        public CalendarWeekViewPage(IWebDriver driver, CalendarEventDateTimeSpanParser calendarEventDateTimeSpanParser)
            : base(driver)
        {
            _calendarEventDateTimeSpanParser = calendarEventDateTimeSpanParser;
        }

        public void Initialize(CancellationToken ct = default)
        {
            WaitForElement(ToggleLeftPaneButtonSelector, ct);
        }

        public void EnsureSingleCalendarIsSelected(string calendarName)
        {
            calendarName = calendarName.ToLower();
            var toggleLeftPaneButton = WaitForElement(ToggleLeftPaneButtonSelector);

            //For some reason the first click doesn't always work
            toggleLeftPaneButton.ClickViaJS();
            var calendarButtons = WaitForElements(CalendarButtonsSelector, TimeSpan.FromSeconds(30),
                msBetweenTries: 100,
                actionBetweenTries: (_) => toggleLeftPaneButton.ClickViaJS());


            var calendarButtonToSelect = calendarButtons.FirstOrDefault(cb => cb.GetAttribute("title").ToLower() == calendarName);

            if (calendarButtonToSelect == null)
            {
                WaitForElement(ShowAllOrSelectedCalendarsToggleSelector).ClickViaJS();
                calendarButtons = WaitForElements(CalendarButtonsSelector);
                calendarButtonToSelect = calendarButtons.FirstOrDefault(cb => cb.GetAttribute("title").ToLower() == calendarName);

                if (calendarButtonToSelect == null)
                {
                    throw new InvalidOperationException($"Could not find calendar '{calendarName}' to select");
                }
            }

            //Ensure that the calendar is selected
            if (calendarButtonToSelect.GetAttribute("aria-selected") != "true")
            {
                var calendarTitle = calendarButtonToSelect.GetAttribute("title");
                calendarButtonToSelect.ClickViaJS();
            }

            CalendarColor = WaitForElements("i", searchContext: calendarButtonToSelect).Single().GetCssValue("background-color");

            //Unselect the other buttons
            var calendarButtonsToUnSelect = calendarButtons
                .Where(cb => cb.GetAttribute("aria-selected") == "true" && cb.GetAttribute("title").ToLower() != calendarName)
                .ToArray();

            foreach (var calendarButton in calendarButtonsToUnSelect)
            {
                calendarButton.ClickViaJS();
            }

            //Collapse the pane when done
            toggleLeftPaneButton.ClickViaJS();
        }

        public void ReadDateOrder()
        {
            var dropdownIconEl = WaitForElement(CalanderWeekNavigationDropdownIconSelector);
            var currentCalendarViewPeriodText = dropdownIconEl.GetSiblings().Single().Text;

            //Matches 28 November – 4 December, 2022
            if (Regex.IsMatch(currentCalendarViewPeriodText, @"^\d+\s+\w+\s+[^\s]\s\d+\s\w+"))
            {
                _dateOrder = DateOrderEnum.DayMonthYear;
                return;
            }
            
            //Matches November 28 – December 4, 2022
            if (Regex.IsMatch(currentCalendarViewPeriodText, @"^\w+\s+\d+\s+[^\s]\s+\w+\s\d+"))
            {
                _dateOrder = DateOrderEnum.MonthDayYear;
                return;
            }

            //Matches 5–11 December, 2022
            if (Regex.IsMatch(currentCalendarViewPeriodText, @"^\d+[^\s^\d]+?\d+\s\w+,{0,1}\s\d{4}"))
            {
                _dateOrder = DateOrderEnum.DayMonthYear;
                return;
            }

            //Matches December 5–11, 2022
            if (Regex.IsMatch(currentCalendarViewPeriodText, @"^\w+\s\d+[^\d^\s]\d+,\s\d{4}"))
            {
                _dateOrder = DateOrderEnum.MonthDayYear;
                return;
            }

            throw new InvalidOperationException("Could not parse '{currentCalendarViewPeriodText}' to decide wether the month or the day comes first");
        }

        public void GetCalanderEvents()
        {
            //We're hoping it won't turn stale after this call which isn't reliable
            //better to keep track of last 'time' we handled and reload events on stale
            var calendarEventEls = GetCalendarEvents();

            if (!calendarEventEls.Any())
            {
                return;
            }

            var calendarEvents = new List<CalendarEventDto>();

            //TODO refresh elements after every iteration and skip ones already analyzed by id
            //TODO retry on stale element exc
            foreach (var calendarEvent in calendarEventEls)
            {
                var calendarItemId = calendarEvent.GetAttribute("data-calitemid");

                calendarEvent.Click();

                InitializeCalendarItemCard();

                var calendarEventCardFullScreenButton = WaitForElement(CalendarEventCardFullScreenButtonSelector);
                calendarEventCardFullScreenButton.Click();
                var calendarEventModal = WaitForElement(CalendarEventModalSelector);

                var name = GetName(calendarEventModal);
                var dateTimeText = GetTextByIconElement(calendarEventModal, CalendarEventModalTimeIconSelector);
                var descriptionText = GetTextByIconElement(calendarEventModal, CalendarEventModalDescriptionIconSelector, isOptional: true);

                (var startDateTime, var endDateTime) = _calendarEventDateTimeSpanParser.ParseDateTimeSpan(_dateOrder!.Value, dateTimeText);
                calendarEvents.Add(new CalendarEventDto(calendarItemId, name, descriptionText, startDateTime, endDateTime, IsAllDay: false));

                var action = new Actions(_driver);
                action.SendKeys(Keys.Escape).Build().Perform();
                WaitForElementToVanish(CalendarEventModalSelector);
            }

            const string ValueCssSelector = ".allowTextSelection > div";
            const string NameValueCssSelector = "span.allowTextSelection";

            static string GetName(IWebElement calendarEventModal)
            {
                var secondRowIcon = calendarEventModal.FindElement(By.CssSelector(CalendarEventModalTimeIconSelector));
                var rowsParent = secondRowIcon.GetParentElement().GetParentElement().GetParentElement().GetParentElement();
                var nameRow = rowsParent.GetChildren().First().FindElement(By.CssSelector(NameValueCssSelector));
                return nameRow.Text;
            }

            static string GetTextByIconElement(IWebElement calendarEventModal,
                string iconSelector, bool isOptional = false)
            {
                var icons = calendarEventModal.FindElements(By.CssSelector(iconSelector));
                if (!icons.Any())
                {
                    return isOptional
                        ? String.Empty
                        : throw new InvalidOperationException($"Could not find icon using {iconSelector}");
                }

                var row = icons.Single().GetParentElement().GetParentElement().GetParentElement();
                var textColumn = row.FindElement(By.CssSelector(ValueCssSelector));
                return textColumn.Text;
            }
        }

        private void InitializeCalendarItemCard()
        {
            var timeOut = DateTime.Now + TimeSpan.FromSeconds(10);
            var succes = false;
            do
            {
                var calendarTimeElement = WaitForElement(CalendarEventCardTimeSelector);
                if (!string.IsNullOrWhiteSpace(calendarTimeElement.Text))
                {
                    succes = true;
                    break;
                }

                Thread.Sleep(50);
            }
            while (DateTime.Now < timeOut);

            if (!succes)
            {
                throw new InvalidOperationException("Timeout expired before conditions were met");
            }
        }

        private ReadOnlyCollection<IWebElement> GetCalendarEvents()
        {
            ReadOnlyCollection<IWebElement> calendarEventEls;
            var timeOut = DateTime.Now + TimeSpan.FromSeconds(10);
            do
            {
                calendarEventEls = _driver.FindElements(By.CssSelector(CalendarEventSelector));
                if (calendarEventEls.Any() &&
                   calendarEventEls.All(e => e.FindElement(By.CssSelector(CalendarEventColorMarkerSelector)).GetCssValue("background-color") == CalendarColor))
                {
                    Thread.Sleep(600); //Don't return if the UI is still rendering
                    if(calendarEventEls.First().IsStale())
                    {
                        continue;
                    }

                    break;
                }
                Thread.Sleep(50);
            }
            while (DateTime.Now < timeOut);
            return calendarEventEls;
        }
    }
}
