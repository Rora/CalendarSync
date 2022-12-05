using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.CalendarEvent
{
    internal record CalendarEventDto(
        string CalendarItemId,
        string Name,
        string Description,
        DateTime StartDateTime,
        DateTime EndDateTime,
        bool IsAllDay);
}
