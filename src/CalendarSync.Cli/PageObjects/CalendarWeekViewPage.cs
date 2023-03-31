using CalendarSync.Cli.PageObjects.AddCalendarItem;
using CalendarSync.Cli.PageObjects.CalendarEvent;
using CalendarSync.Cli.Selenium;
using Microsoft.Extensions.Logging;
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
    internal class CalendarWeekViewPage : PageComponentBase
    {
        private const string AddCalendarItemButtonSelector = "div[data-unique-id=\"RibbonBottomBarContainer\"] div[data-unique-id=\"Ribbon-2545\"] button[data-unique-id=\"Ribbon-2545\"]";

        private const string ToggleLeftPaneButtonSelector = "button[data-automation-type=\"RibbonButton\"] i[data-icon-name=\"LineHorizontal3Regular\"]";
        private const string ShowAllOrSelectedCalendarsToggleSelector = "#leftPaneContainer div[role=\"complementary\"] div[role=\"listbox\"] > button";
        private const string CalendarButtonsSelector = "#leftPaneContainer div[role=\"complementary\"] div[role=\"listbox\"] button[role=\"option\"]";
        private const string LoadCalendarSpinnerSelector = "#leftPaneContainer div[role=\"complementary\"] div[role=\"listbox\"] button[role=\"option\"] .ms-Spinner-circle.ms-Spinner--small";
        private const string CalanderWeekNavigationDropdownIconSelector = "div[data-app-section=\"CalendarModuleNavigationBar\"] > button.ms-Button.ms-Button--action.ms-Button--command i[data-icon-name=\"ChevronDown\"]";

        private const string SingleDayCalendarItemSelector = "div[data-app-section=\"calendar-view-0\"] div.calendar-SelectionStyles-resizeBoxParent";
        private const string SingleAndMultiDayCalendarItemSelector = "div[data-app-section=\"calendar-view-0\"] div.calendar-SelectionStyles-resizeBoxParent, div[data-app-section=\"calendar-view-header-0\"] div.calendar-SelectionStyles-resizeBoxParent";
        private const string CalendarItemColorMarkerSelector = "div[role=\"button\"] div:first-child";

        private const string CalendarItemCardTimeSelector = "div[data-app-section=\"CalendarItemPeek\"] span[aria-label=\"Time\"]";
        private const string CalendarItemCardFullScreenButtonSelector = "div[data-app-section=\"CalendarItemPeek\"] i[data-icon-name=\"FullScreen\"]";
        private const string CalendarItemModalSelector = ".ms-Dialog-main div[data-app-section=\"ReadingPane\"]";
        private const string CalendarItemModalTimeIconSelector = "i[data-icon-name=\"ClockRegular\"]";
        private const string CalendarItemModalCalendarIconSelector = "i[data-icon-name=\"CalendarEmptyRegular\"]";
        private const string CalendarItemModalDescriptionIconSelector = "i[data-icon-name=\"TextboxRegular\"]";
        private readonly CalendarItemDateTimeSpanParser _calendarItemDateTimeSpanParser;
        private string? _calendarColor;
        private DateOrderEnum? _dateOrder;

        private string CalendarColor
        {
            get => _calendarColor ?? throw new InvalidOperationException($"Call {EnsureSingleCalendarIsSelected} first to set this property");
            set => _calendarColor = value;
        }

        public CalendarWeekViewPage(IWebDriver driver,
            CalendarItemDateTimeSpanParser calendarEventDateTimeSpanParser)
            : base(driver)
        {
            _calendarItemDateTimeSpanParser = calendarEventDateTimeSpanParser;
        }

        public void Initialize(CancellationToken ct = default)
        {
            WaitForElement(ToggleLeftPaneButtonSelector, ct);
        }

        public void EnsureSingleCalendarIsSelected(string calendarName)
        {
            calendarName = calendarName.ToLower();

            var calendarButtons = OpenLeftPaneAndGetCalendarButtons();
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
                Thread.Sleep(100);
                WaitForElementToVanish(LoadCalendarSpinnerSelector);
                Thread.Sleep(100);
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
            var toggleLeftPaneButton = WaitForElement(ToggleLeftPaneButtonSelector);
            toggleLeftPaneButton.ClickViaJS();

            //Wait for the calendar items to switch
            WaitUntilSingleDayCalendarItemsMatchInColor();
        }

        private IEnumerable<IWebElement> OpenLeftPaneAndGetCalendarButtons()
        {
            var retryNr = 0;
            while (true)
            {
                try
                {
                    var toggleLeftPaneButton = WaitForElement(ToggleLeftPaneButtonSelector);

                    //For some reason the first click doesn't always work
                    toggleLeftPaneButton.ClickViaJS();
                    var calendarButtons = WaitForElements(CalendarButtonsSelector, TimeSpan.FromSeconds(30),
                        msBetweenTries: 100,
                        actionBetweenTries: (_) => toggleLeftPaneButton.ClickViaJS());

                    return calendarButtons;
                }
                catch (StaleElementReferenceException)
                {
                    retryNr++;
                    Console.WriteLine($"Failed to expand left pane because of stale element reference exception, retry nr {retryNr}");

                    if (retryNr > 10)
                    {
                        throw;
                    }
                }
            }
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

        public IEnumerable<CalendarItemDto> GetCalendarItems()
        {
            var calendarItemElements = GetCalendarItemElements();

            if (!calendarItemElements.Any())
            {
                return Enumerable.Empty<CalendarItemDto>();
            }

            var calendarItems = new List<CalendarItemDto>();
            var retryCount = 0;

            //TODO throw error 
            while (calendarItems.Count < 1000
                && retryCount < 100)
            {
                try
                {
                    for (int i = 0; i < calendarItemElements.Count; i++)
                    {
                        var calendarItemElement = calendarItemElements[i];
                        var calendarItemId = calendarItemElement.GetAttribute("data-calitemid");

                        //Skip already handled items
                        if (calendarItems.Any(ce => ce.CalendarItemId == calendarItemId))
                        {
                            continue;
                        }

                        var calendarItem = OpenAndParseCalendarItem(calendarItemElement, calendarItemId);
                        calendarItems.Add(calendarItem);
                        CloseCalendarItem();
                    }

                    //When done, exit this loop
                    break;
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    Console.WriteLine($"Retrying to read calendar items because of stale element exception. Retry count {retryCount}");

                    //Refetch these on every try to prevent stale elements
                    calendarItemElements = GetCalendarItemElements();
                }
                catch (ElementClickInterceptedException)
                {
                    Console.WriteLine($"Reading calendar item failed because of an {nameof(ElementClickInterceptedException)}, trying to close any unrelated panel");

                    if (!TryCloseAnyUnrelatedPopIn())
                    {
                        Console.WriteLine("Could not find any unrelated panel to close");
                    }

                    retryCount++;
                    Console.WriteLine($"Closed an unrelated panel, retrying. Retry count {retryCount}");
                }
            }

            return calendarItems;
        }

        private bool TryCloseAnyUnrelatedPopIn()
        {
            var ringerOffIcons = _webDriver.FindElements(By.CssSelector("i[data-icon-name=\"RingerOff\"]"));
            if (!ringerOffIcons.Any())
            {
                return false;
            }

            var reminderPanel = ringerOffIcons[0].GetParentElement(7);
            reminderPanel.FindElement(By.CssSelector("i[data-icon-name=\"Cancel\"]"));
            reminderPanel.Click();
            return true;
        }

        private void CloseCalendarItem()
        {
            var action = new Actions(_webDriver);
            action.SendKeys(Keys.Escape).Build().Perform();
            WaitForElementToVanish(CalendarItemModalSelector);
        }

        private CalendarItemDto OpenAndParseCalendarItem(IWebElement calendarItemElement, string calendarItemId)
        {
            calendarItemElement.Click();

            InitializeCalendarItemCard();

            var calendarEventCardFullScreenButton = WaitForElement(CalendarItemCardFullScreenButtonSelector);
            calendarEventCardFullScreenButton.Click();
            var calendarEventModal = WaitForElement(CalendarItemModalSelector);

            var name = GetName(calendarEventModal);
            var dateTimeText = GetTextByIconElement(calendarEventModal, CalendarItemModalTimeIconSelector);
            var calendarName = GetTextByIconElement(calendarEventModal, CalendarItemModalCalendarIconSelector);
            var descriptionText = GetTextByIconElement(calendarEventModal, CalendarItemModalDescriptionIconSelector, isOptional: true);

            (var startDateTime, var endDateTime, var isAllDayItem) = _calendarItemDateTimeSpanParser.ParseDateTimeSpan(_dateOrder!.Value, dateTimeText);
            return new CalendarItemDto(calendarItemId, name, calendarName, descriptionText, startDateTime, endDateTime, IsAllDay: false);

            static string GetName(IWebElement calendarEventModal)
            {
                const string NameValueCssSelector = "span.allowTextSelection";

                var secondRowIcon = calendarEventModal.FindElement(By.CssSelector(CalendarItemModalTimeIconSelector));
                var rowsParent = secondRowIcon.GetParentElement().GetParentElement().GetParentElement().GetParentElement();
                var nameRow = rowsParent.GetChildren().First().FindElement(By.CssSelector(NameValueCssSelector));
                return nameRow.Text;
            }

            static string GetTextByIconElement(IWebElement calendarEventModal,
                string iconSelector, bool isOptional = false)
            {
                const string ValueCssSelector = ".allowTextSelection > div";

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
                var calendarTimeElement = WaitForElement(CalendarItemCardTimeSelector);
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

        private ReadOnlyCollection<IWebElement> WaitUntilSingleDayCalendarItemsMatchInColor()
        {
            ReadOnlyCollection<IWebElement> calendarEventEls;
            var timeOut = DateTime.Now + TimeSpan.FromSeconds(10);
            do
            {
                calendarEventEls = _webDriver.FindElements(By.CssSelector(SingleDayCalendarItemSelector));
                if (calendarEventEls
                   .All(e => e.FindElement(By.CssSelector(CalendarItemColorMarkerSelector))
                              .GetCssValue("background-color") == CalendarColor))
                {
                    Thread.Sleep(600); //Don't return if the UI is still rendering
                    if (calendarEventEls.First().IsStale())
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

        private ReadOnlyCollection<IWebElement> GetCalendarItemElements()
        {
            return _webDriver.FindElements(By.CssSelector(SingleAndMultiDayCalendarItemSelector));
        }

        public AddCalendarItemDialog OpenAddCalendarItemDialog()
        {
            var openAddCalendarItemDialogButton = WaitForElement(AddCalendarItemButtonSelector);
            openAddCalendarItemDialogButton.Click();
            return new AddCalendarItemDialog(_webDriver).Initialize();
        }
    }
}
