$ErrorActionPreference = 'Stop'

$env:TIMEKEEPER_TEST_SQL = 'Server=(localdb)\TimeKeeper_FEAT018;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'

& dotnet test TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj

exit $LASTEXITCODE
