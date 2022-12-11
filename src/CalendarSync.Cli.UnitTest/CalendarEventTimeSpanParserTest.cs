using CalendarSync.Cli.PageObjects;
using CalendarSync.Cli.PageObjects.CalendarEvent;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CalendarSync.Cli.UnitTest
{
    public class CalendarEventTimeSpanParserTest
    {
        public static IEnumerable<TestCaseData> TestCases
            => new TestCaseData[]
                {
                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Mon 28/11/2022 17:30 - 18:00",
                        new DateTime(2022, 11, 28, 17, 30, 0),
                        new DateTime(2022, 11, 28, 18, 0, 0),
                        false)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Single day with time"),

                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Mon 28 Nov 2022 17:30 - 18:00",
                        new DateTime(2022, 11, 28, 17, 30, 0),
                        new DateTime(2022, 11, 28, 18, 0, 0),
                        false)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Single day with time, but month name instead of nr"),
                    
                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Mon 28/11/2022 (All day)",
                        new DateTime(2022, 11, 28, 0, 0, 0),
                        new DateTime(2022, 11, 28, 0, 0, 0),
                        true)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Single day no time"),

                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Mon 28 Nov 2022 (All day)",
                        new DateTime(2022, 11, 28, 0, 0, 0),
                        new DateTime(2022, 11, 28, 0, 0, 0),
                        true)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Single day no time, but month name instead of nr"),

                    new TestCaseData(DateOrderEnum.MonthDayYear,
                        "Mon Nov 28 2022 (All day)",
                        new DateTime(2022, 11, 28, 0, 0, 0),
                        new DateTime(2022, 11, 28, 0, 0, 0),
                        true)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Single day no time, but month name instead of nr and month in front of day"),

                    new TestCaseData(DateOrderEnum.MonthDayYear,
                        "Sun 12/15/2022 19:30 - 22:00",
                        new DateTime(2022, 12, 15, 19, 30, 0),
                        new DateTime(2022, 12, 15, 22, 0, 0),
                        false)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Single day with time but different format"),
                    
                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Zondag 30/01/2022 00:00-23:59",
                        new DateTime(2022, 1, 30, 0, 0, 0),
                        new DateTime(2022, 1, 30, 23, 59, 0),
                        false)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Single day with time and longer day part"),
                    
                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Tue 29/11/2022, 18:30 to Wed 30/11/2022, 19:30",
                        new DateTime(2022, 11, 29, 18, 30, 0),
                        new DateTime(2022, 11, 30, 19, 30, 0),
                        false)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Multi day with time"),

                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Tue 29 Nov 2022, 18:30 to Wed 30 Nov 2022, 19:30",
                        new DateTime(2022, 11, 29, 18, 30, 0),
                        new DateTime(2022, 11, 30, 19, 30, 0),
                        false)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Multi day with time, but month name instead of nr"),

                    new TestCaseData(DateOrderEnum.MonthDayYear,
                        "Tue Nov 29 2022, 18:30 to Wed Nov 30 2022, 19:30",
                        new DateTime(2022, 11, 29, 18, 30, 0),
                        new DateTime(2022, 11, 30, 19, 30, 0),
                        false)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Multi day with time, but month name instead of nr and month in front of day"),

                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Fri 02/12/2022 to Sat 03/12/2022",
                        new DateTime(2022, 12, 2, 0, 0, 0),
                        new DateTime(2022, 12, 3, 0, 0, 0),
                        true)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Multi day without time"),
                    
                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Fri 02 Dec 2022 to Sat 03 Dec 2022",
                        new DateTime(2022, 12, 2, 0, 0, 0),
                        new DateTime(2022, 12, 3, 0, 0, 0),
                        true)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Multi day without time, but month name instead of nr"),
                    
                    new TestCaseData(DateOrderEnum.DayMonthYear,
                        "Fri 02/12/2022 to Sat 01/01/2023",
                        new DateTime(2022, 12, 2, 0, 0, 0),
                        new DateTime(2023, 1, 1, 0, 0, 0),
                        true)
                        .SetName($"{nameof(ParseDateTimeSpanTest)} - Multi year where start date is in an earlier month of the year"),
                };


        [TestCaseSource(nameof(TestCases))]
        public void ParseDateTimeSpanTest(DateOrderEnum dateOrder, string dateTimePeriodText,
            DateTime expectedStartDateTime, DateTime expectedEndDateTime, bool expectedIsAllDayItem)
        {
            var sut = new CalendarItemDateTimeSpanParser(Dto.Language.English);

            (var actualStartDateTime, var actualEndDateTime, var actualIsAllDayItem) =
                sut.ParseDateTimeSpan(dateOrder, dateTimePeriodText);

            //Assert
            Assert.AreEqual(expectedStartDateTime, actualStartDateTime);
            Assert.AreEqual(expectedEndDateTime, actualEndDateTime);
            Assert.AreEqual(expectedIsAllDayItem, actualIsAllDayItem);
        }
    }
}