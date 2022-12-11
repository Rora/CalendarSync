using CalendarSync.Cli.Dto;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalendarSync.Cli.PageObjects.CalendarEvent
{
    internal class CalendarItemDateTimeSpanParser
    {
        private const string TimeFormat = "HH:mm";
        private readonly Language _language;
        private static readonly IDictionary<Language, IDictionary<string, int>> MonthNrByLanguageAndName = new Dictionary<Language, IDictionary<string, int>>
        {
            {
                Language.English, new Dictionary<string, int>()
                {
                    { "Jan", 1 },
                    { "Feb", 2 },
                    { "Mar", 3 },
                    { "Apr", 4 },
                    { "May", 5 },
                    { "Jun", 6 },
                    { "Jul", 7 },
                    { "Aug", 8 },
                    { "Sep", 9 },
                    { "Oct", 10 },
                    { "Nov", 11 },
                    { "Dec", 12 },
                }
            },
            {
                Language.Dutch, new Dictionary<string, int>()
                {
                    { "Jan", 1 },
                    { "Feb", 2 },
                    { "Mrt", 3 },
                    { "Apr", 4 },
                    { "Mei", 5 },
                    { "Jun", 6 },
                    { "Jul", 7 },
                    { "Aug", 8 },
                    { "Sep", 9 },
                    { "Okt", 10 },
                    { "Nov", 11 },
                    { "Dec", 12 },
                }
            }
        };

        public CalendarItemDateTimeSpanParser(Language language)
        {
            _language = language;
        }

        internal (DateTime from, DateTime to, bool isAllDayItem) ParseDateTimeSpan
            (DateOrderEnum dateOrder, string dateTimePeriodText)
        {
            var datePartMatches = Regex.Matches(dateTimePeriodText, @"\d+\/\d+\/\d+");
            var isMonthWrittenAsAWord = !datePartMatches.Any();
            if (isMonthWrittenAsAWord)
            {
                //Matches tue 10 sep 2022 and tue sep 10 2222
                datePartMatches = Regex.Matches(dateTimePeriodText, @"\w+\s+(\w+)\s+(\w+)\s\d+");
            }

            switch (datePartMatches.Count)
            {
                case 1:
                    //If the text only contains 1 date part, it's a single day event
                    return ParseSingleDayDateTimeSpan(dateOrder, dateTimePeriodText, isMonthWrittenAsAWord);

                case 2:
                    //If the dateTime string contains more than 1 date part it's a multi day event
                    return ParseMultiDayDateTimeSpan(dateOrder, dateTimePeriodText, isMonthWrittenAsAWord);

                default:
                    throw new InvalidOperationException($"Could not detect date/datetime parts in '{dateTimePeriodText}'");
            }
        }

        private (DateTime from, DateTime to, bool isAllDayItem) ParseMultiDayDateTimeSpan(DateOrderEnum dateOrder,
            string dateTimePeriodText, bool isMonthWrittenAsAWord)
        {
            var allDayMultiDatesPattern = isMonthWrittenAsAWord
                //Matches Fri 9 Dec 2022 to Sat 10 Dec 2022 & Fri Dec 9 2022 to Sat Dec 10 2022
                ? @"^[^\s]+\s+([^\s]+\s+[^\s]+\s+\d{4})\s+[^\s]+\s+[^\s]+\s+([^\s]+\s+[^\s]+\s+\d{4})$"
                //Matches Fri 02/12/2022 to Sat 03/12/2022                
                : @"^\w+\s(\d+\/\d+\/\d+)\s.+?\s\w+?\s(\d+\/\d+\/\d+)$";
            var allDayMultiDatesMatch = Regex.Match(dateTimePeriodText, allDayMultiDatesPattern);

            if (allDayMultiDatesMatch.Success)
            {
                var date1Str = allDayMultiDatesMatch.Groups[1].Value;
                var date2Str = allDayMultiDatesMatch.Groups[2].Value;
                var date1 = ParseDate(dateOrder, isMonthWrittenAsAWord, date1Str);
                var date2 = ParseDate(dateOrder, isMonthWrittenAsAWord, date2Str);

                return (date1.ToDateTime(new TimeOnly(0, 0)),
                    date2.ToDateTime(new TimeOnly(0, 0)),
                    isAllDayItem: true);
            }

            //Multi day event with a time
            var timedMultiDatesPattern = isMonthWrittenAsAWord
                //Matches Tue 6 Dec 2022, 20:30 to Wed 7 Dec 2022, 21:00 & Tue Dec 7 2022, 20:30 to Wed Dec 7 2022, 21:00
                ? @"^[^\s]+\s([^\s]+\s+[^\s]+\s\d{4}),{0,1}\s(\d+:\d+)\s[^\s]+\s[^\s]+\s([^\s]+\s+[^\s]+\s\d{4}),{0,1}\s(\d+:\d+)$"
                //Matches Tue 29/11/2022, 18:30 to Wed 30/11/2022, 19:30                
                : @"^\w+\s(\d+\/\d+\/\d+),\s(\d+:\d+)\s.+?\s\w+?\s(\d+\/\d+\/\d+),\s(\d+:\d+)$";
            var timedMutliDatesMatch = Regex.Match(dateTimePeriodText, timedMultiDatesPattern);
            var startDateStr = timedMutliDatesMatch.Groups[1].Value;
            var startTimeStr = timedMutliDatesMatch.Groups[2].Value;
            var endDateStr = timedMutliDatesMatch.Groups[3].Value;
            var endTimeStr = timedMutliDatesMatch.Groups[4].Value;

            return (ParseDateTime(startDateStr, startTimeStr, dateOrder, isMonthWrittenAsAWord),
                ParseDateTime(endDateStr, endTimeStr, dateOrder, isMonthWrittenAsAWord),
                isAllDayItem: false);
        }

        private (DateTime from, DateTime to, bool isAllDayItem) ParseSingleDayDateTimeSpan
            (DateOrderEnum dateOrder, string dateTimePeriodText, bool isMonthWrittenAsAWord)
        {
            var dateTimePattern = isMonthWrittenAsAWord
                ? @"^\w+\s+(\w+\s+\w+\s+\d{4})\s(.+)$"  //Matches Tue 6 Dec 2022 18:30 - 20:00 & Tue Dec 6 2022 18:30 - 20:00
                : @"^\w+\s(\d+\/\d+\/\d{4})\s(.+)$";    //Matches Tue 6/12/2022 18:30 - 20:00

            var dateTimeMatch = Regex.Match(dateTimePeriodText, dateTimePattern);
            var dateText = dateTimeMatch.Groups[1].Value;
            var timePeriodText = dateTimeMatch.Groups[2].Value;
            var date = ParseDate(dateOrder, isMonthWrittenAsAWord, dateText);

            var isAllDay = timePeriodText.StartsWith('(') && timePeriodText.EndsWith(')');

            if (isAllDay)
            {
                return (date.ToDateTime(new TimeOnly()), 
                    date.ToDateTime(new TimeOnly()),
                    isAllDayItem: true);
            }

            //Single date even with a time
            var timePeriodParts = timePeriodText.Split('-', StringSplitOptions.TrimEntries);
            var startTimeText = timePeriodParts[0];
            var endTimeText = timePeriodParts[1];
            var startTime = TimeOnly.ParseExact(startTimeText, TimeFormat);
            var endTime = TimeOnly.ParseExact(endTimeText, TimeFormat);

            return (date.ToDateTime(startTime),
                date.ToDateTime(endTime),
                isAllDayItem: false);
        }

        private DateOnly ParseDate(DateOrderEnum dateOrder, bool isMonthWrittenAsAWord, 
            string dateText)
        {
            if (isMonthWrittenAsAWord)
            {
                var dateParts = dateText.Split(' ');
                var dayPart = dateOrder == DateOrderEnum.DayMonthYear
                    ? dateParts[0] : dateParts[1];
                var monthPart = dateOrder == DateOrderEnum.DayMonthYear
                    ? dateParts[1] : dateParts[0];
                var yearPart = dateParts[2];

                var month = MonthNrByLanguageAndName[_language][monthPart];
                return new DateOnly(Convert.ToInt32(yearPart), month, Convert.ToInt32(dayPart));
            }

            var datePattern = dateOrder == DateOrderEnum.DayMonthYear
                ? "dd/MM/yyyy"
                : "MM/dd/yyyy";

            return DateOnly.ParseExact(dateText, datePattern);
        }

        DateTime ParseDateTime(string dateStr, string timeStr, 
            DateOrderEnum dateOrder, bool isMonthWrittenAsAWord)
        {
            var date = ParseDate(dateOrder, isMonthWrittenAsAWord, dateStr);
            var time = TimeOnly.ParseExact(timeStr, TimeFormat);
            return date.ToDateTime(time);
        }
    }
}
