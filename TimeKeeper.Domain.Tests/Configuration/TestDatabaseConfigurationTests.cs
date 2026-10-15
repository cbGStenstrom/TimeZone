using Xunit;

namespace TimeKeeper.Domain.Tests;

public class TestDatabaseConfigurationTests
{
    /// <summary>
    ///  Verifies that the development connection string is loaded, is not null or whitespace, and 
    ///  references the TimeZoneDB_Dev database.    
    /// </summary>
    [Fact]
    public void DevelopmentConnectionString_CanBeLoaded()
    {
        var connectionString =
            TestDatabaseConfiguration.DevelopmentConnectionString;

        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        Assert.Contains("TimeZoneDB_Dev", connectionString);
    }


    /// <summary>
    ///  Verifies that the test connection string is configured and contains the expected TimeZoneDB_Test 
    ///  database name. 
    /// </summary>
    [Fact]
    public void TestConnectionString_CanBeLoaded()
    {
        var connectionString =
            TestDatabaseConfiguration.TestConnectionString;

        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        Assert.Contains("TimeZoneDB_Test", connectionString);
    }
}