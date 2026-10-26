# Testing

Run these separate required commands from the repository root:

```text
msbuild TimeKeeper.sln
dotnet test TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj
dotnet test TimeKeeper.App.Tests/TimeKeeper.App.Tests.csproj
```

Use the repository SDK/compiler (the application specifies C# 14), restored NuGet
dependencies and SQL project tooling for the solution build. The domain command
does not execute the component suite.

| Layer | Responsibility | Prerequisites and limits |
| --- | --- | --- |
| Domain unit tests | Business rules and model behavior | In-memory fixtures where possible |
| SQL integration tests in Domain.Tests | Database mappings, queries, commands and persistence | Configured SQL access and explicit disposable database fixtures; inspect TestDatabaseConfiguration and each fixture before running; never target production |
| bUnit in App.Tests | Rendered values, parameters, state transitions, callbacks, conditional/empty states and real Radzen composition | Restored packages and .NET SDK; no SQL, secrets, running server or browser installation |
| Manual or real-browser tests | Actual CSS/layout/readability, browser rendering, visual polish and genuine JS/browser integration | Explicit scenarios and suitable browser/host environment |

The initial component tests render YesterdayAccomplishmentsCardComponent and
ContinueWorkingComponent directly. Each test owns and disposes its BunitContext.
They use real Radzen children, fixed in-memory data and strict default JS interop.
No extra services or JS setups are needed for these render paths. They never start
Program or load application configuration. The application reference compiles the
application but does not start it.

Yesterday coverage includes default/explicit empty input, nested cards and distinct
identity/title/accomplishment values, parameter updates back to empty, and null
work-item navigation. Continue Working covers labels, resume button parameters,
DOM clicks delivering exactly the selected callback argument, absent callback
safety and updated activities/controls. This does not prove the parent page's
resume/save workflow or persistence. Neither component implements sorting or
filtering; do not add production behavior to manufacture such coverage.

## Authoring component tests

Add C# xUnit tests under Components matching the application namespace structure.
Create a disposable context, render the real component, supply deterministic
parameters, act through rendered controls and assert observable results:

```csharp
using (var context = new BunitContext())
{
    var component = context.Render<YesterdayAccomplishmentsCardComponent>();
    Assert.Contains("No work recorded yesterday.", component.Markup);
    component.Render(parameters => parameters.Add(item => item.TimeEntries,
        [new TimeEntry { WorkItem = null, Accomplishment = "Completed analysis" }]));
    Assert.Contains("Completed analysis", component.Markup);
    Assert.DoesNotContain("No work recorded yesterday.", component.Markup);
}
```

Use component queries for Radzen parameters and composition and DOM events for
interaction wiring. Avoid private handler calls, whole-markup snapshots, CSS
implementation details, global loose JS interop, and stubs that remove the behavior
being claimed. If future components need services or JS, configure only the
dependencies/calls actually required and document what the simulation cannot prove.
Run the entire component command after adding tests; report discovered/passed/
failed/skipped counts and exit code. Skipped required cases do not establish coverage.

bUnit 2.1.1 is pinned as a test-only dependency, compatible with .NET 9 and xUnit:
[package compatibility](https://www.nuget.org/packages/bunit/2.1.1),
[official C# test guide](https://bunit.dev/docs/getting-started/writing-tests.html).

## Ship coverage and continuation

Map each acceptance criterion to the narrowest meaningful test. Passing executed
component tests establish only their claimed behaviors. For example, a passing
selected-resume DOM click test and empty-state test require no duplicate manual
callback/empty-state scenario. A criterion about real layout readability or browser
JS remains outstanding until appropriate human or browser evidence covers it.
A UI-related change alone does not require a browser pass.

When sandbox tooling blocks the component command, Tester may request the reviewed
script using executable `powershell`, arguments
`["-NoProfile", "-File", ".ship/validation/component-tests.ps1"]` and
workingDirectory `.` in the existing version 1 pending-validation contract. Pin
the script's final SHA256 in validation-policy.json with disposable true. Preserve
all identity fields and exact infrastructure-record mappings; use an empty records
list only for a required check that never ran. Direct arbitrary dotnet commands
are not enabled. SQL fixture restrictions remain unchanged.

Only the user invokes `ship-validate.ps1 -Run <id>` from Developer PowerShell.
Tester assesses fresh matching attempt streams, test names, counts, skips, exit
code and exact coverage in the report and external-assessment.json. Exit zero alone
is insufficient. Infrastructure gaps remain BLOCKED, demonstrated defects FAIL,
and complete required validation PASS. Existing evidence freshness and human
attestation contracts apply unchanged.

## Playwright decision

The initial examples need neither browser-dependent JavaScript nor end-to-end
application workflows. bUnit is sufficient for their deterministic coverage.
Defer Playwright to a later ticket with a specific browser integration or end-to-end
requirement; this change installs no browser binaries or hosting fixtures.
