using OpenQA.Selenium;

namespace CalendarSync.Cli.PageObjects
{
    internal abstract class PageBase
    {
        protected IWebDriver _driver;

        protected PageBase(IWebDriver driver)
        {
            this._driver = driver;
        }

        protected IWebElement WaitForElement(string cssSelector)
        {
            return WaitForElement(cssSelector, TimeSpan.FromSeconds(30));
        }

        protected IWebElement WaitForElement(string cssSelector, TimeSpan timeout)
        {
            return WaitForElements(cssSelector, timeout).Single();
        }

        protected IEnumerable<IWebElement> WaitForElements(string cssSelector)
        {
            return WaitForElements(cssSelector, TimeSpan.FromSeconds(30));
        }

        protected IEnumerable<IWebElement> WaitForElements(string cssSelector, TimeSpan timeout)
        {
            var timeoutDate = DateTime.Now + timeout;

            while (DateTime.Now < timeoutDate)
            {
                var els = _driver.FindElements(By.CssSelector(cssSelector));
                if (els.Any())
                {
                    return els;
                }
                Thread.Sleep(10);
            }

            throw new InvalidOperationException("Timeout reached before element was found");
        }



        protected void WaitForElementToVanish(string cssSelector)
        {
            var timeoutDate = DateTime.Now + TimeSpan.FromSeconds(30);

            while (DateTime.Now < timeoutDate)
            {
                var els = _driver.FindElements(By.CssSelector(cssSelector));
                if (!els.Any())
                {
                    return;
                }
                Thread.Sleep(10);
            }

            throw new InvalidOperationException("Timeout reached before element was vanished");
        }
    }
}