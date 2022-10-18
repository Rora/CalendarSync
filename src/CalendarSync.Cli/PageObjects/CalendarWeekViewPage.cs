using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects
{
    internal class CalendarWeekViewPage : PageBase
    {
        private const string ToggleLeftPaneButtonSelector = "button[data-automation-type=\"RibbonButton\"] i[data-icon-name=\"LineHorizontal3Regular\"]";
        private const string ShowAllOrSelectedCalendarsToggleSelector = "div[aria-label=\"calendar list\"] div[role=\"listbox\"] > button";
        private const string CalendarButtonsSelector = "div[aria-label=\"calendar list\"] ul div[role=\"listbox\"] button[role=\"option\"]";
        private IWebElement _toggleLeftPaneButton;

        public CalendarWeekViewPage(IWebDriver driver)
            : base(driver)
        {
        }

        public void Initialize()
        {
            _toggleLeftPaneButton = WaitForElement(ToggleLeftPaneButtonSelector);
        }
        
        public void EnsureSingleCalendarIsSelected(string calendarName)
        {
            calendarName = calendarName.ToLower();
            _toggleLeftPaneButton.Click();

            var calendarButtons = WaitForElements(CalendarButtonsSelector);
            var calendarButtonToSelect = calendarButtons.FirstOrDefault(cb => cb.GetAttribute("title").ToLower() == calendarName);
            if(calendarButtonToSelect == null)
            {
                WaitForElement(ShowAllOrSelectedCalendarsToggleSelector).Click();
                calendarButtons = WaitForElements(CalendarButtonsSelector);
                calendarButtonToSelect = calendarButtons.FirstOrDefault(cb => cb.GetAttribute("title").ToLower() != calendarName);

                if(calendarButtonToSelect == null)
                {
                    throw new InvalidOperationException($"Could not find calendar '{calendarName}' to select");
                }
            }

            //Ensure that the calendar is selected
            if(calendarButtonToSelect.GetAttribute("aria-selected") != "true")
            {
                calendarButtonToSelect.Click();
            }

            //Unselect the other buttons
            var calendarButtonsToUnSelect = calendarButtons
                .Where(cb => cb.GetAttribute("aria-selected") == "true" && cb.GetAttribute("title").ToLower() != calendarName)
                .ToArray();

            foreach (var calendarButton in calendarButtonsToUnSelect)
            {
                calendarButton.Click();
            }

            //Collapse the pane when done
            _toggleLeftPaneButton.Click();
        }
    }
}
