using CalendarSync.Cli.Selenium;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects
{
    internal class DatePicker : PageComponentBase
    {
        private const string DatePickerDialogSelector = ".ms-Callout.ms-DatePicker-callout div[role=\"dialog\"] div[role=\"group\"]";
        private const string DatesContainerSelector = ".ms-FocusZone";
        private const string MonthAndYearSpanSelector = "button[data-is-focusable=\"true\"] > span";
        private const string GoToTodayButtonSelector = "button.js-goToday";
        private const string PreviousMonthButtonSelector = "i[data-icon-name=\"Up\"]";
        private const string NextMonthButtonSelector = "i[data-icon-name=\"Down\"]";
        private IWebElement? _datePickerDialog;
        private IWebElement? _datesContainer;
        private IWebElement? _monthSelector;
        private IWebElement? _prevMonthButton;
        private IWebElement? _nextMonthButton;

        public DatePicker(IWebDriver webDriver) : base(webDriver)
        {
        }

        public DatePicker Initialize()
        {
            _datePickerDialog = WaitForElement(DatePickerDialogSelector);
            _datesContainer = WaitForElement(DatesContainerSelector, searchContext: _datePickerDialog);
            _monthSelector = _datesContainer.GetSiblings().Single(el => el != _datesContainer);
            _prevMonthButton = _monthSelector!.FindElement(By.CssSelector(PreviousMonthButtonSelector));
            _nextMonthButton = _monthSelector!.FindElement(By.CssSelector(NextMonthButtonSelector));
            WaitForMonthAndYearToBeSet();

            return this;
        }

        public void SelectDate(DateOnly date)
        {
            NavigateToYearAndMonth(date);
            ClickDayOfSelectedMonth(date.Day);
        }

        private void NavigateToYearAndMonth(DateOnly date)
        {
            GoToToday();
            var nowDate = DateOnly.FromDateTime(DateTime.Now);

            var yearDiff = date.Year - nowDate.Year;
            var monthDiff = date.Month - nowDate.Month;

            var totalMonthsDiff = yearDiff * 12 + monthDiff;

            if (totalMonthsDiff > 0)
            {
                for (int i = 0; i < totalMonthsDiff; i++)
                {
                    GoToNextMonth();
                }
            }
            else
            {
                for (int i = 0; i > totalMonthsDiff; i--)
                {
                    GoToPreviousMonth();
                }
            }
        }

        public void GoToPreviousMonth()
        {
            var currentMonthAndYear = WaitForMonthAndYearToBeSet();
            _prevMonthButton!.Click();

            WaitFor(() =>
            {
                var monthAndYear = WaitForMonthAndYearToBeSet();
                //Wait until the month changed
                return (monthAndYear[0] != currentMonthAndYear[0], 0);
            });
        }

        public void GoToNextMonth()
        {
            var currentMonthAndYear = WaitForMonthAndYearToBeSet();
            _nextMonthButton!.Click();

            WaitFor(() =>
            {
                var monthAndYear = WaitForMonthAndYearToBeSet();
                //Wait until the month changed
                return (monthAndYear[0] != currentMonthAndYear[0], 0);
            });
        }

        public void GoToToday()
        {
            var goToTodayButton = WaitForElement(GoToTodayButtonSelector, searchContext: _datePickerDialog);
            goToTodayButton.ClickViaJS();
        }

        public void ClickDayOfSelectedMonth(int dayNr)
        {
            var dayButtons = _datesContainer!.FindElements(By.CssSelector("button > span"))
                //Day nrs of a datepicker show week rows, they might contain dates of the prev month
                .SkipWhile(el => el.GetTextContent().Trim() != "1")
                //Day nrs of a datepicker show week rows, they might contain dates of the next month
                .TakeWhile((el, index) => index == 0 || el.GetTextContent().Trim() != "1")
                .ToArray();

            var dayButton = dayButtons[dayNr - 1];
            dayButton.ClickViaJS();
            WaitForElementToVanish(DatePickerDialogSelector);
        }

        private string[] WaitForMonthAndYearToBeSet()
        {
            return WaitFor(() =>
            {
                var monthAndYearSpan = WaitForElement(MonthAndYearSpanSelector, searchContext: _monthSelector);
                if (monthAndYearSpan != null)
                {
                    var montAndYearTxt = monthAndYearSpan.Text; //e.g. March 2022
                    if (!string.IsNullOrWhiteSpace(montAndYearTxt))
                    {
                        var monthYearParts = montAndYearTxt.Split(' ');
                        if (monthYearParts.Length == 2 &&
                            int.TryParse(monthYearParts[1], out _))
                        {
                            return (true, monthYearParts);
                        }
                    }
                }

                return (false, null);
            })!;
        }
    }
}
