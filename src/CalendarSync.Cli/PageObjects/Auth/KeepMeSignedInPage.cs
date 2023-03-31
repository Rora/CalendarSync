using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.Auth
{
    internal class KeepMeSignedInPage : PageComponentBase
    {
        private const string DontShowAgainCheckBoxSelector = "#KmsiCheckboxField";
        private IWebElement _yesButton;

        public KeepMeSignedInPage(IWebDriver driver)
            : base(driver)
        {
        }

        public void Initialize()
        {
            WaitForElement(DontShowAgainCheckBoxSelector);
            _yesButton = WaitForElement("input[type=submit].primary");
        }

        public void SubmitYes()
        {
            _yesButton.Submit();
            WaitForElementToVanish(DontShowAgainCheckBoxSelector);
        }
    }
}
