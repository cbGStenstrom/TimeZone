# TimeZone Architecture

**Product:** TimeZone - Personal Time Tracker for Software Developers

**Version:** 1.0

**Status:** Active

**Last Updated:** September 2026

---

# Purpose

This document defines the architectural standards, design principles, and development guidelines for the TimeZone application.

The objective is to ensure that all future development follows a consistent architecture that is:

- Maintainable
- Testable
- Extensible
- Secure
- Deployable
- Understandable

---

# Architectural Principles

## Principle 1 - Business Logic Does Not Belong In The UI

Blazor pages and components should be responsible for:

- Displaying data
- Capturing user input
- Raising events

Blazor pages should never:

- Access the database directly
- Implement business rules
- Manage persistence

---

## Principle 2 - Domain Logic Is Centralized

Business rules belong in:

```text
TimeKeeper.Domain
```

Examples:

- User validation
- Password validation
- WorkItem state transitions
- TimeEntry business rules

---

## Principle 3 - Data Access Is Isolated

Database access should occur only through:

```text
Command Handlers
Query Handlers
EF Core
```

UI layers must never reference EF Entities directly.

---

## Principle 4 - Favor Evolution Over Rewrite

The application already contains valuable functionality.

Future work should:

- Refactor incrementally
- Preserve user data
- Preserve existing workflows where practical

Large-scale rewrites are discouraged.

---

# Solution Structure

```text
TimeKeeper.sln
│
├── TimeKeeper.App
│
├── TimeKeeper.Domain
│
├── TimeKeeper.DataAccess
│
└── TimeKeeper.Data
```

---

# Layer Responsibilities

## TimeKeeper.App

### Responsibility

Presentation Layer

### Contains

- Pages
- Components
- Layouts
- Radzen controls
- User interaction logic

### Must Not Contain

- SQL access
- DbContext access
- EF entities
- Business rules

---

## TimeKeeper.Domain

### Responsibility

Business Layer

### Contains

- Domain Models
- Services
- Commands
- Queries
- Validators
- Security Utilities
- DTOs

### Responsibilities

- Validation
- Business workflows
- Data transformation

---

## TimeKeeper.DataAccess

### Responsibility

Persistence Layer

### Contains

- DbContext
- EF entities
- Entity relationships

### Responsibilities

- Database mapping
- Persistence

---

## TimeKeeper.Data

### Responsibility

Database Project

### Contains

- Tables
- Views
- Stored Procedures
- Migration scripts

---

# Current Request Flow

A typical request follows:

```text
Blazor Page

    ↓

Domain Service

    ↓

MediatR

    ↓

Command / Query Handler

    ↓

EF Core

    ↓

SQL Server
```

Example:

```text
ProjectManagerPage

    ↓

ProjectService

    ↓

AddProject Command

    ↓

ProjectCommandHandler

    ↓

TimeKeeperDbContext

    ↓

Projects Table
```

---

# CQRS Pattern

TimeZone uses CQRS through MediatR.

---

## Commands

Commands modify state.

Examples:

```text
AddProject

UpdateProject

DeleteProject

CreateTimeEntry

StartWorkOnWorkItem

EndWorkOnWorkItem
```

Commands should:

- Change data
- Not return collections

---

## Queries

Queries retrieve data.

Examples:

```text
GetProjectQuery

GetFilteredProjectsQuery

GetWorkItemSummaryForDateRange

GetActiveTimeEntry
```

Queries should:

- Never modify data
- Support filtering

---

# Entity Mapping Strategy

The application currently separates:

```text
Database Entity

        ↕

Mapper

        ↕

Domain Model
```

Example:

```text
DataAccess.Entities.Project

        ↕

ProjectMapper

        ↕

Domain.Models.Project
```

Benefits:

- Database independence
- Cleaner business layer
- Easier testing

---

# Dependency Injection

All dependencies should be registered through:

```csharp
AddTimeKeeperDomainServices()

AddTimeKeeperDbServices()
```

Avoid directly instantiating dependencies using:

```csharp
new MyService()
```

Use constructor injection wherever possible.

---

# Database Standards

## General Rules

All tables should have:

```sql
CreatedDate
CreatedBy
UpdatedDate
UpdatedBy
```

unless there is a compelling reason not to.

---

## Foreign Keys

All relationships should use explicit foreign keys.

Example:

```text
Category
    |
    └── Activities

Activity
    |
    └── TimeEntries

User
    |
    └── TimeEntries
```

---

## Naming Strategy

Current Names:

```text
Laborers
Projects
WorkItems
TimeEntries
```

Future Business Terminology:

```text
Users
Categories
Activities
TimeEntries
```

Database renaming will be evaluated later.

Business terminology should move first.

---

# Security Architecture

## Current State

Authentication:

```text
Database User
Password Hash
SessionService
```

---

## Future State

```text
Cookie Authentication

ClaimsPrincipal

AuthenticationStateProvider
```

---

## Roles

Planned:

```text
SuperUser

Administrator

Developer
```

---

# Configuration Standards

## Current Issue

Connection strings should not be embedded in code.

Avoid:

```csharp
UseSqlServer("Hard Coded Connection String")
```

---

## Preferred Approach

Store configuration in:

```json
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "TimeZone": "..."
  }
}
```

Access through:

```csharp
builder.Configuration
```

---

# Logging Strategy

Future versions should implement:

```text
Microsoft ILogger

Structured Logging

Serilog (optional)
```

Goals:

- Troubleshooting
- Diagnostics
- Error analysis

---

# UI Standards

## Design Goals

Interfaces should be:

- Clean
- Efficient
- Compact
- Modern

Avoid:

- Excessive spacing
- Oversized controls
- Excessive clicks

---

## Work Item Display Format

Future activity display format:

```text
{CategoryKey}-{ActivityNumber} {Title}
```

Example:

```text
JCAM-7251 Blazor Custom Query UI Component

AGILE-15 Sprint Planning

TIME-1 TimeZone Development
```

Activity Number should be stored independently from Title.

---

# Deployment Architecture

## Version 1.0 Target

```text
Local IIS

    ↓

Blazor Server

    ↓

SQL Server
```

---

## Supported Database

```text
SQL Server
```

---

## Publish Method

```text
Visual Studio Publish Profile

IIS Site

Folder Deployment
```

---

# Future Architectural Goals

## Version 1.1

- New Activity Manager
- Activity Number support
- Enhanced filtering

---

## Version 1.2

- WorkItem History
- Dashboard enhancements
- Reporting expansion

---

## Version 2.0

- Multi-user support
- Roles
- Authorization
- User administration

---

# Architectural Rule

Whenever a design decision is unclear:

1. Prefer simplicity.
2. Prefer maintainability.
3. Prefer backward compatibility.
4. Favor evolution over rewrite.
5. Preserve user data whenever practical.