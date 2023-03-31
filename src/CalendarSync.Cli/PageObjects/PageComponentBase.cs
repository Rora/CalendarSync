using OpenQA.Selenium;

namespace CalendarSync.Cli.PageObjects
{
    internal abstract class PageComponentBase
    {
        protected IWebDriver _webDriver;

        protected PageComponentBase(IWebDriver webDriver)
        {
            this._webDriver = webDriver;
        }

        protected IWebElement WaitForElement(string cssSelector, CancellationToken ct = default)
        {
            return WaitForElement(cssSelector, TimeSpan.FromSeconds(30), ct);
        }

        protected IWebElement WaitForElement(string cssSelector, TimeSpan timeout, CancellationToken ct = default)
        {
            return WaitForElements(cssSelector, timeout, ct: ct).Single();
        }

        protected IEnumerable<IWebElement> WaitForElements(string cssSelector, TimeSpan? timeout = null, 
            int msBetweenTries = 10, ISearchContext? searchContext = null, Action<IWebDriver>? actionBetweenTries = null, 
            int minimumElements = 1, CancellationToken ct = default)
        {
            var timeoutDate = DateTime.Now + (timeout ?? TimeSpan.FromSeconds(30));
            searchContext ??= _webDriver;

            while (DateTime.Now < timeoutDate)
            {
                var els = searchContext.FindElements(By.CssSelector(cssSelector));
                if (els.Count >= minimumElements)
                {
                    return els;
                }
                Thread.Sleep(msBetweenTries);
                ct.ThrowIfCancellationRequested();
                actionBetweenTries?.Invoke(_webDriver);
            }

            throw new InvalidOperationException("Timeout reached before element was found");
        }

        protected void WaitForElementToVanish(string cssSelector, CancellationToken ct = default)
        {
            var timeoutDate = DateTime.Now + TimeSpan.FromSeconds(30);

            while (DateTime.Now < timeoutDate)
            {
                var els = _webDriver.FindElements(By.CssSelector(cssSelector));
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