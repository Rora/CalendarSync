using CalendarSync.Cli.FileLock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.Selenium
{
    public static class SeleniumProcessKiller
    {
        public static void KillExistingSeleniumProcessesForProfile(string profileDir)
        {
            try
            {
                var seleniumProfileLockFile = Path.Combine(profileDir, "lockfile");
                FileLockHelper.KillProcessesLockingFile(seleniumProfileLockFile);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException("Found other webdriver(s) using the CalendarSync webdriver profile, failed to kill them. Please make sure other instances of CalendarSync aren't running.", e);
            }
        }

    }
}
