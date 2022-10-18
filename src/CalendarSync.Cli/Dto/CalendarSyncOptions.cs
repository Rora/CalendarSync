using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.Dto
{
    internal record CalendarSyncOptions(CalendarOptions Source, CalendarOptions Target);
}
