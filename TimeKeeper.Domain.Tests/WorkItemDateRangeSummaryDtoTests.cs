using TimeKeeper.Domain.Models.DTOs;
using Xunit;

namespace TimeKeeper.Domain.Tests;

public class WorkItemDateRangeSummaryDtoTests
{
    [Theory]
    [InlineData("JCAM", "5721", "Refresh Design of the System Bar in A&A", "JCAM-5721 Refresh Design of the System Bar in A&A")]
    [InlineData("OTHER", "5721", "Refresh Design of the System Bar in A&A", "OTHER-5721 Refresh Design of the System Bar in A&A")]
    [InlineData("JCAM", "0007", "Fix A&A: Keep CASE!", "JCAM-0007 Fix A&A: Keep CASE!")]
    [InlineData("JCAM", " 007 ", " Title! ", "JCAM- 007   Title! ")]
    [InlineData(null, "5721", "Original Title!", "Original Title!")]
    [InlineData("", "5721", "Original Title!", "Original Title!")]
    [InlineData(" \t", "5721", "Original Title!", "Original Title!")]
    [InlineData("JCAM", null, "Original Title!", "Original Title!")]
    [InlineData("JCAM", "", "Original Title!", "Original Title!")]
    [InlineData("JCAM", " \t", "Original Title!", "Original Title!")]
    public void DisplayTitlePreservesStoredValues(
        string? projectKey, string? activityNumber, string title, string expected)
    {
        var summary = new WorkItemDateRangeSummaryDto
        {
            ProjectKey = projectKey,
            ActivityNumber = activityNumber,
            WorkItemTitle = title
        };

        Assert.Equal(expected, summary.DisplayTitle);
        Assert.Equal(projectKey, summary.ProjectKey);
        Assert.Equal(activityNumber, summary.ActivityNumber);
        Assert.Equal(title, summary.WorkItemTitle);
    }
}
