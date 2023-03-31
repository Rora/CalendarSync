using Meziantou.Framework.Win32;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.FileLock
{
    internal class FileLockHelper
    {
        public static IEnumerable<Process> GetProcessesLockingFile(string filePath)
        {
            using var session = RestartManager.CreateSession();
            session.RegisterFile(filePath);
            return session.GetProcessesLockingResources();
        }

        public static void KillProcessesLockingFile(string filePath)
        {
            var lockingProcesses = GetProcessesLockingFile(filePath);
            foreach (var lockingProcess in lockingProcesses)
            {
                lockingProcess.Kill();
            }
        }
    }
}
