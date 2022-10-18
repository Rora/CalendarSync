using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.Auth
{
    internal class SignInEnterEmailPage : PageBase
    {
        private const string EmailInputSelector = "input[type=email]";
        private IWebElement _emailInputElement;
        private IWebElement _submitButtonElement;

        public SignInEnterEmailPage(IWebDriver driver)
            : base(driver)
        {
        }

        public void Initialize()
        {
            _emailInputElement = WaitForElement(EmailInputSelector);
            _submitButtonElement = WaitForElement("input[type=submit]");
        }

        public void SubmitEmailAddress(string emailAddress)
        {
            _emailInputElement.SendKeys(emailAddress);
            _submitButtonElement.Submit();
            WaitForElementToVanish(EmailInputSelector);
        }
    }
}
