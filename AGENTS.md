# TimeZone Agent Instructions

## Solution

The primary solution is:

```text
TimeKeeper.sln
```

## Build

Build the solution with:

```text
msbuild TimeKeeper.sln
```

## Test

Run all .NET tests with:

```text
dotnet test TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj
```

## General Rules

- Preserve the existing architecture unless a task explicitly requires a change.
- Do not remove tests simply to make a build pass.
- Do not suppress compiler warnings without explanation.
- Do not modify generated files.
- Do not add new NuGet packages unless they are necessary.
- Never commit passwords, API keys, connection strings, or secrets.

## Working Rules

Before modifying code:

1. Understand the requested change.
2. Inspect the relevant existing code.
3. Produce a plan.
4. Make the smallest reasonable implementation.
5. Build the solution.
6. Run relevant tests.
7. Explain what changed.