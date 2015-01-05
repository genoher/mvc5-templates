using System;

namespace DuetGroup.WebsiteUtils.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        static Lazy<DateTimeOffset> _today;
        static Lazy<DateTimeOffset> _endOfTime;

        static DateTimeOffsetExtensions()
        {
            _today = new Lazy<DateTimeOffset>(() => new DateTimeOffset(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)));
            _endOfTime = new Lazy<DateTimeOffset>(() => new DateTimeOffset(new DateTime(2099, 12, 31)));
        }

        public static DateTimeOffset GetToday()
        {
            return _today.Value;
        }

        public static DateTimeOffset GetEndOfTime()
        {
            return _endOfTime.Value;
        }

    }
}
