namespace TimeKeeper.App.Api.RequestPipeline
{
    public static class WebApplicationExtensions
    {
        public static WebApplication InitializeDatabase(this WebApplication app)
        {
            return app;
        }
    }
}
