using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.CalendarEvent
{
    internal record CalendarItemDto(
        string CalendarItemId,
        string Name,
        string CalendarName,
        string Description,
        DateTime StartDateTime,
        DateTime EndDateTime,
        bool IsAllDay);
}
