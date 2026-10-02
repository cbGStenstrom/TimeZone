# Implementation Plan

## Objective

Add one harmless, deterministic unit test for DateTimeUtils that verifies an exact whole-hour conversion. Do not modify production source code.

## Existing Behavior

`TimeKeeper.Domain/Utilities/DateTimeUtils.cs` implements `MinutesToHours(int)` by separating whole hours from remaining minutes and rounding the remainder to quarter-hour increments. For 60 minutes, the whole-hour component is 1 and the remainder is 0, so the method returns `1.0`.

`TimeKeeper.Domain.Tests/Utilities/DateTimeUtilsTests.cs` currently contains two xUnit `[Fact]` tests: 75 minutes returns `1.25`, and zero minutes returns `0.0`. The previous plan's zero-minute test is already present, so this task adds a distinct case. The test project targets .NET 9 and already references xUnit and the domain project.

## Files Likely Affected

- Modify `TimeKeeper.Domain.Tests/Utilities/DateTimeUtilsTests.cs` in the `TimeKeeper.Domain.Tests` project.
- Reference `TimeKeeper.Domain/Utilities/DateTimeUtils.cs` without modifying it.
- No project, dependency, generated-file, or architecture changes are needed.

## Implementation Plan

1. Preserve both existing tests and all unrelated repository changes.
2. Add one `[Fact]` named `MinutesToHours_With60Minutes_ReturnsOneHour` to the existing `DateTimeUtilsTests` class.
3. Follow the existing arrange/act/assert style: set `int totalMinutes = 60`, call `DateTimeUtils.MinutesToHours(totalMinutes)`, and assert `Assert.Equal(1.0, result)`.
4. Build the solution with `msbuild TimeKeeper.sln`.
5. Run all domain tests with `dotnet test TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj`.
6. Review the implementation diff against the starting state to confirm only the intended test was added. Report build and test results, including any environmental blockers or failures, without changing production code, removing tests, or suppressing warnings.

## Acceptance Criteria

- Exactly one additional xUnit test verifies that 60 minutes converts to `1.0` hours.
- Both existing tests remain unchanged and pass.
- The new test passes against the current production implementation.
- Production source code, project dependencies, and unrelated changes remain untouched.
- The solution builds and all domain tests pass; any validation blocker is explicitly reported.
- The new test uses fixed input and no current time, local time zone, external services, or shared mutable state.

## Required Tests

- New: `DateTimeUtilsTests.MinutesToHours_With60Minutes_ReturnsOneHour` expects `1.0` for input `60`.
- Existing: `DateTimeUtilsTests.MinutesToHours_With75Minutes_ReturnsOneAndQuarterHours` expects `1.25` for input `75`.
- Existing: `DateTimeUtilsTests.MinutesToHours_WithZeroMinutes_ReturnsZeroHours` expects `0.0` for input `0`.
- Full suite: `dotnet test TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj`.

This document is planning only. Implementation and build/test execution belong to subsequent Ship stages.
