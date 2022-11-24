using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CalendarSync.Cli
{
    internal static class IWebElementExtensions
    {
        public static IWebDriver? WebDriver { get; set; }

        internal static void ClickViaJS(this IWebElement webElement)
        {
            var executor = (IJavaScriptExecutor?) WebDriver ?? throw new InvalidOperationException("WebDriver is not set");
            executor.ExecuteScript("arguments[0].click();", webElement);
        }

        internal static void WaitUntilClickable(this IWebElement webElement, CancellationToken ct = default)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementToBeClickable(webElement), ct);
        }
    }
}
