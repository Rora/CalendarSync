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

        protected IWebElement WaitForElement(string cssSelector, TimeSpan? timeout = null,
            int msBetweenTries = 10, ISearchContext? searchContext = null, Action<IWebDriver>? actionBetweenTries = null,
            int minimumElements = 1, CancellationToken ct = default)
        {
            return WaitForElements(cssSelector, timeout, msBetweenTries, searchContext, actionBetweenTries, minimumElements, ct)
                .Single();
        }

        protected IEnumerable<IWebElement> WaitForElements(string cssSelector, TimeSpan? timeout = null,
            int msBetweenTries = 10, ISearchContext? searchContext = null, Action<IWebDriver>? actionBetweenTries = null,
            int minimumElements = 1, CancellationToken ct = default)
        {
            searchContext ??= _webDriver;
            
            return WaitFor(() =>
            {
                var els = searchContext.FindElements(By.CssSelector(cssSelector));
                if (els.Count >= minimumElements)
                {
                    return (true, els);
                }

                return (false, null);
            }, timeout, msBetweenTries, actionBetweenTries, ct)!;
        }

        protected T? WaitFor<T>(Func<(bool, T?)> func, TimeSpan? timeout = null,
            int msBetweenTries = 10, Action<IWebDriver>? actionBetweenTries = null,
            CancellationToken ct = default)
        {
            var timeoutDate = DateTime.Now + (timeout ?? TimeSpan.FromSeconds(30));

            while (DateTime.Now < timeoutDate)
            {
                var (success, result) = func();
                if (success)
                {
                    return result;
                }
                Thread.Sleep(msBetweenTries);
                ct.ThrowIfCancellationRequested();
                actionBetweenTries?.Invoke(_webDriver);
            }

            throw new InvalidOperationException("Timeout reached success condition was met");
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