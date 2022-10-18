using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.Auth
{
    internal class WaitingForMfaPage : PageBase
    {
        private const string WaitingForMfaTextSelector = "#idDiv_SAOTCAS_Description";

        public WaitingForMfaPage(IWebDriver driver)
            : base(driver)
        {
        }

        public void Initialize()
        {
            WaitForElement(WaitingForMfaTextSelector);
        }

        public void WaitForMfaToCompleteOrTimeout()
        {
            WaitForElementToVanish(WaitingForMfaTextSelector);
        }
    }
}
