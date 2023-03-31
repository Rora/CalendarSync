using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CalendarSync.Cli.Selenium;
internal static class IWebElementExtensions
{
    public static IWebDriver? WebDriver { get; set; }

    internal static bool IsStale(this IWebElement webElement)
    {
        return ExpectedConditions.StalenessOf(webElement)(WebDriver);
    }

    internal static string GetTextContent(this IWebElement webElement)
    {
        return webElement.GetAttribute("textContent");
    }

    internal static IWebElement GetParentElement(this IWebElement webElement)
    {
        return webElement.FindElement(By.XPath("./.."));
    }

    internal static IWebElement GetParentElement(this IWebElement webElement, int nrOfHopsUp)
    {
        for (int i = 0; i < nrOfHopsUp; i++)
        {
            webElement = webElement.GetParentElement();
        }
        return webElement;
    }

    internal static IEnumerable<IWebElement> GetSiblings(this IWebElement webElement)
    {
        //return webElement.GetParentElement()
        //    .GetChildren()
        //    .ToArray();
        return webElement.FindElements(By.XPath("preceding-sibling::*")).Union(
            webElement.FindElements(By.XPath("following-sibling::*")))
            .ToArray();
    }
    internal static IEnumerable<IWebElement> GetChildren(this IWebElement webElement)
    {
        return webElement.FindElements(By.XPath("./child::*"));
    }

    internal static void ClickViaJS(this IWebElement webElement)
    {
        var executor = (IJavaScriptExecutor?)WebDriver ?? throw new InvalidOperationException("WebDriver is not set");
        executor.ExecuteScript("arguments[0].click();", webElement);
    }

    internal static void WaitUntilClickable(this IWebElement webElement, CancellationToken ct = default)
    {
        var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(5));
        wait.Until(ExpectedConditions.ElementToBeClickable(webElement), ct);
    }
}
