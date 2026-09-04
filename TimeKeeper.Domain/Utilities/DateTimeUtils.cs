using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Utilities;

public static class DateTimeUtils
{
    /// <summary>
    /// Converts the DateTime from UTC to Local
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ConvertUtcToLocal(this DateTime dateTime)
    {
        TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
        DateTime result = TimeZoneInfo.ConvertTimeFromUtc(dateTime, localTimeZone);
        return result;
    }

    /// <summary>
    /// Converts the DateTime from Local to Utc
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ConvertLocalToUtc(this DateTime dateTime)
    {
        TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

        DateTime localTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        DateTime result = TimeZoneInfo.ConvertTimeToUtc(localTime, localTimeZone);
        return result;
    }

    /// <summary>
    ///  Returns a timestamp representing the end of the day (11:59 PM) of the <paramref name="dateTime"/> 
    ///  argument.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime GetEndOfDay(this DateTime dateTime)
    {
        DateOnly dateOnly = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        DateTime endOfDay = dateOnly.GetEndOfDay();
        return endOfDay;
    }

    /// <summary>
    ///  Returns a timestamp representing the end of the day (11:59 PM) of the <paramref name="dateTime"/> 
    ///  argument.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime GetEndOfDay(this DateOnly dateOnly)
    {
        DateTime dateTime = dateOnly.ToDateTime(new TimeOnly(0, 0, 0));
        DateTime endOfDay = dateTime.AddDays(1).AddTicks(-1);
        return endOfDay;
    }

    /// <summary>
    ///  Returns a new DateTime instance representing the start of the day (midnight) for the specified 
    ///  date.
    /// </summary>
    /// <remarks>
    ///  The returned DateTime has the same date and Kind as the input value, but the time component
    ///  is set to midnight.
    /// </remarks>
    /// <param name="dateTime">
    ///  The date and time value for which to obtain the start of the day.
    /// </param>
    /// <returns>
    ///  A DateTime value set to 00:00:00 on the same date as the specified <paramref name="dateTime"/>.
    /// </returns>
    public static DateTime GetStartOfDay(this DateTime dateTime)
    {
        DateTime result = new(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        return result;
    }

    /// <summary>
    ///  Returns a timestamp representing the beginning of the day (12:00 AM) of the 
    ///  <paramref name="dateTime"/> argument.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime GetStartOfDay(this DateOnly dateOnly)
    {
        DateTime dateTime = dateOnly.ToDateTime(new TimeOnly(0, 0, 0));
        DateTime startOfDay = dateTime.Date;
        return startOfDay;
    }

    /// <summary>
    ///  Calculates the number of hours, rounded up or down to the quarter hour between two specified 
    ///  dates and times.
    /// </summary>
    /// <param name="lowerBound">
    ///  The earlier date and time to compare. Represents the start of the interval.
    ///  </param>
    /// <param name="upperBound">
    ///  The later date and time to compare. Represents the end of the interval.
    /// </param>
    /// <returns>
    ///  The total number of hours, rounded up or down to the quarter hour between <paramref name="lowerBound"/> 
    ///  and <paramref name="upperBound"/>. The value is positive if <paramref name="upperBound"/> 
    ///  is after <paramref name="lowerBound"/>; otherwise, it is negative.
    /// </returns>
    public static double HoursBetween(DateTime lowerBound, DateTime upperBound)
    {
        if(lowerBound > upperBound)
        {
            throw new ArgumentException("The lowerBound date cannot be later than the upperBound date.")    ;
        }

        double minutes = MinutesBetween(lowerBound, upperBound);
        return MinutesToHours((int)minutes);
    }

    /// <summary>
    ///  Calculates the total number of minutes between two DateTime values.
    /// </summary>
    /// <param name="lowerBound">
    ///  The earlier DateTime value.
    /// </param>
    /// <param name="upperBound">
    ///  The later DateTime value.
    /// </param>
    /// <returns>
    ///  The total number of minutes between the two DateTime values as a double.
    /// </returns>
    public static double MinutesBetween(DateTime lowerBound, DateTime upperBound)
    {
        TimeSpan difference = upperBound - lowerBound;
        return difference.TotalMinutes;
    }

    /// <summary>
    ///  Converts and returns the <paramref name="totalMinutes"/> argument to hours rounded to 
    ///  the nearest quarter hour
    /// </summary>
    /// <param name="totalMinutes"></param>
    /// <returns></returns>
    public static double MinutesToHours(int totalMinutes)
    {

        int hours = (int)(totalMinutes / 60);
        int minutes = totalMinutes - (60 * hours);

        double quarterHour = 0;

        if (minutes > 8 && minutes < 22)
        {
            quarterHour = .25;
        }
        else if (minutes > 21 && minutes < 37)
        {
            quarterHour = .5;
        }
        else if (minutes > 36 && minutes < 51)
        {
            quarterHour = .75;
        }
        else if (minutes > 50)
        {
            hours++;
        }

        return hours + quarterHour;

    }

    /// <summary>
    ///  Converts a DateTime value to a DateOnly value representing the same calendar date.
    /// </summary>
    /// <param name="dateTime">
    ///  The DateTime value to convert. The time component is ignored.
    /// </param>
    /// <returns>
    ///  A DateOnly value representing the year, month, and day components of the specified 
    ///  DateTime.
    /// </returns>
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        DateOnly result = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        return result;
    }
}
