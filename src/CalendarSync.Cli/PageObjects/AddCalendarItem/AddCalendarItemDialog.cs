using CalendarSync.Cli.Selenium;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.AddCalendarItem
{
    internal class AddCalendarItemDialog : PageComponentBase
    {
        private const string SelectedCalendarIconSelector = "div[data-app-section=\"CalendarCompose\"] div[data-app-section=\"Form_Content\"] i[data-icon-name=\"CircleFilled\"]";
        private const string ClockIconSelector = "div.ms-Modal-scrollableContent div[data-app-section=\"CalendarCompose\"] div[data-app-section=\"Form_Content\"] i[data-icon-name=\"Clock\"]";
        private const string FromDatePickerIconSelector = "i[data-icon-name=\"CalendarLtrRegular\"]";
        public string SelectedCalendarName
        {
            get
            {
                var calIconElement = WaitForElement(SelectedCalendarIconSelector);
                var calNameElement = calIconElement.GetParentElement().FindElement(By.CssSelector("div.ms-TextField-suffix span"));
                return calNameElement.GetTextContent();
            }
        }

        public AddCalendarItemDialog(IWebDriver webDriver) : base(webDriver)
        {
        }

        public AddCalendarItemDialog Initialize()
        {
            //The selected calendar gets rendered last
            WaitForElement(SelectedCalendarIconSelector);
            return this;
        }

        public AddCalendarItemDialog SetTitle(string title)
        {
            var clockIcon = WaitForElement(ClockIconSelector);
            var fieldRowsContainer = clockIcon.GetParentElement(6);
            var titleRow = fieldRowsContainer.GetChildren().First();
            var titleInput = titleRow.FindElement(By.CssSelector("input.ms-TextField-field"));
            titleInput.SendKeys(title);

            return this;
        }

        public AddCalendarItemDialog SetDateTimePeriod(DateTime fromDateTime, DateTime toDateTime)
        {
            var clockIcon = WaitForElement(ClockIconSelector);
            var dateTimePeriodRow = clockIcon.GetParentElement(3);
            var datePickerIcons = dateTimePeriodRow.FindElements(By.CssSelector(FromDatePickerIconSelector));
            var fromDatePickerIcon = datePickerIcons[0];
            var toDatePickerIcon = datePickerIcons[1];


            return this;
        }
    }
}
