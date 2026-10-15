using System.Text.Json;

namespace TimeKeeper.Domain.Tests;

internal static class TestDatabaseConfiguration
{
    #region Connection String Names 

    internal static class ConnectionNames
    {
        /// <summary>
        /// The connection string name used for the development environment.
        /// </summary>
        public const string Development = "TimeKeeperDB";
        /// <summary>
        /// The connection string name used for the test database.
        /// </summary>
        public const string Test = "TimeKeeperTestDB";
    }

    internal static class ProjectNames
    {
        /// <summary>
        /// The project name for the TimeKeeper application.
        /// </summary>
        public const string ApplicationProject = "TimeKeeper.App";

        /// <summary>
        /// The file name of the app settings configuration file for the development environment.
        /// </summary>
        public const string AppSettingsFileName = "appsettings.Development.json";

        /// <summary>
        /// The solution file name.
        /// </summary>
        public const string SolutionName = "TimeKeeper.sln";
    }

    #endregion Connection String Names 

    #region public Properties

    /// <summary>
    /// Gets the connection string for the development environment.
    /// </summary>
    public static string DevelopmentConnectionString =>
        GetConnectionString(ConnectionNames.Development);

    /// <summary>
    /// Gets the connection string associated with the test connection name.
    /// </summary>
    /// <remarks>Resolved by calling <c>GetConnectionString(ConnectionNames.Test)</c>.</remarks>
    public static string TestConnectionString =>
        GetConnectionString(ConnectionNames.Test);

    #endregion public Properties

    #region private Methods

    /// <summary>
    ///  Gets a named connection string from TimeKeeper.App/appsettings.Development.json.
    /// </summary>
    /// <remarks>
    ///  Resolves the configuration file path from the solution root.</remarks>
    /// <param name="name">
    ///  The connection string key in the ConnectionStrings section.</param>
    /// <returns>
    ///  The non-empty connection string value for the specified key.</returns>
    /// <exception cref="InvalidOperationException">
    ///  Thrown when the development configuration file is missing, the ConnectionStrings section 
    ///  is missing, the specified connection string key is missing, or the resolved value is null, 
    ///  empty, or whitespace.</exception>
    private static string GetConnectionString(string name)
    {
        var solutionRoot = FindSolutionRoot();

        var settingsPath = Path.Combine(solutionRoot, ProjectNames.ApplicationProject, ProjectNames.AppSettingsFileName);

        if (!File.Exists(settingsPath))
        {
            throw new InvalidOperationException(
                $"Development application configuration was not found: {settingsPath}");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(settingsPath));

        if (!document.RootElement.TryGetProperty(
                "ConnectionStrings",
                out var connectionStrings))
        {
            throw new InvalidOperationException(
                "The ConnectionStrings section was not found in appsettings.Development.json.");
        }

        if (!connectionStrings.TryGetProperty(name, out var connectionValue))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{name} was not found in appsettings.Development.json.");
        }

        var connectionString = connectionValue.GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{name} is empty in appsettings.Development.json.");
        }

        return connectionString;
    }

    /// <summary>
    ///  Locates the solution root directory by searching the current base directory and its parent 
    ///  directories for the `TimeKeeper.sln` file.
    /// </summary>
    /// <returns>
    ///  The full path of the directory that contains `TimeKeeper.sln`.</returns>
    ///  <exception cref="InvalidOperationException">Thrown when no directory in the current base 
    ///  directory hierarchy contains `TimeKeeper.sln`.</exception>
    private static string FindSolutionRoot()
    {
        // WHAT: This method is used to locate the root directory of the solution by searching for
        //  the `TimeKeeper.sln` file. It starts from the base directory of the application and
        //  traverses up the directory hierarchy until it finds the solution file or reaches the
        //  root of the filesystem.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, ProjectNames.SolutionName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the TimeKeeper solution root.");
    }

    #endregion private Methods
}