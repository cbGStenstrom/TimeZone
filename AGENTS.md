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

## Coding standards

All production-code changes must follow `docs/CodingStandards.md`.

The coding standards are requirements, not suggestions. Before modifying C# or
Blazor code, read the standards document and inspect the relevant existing code.

For Blazor pages and components:

- Never place implementation code in an `@code` block.
- Use a `.razor` file with a matching `.razor.cs` partial class.
- Follow the project's prescribed regions, member ordering, comments, naming,
  markup conventions, and dependency patterns.
- Do not reformat or reorganize unrelated code.

When a repository template exists for the artifact being created, use that
template as the structural reference.