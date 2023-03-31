using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.Auth
{
    internal class SignInEnterPasswordPage : PageComponentBase
    {
        private const string PasswordInputSelector = "input[type=password]";
        private IWebElement? _passwordInputElement;
        private IWebElement? _submitButtonElement;

        public SignInEnterPasswordPage(IWebDriver driver)
            : base(driver)
        {
        }

        public void Initialize()
        {
            _passwordInputElement = WaitForElement(PasswordInputSelector);
            _submitButtonElement = WaitForElement("input[type=submit]");
        }

        public void SubmitPassword(string password)
        {
            _passwordInputElement!.SendKeys(password);
            _submitButtonElement!.Submit();
            WaitForElementToVanish(PasswordInputSelector);
        }
    }
}
