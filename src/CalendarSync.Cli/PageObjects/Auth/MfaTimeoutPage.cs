using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.Auth
{
    internal class MfaTimeoutPage : PageBase
    {
        private const string WaitingForMfaTextSelector = "#idDiv_SAASTO_Title";

        public MfaTimeoutPage(IWebDriver driver)
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
