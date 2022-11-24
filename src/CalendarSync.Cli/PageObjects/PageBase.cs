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

        protected IWebElement WaitForElement(string cssSelector, CancellationToken ct = default)
        {
            return WaitForElement(cssSelector, TimeSpan.FromSeconds(30), ct);
        }

        protected IWebElement WaitForElement(string cssSelector, TimeSpan timeout, CancellationToken ct = default)
        {
            return WaitForElements(cssSelector, timeout, ct: ct).Single();
        }

        protected IEnumerable<IWebElement> WaitForElements(string cssSelector, CancellationToken ct = default)
        {
            return WaitForElements(cssSelector, TimeSpan.FromSeconds(30), ct: ct);
        }

        protected IEnumerable<IWebElement> WaitForElements(string cssSelector, TimeSpan timeout, 
            int msBetweenTries = 10, Action<IWebDriver>? actionBetweenTries = null, CancellationToken ct = default)
        {
            var timeoutDate = DateTime.Now + timeout;

            while (DateTime.Now < timeoutDate)
            {
                var els = _driver.FindElements(By.CssSelector(cssSelector));
                if (els.Any())
                {
                    return els;
                }
                Thread.Sleep(msBetweenTries);
                ct.ThrowIfCancellationRequested();
                actionBetweenTries?.Invoke(_driver);
            }

            throw new InvalidOperationException("Timeout reached before element was found");
        }

        protected void WaitForElementToVanish(string cssSelector, CancellationToken ct = default)
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
                ct.ThrowIfCancellationRequested();
            }

            throw new InvalidOperationException("Timeout reached before element was vanished");
        }
    }
}