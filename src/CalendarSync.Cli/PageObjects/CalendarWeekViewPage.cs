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
        private const string ShowAllOrSelectedCalendarsToggleSelector = "#leftPaneContainer div[role=\"complementary\"] div[role=\"listbox\"] > button";
        private const string CalendarButtonsSelector = "#leftPaneContainer div[role=\"complementary\"] div[role=\"listbox\"] button[role=\"option\"]";

        public CalendarWeekViewPage(IWebDriver driver)
            : base(driver)
        {
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
            if(calendarButtonToSelect.GetAttribute("aria-selected") != "true")
            {
                var calendarTitle = calendarButtonToSelect.GetAttribute("title");
                calendarButtonToSelect.ClickViaJS();
            }

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
    }
}
