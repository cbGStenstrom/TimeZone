using TimeKeeper.Domain.Utilities;
using Xunit;

namespace TimeKeeper.Domain.Tests.Utilities;

public class DateTimeUtilsTests
{
    [Fact]
    public void ToDateOnly_WithJanuaryFifth2026AtTwelveTwentyTwoFortyFive_ReturnsJanuaryFifth2026()
    {
        DateTime input = new DateTime(2026, 1, 5, 12, 22, 45);
        DateOnly expected = new DateOnly(2026, 1, 5);

        DateOnly result = DateTimeUtils.ToDateOnly(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void HoursBetween_FromFiveAmToSevenTwentyFivePm_ReturnsFourteenAndHalfHours()
    {
        DateTime lowerBound = new DateTime(2026, 1, 15, 5, 0, 0);
        DateTime upperBound = new DateTime(2026, 1, 15, 19, 25, 0);

        double result = DateTimeUtils.HoursBetween(lowerBound, upperBound);

        Assert.Equal(14.5, result);
    }

    [Fact]
    public void MinutesToHours_With75Minutes_ReturnsOneAndQuarterHours()
    {
        int totalMinutes = 75;

        double result = DateTimeUtils.MinutesToHours(totalMinutes);

        Assert.Equal(1.25, result);
    }

    [Fact]
    public void MinutesToHours_WithZeroMinutes_ReturnsZeroHours()
    {
        int totalMinutes = 0;

        double result = DateTimeUtils.MinutesToHours(totalMinutes);

        Assert.Equal(0.0, result);
    }

    [Fact]
    public void MinutesToHours_With60Minutes_ReturnsOneHour()
    {
        int totalMinutes = 60;

        double result = DateTimeUtils.MinutesToHours(totalMinutes);

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void MinutesToHours_With120Minutes_ReturnsTwoHours()
    {
        int totalMinutes = 120;

        double result = DateTimeUtils.MinutesToHours(totalMinutes);

        Assert.Equal(2.0, result);
    }
}
