# TimeZone Deployment Guide

**Product:** TimeZone - Personal Time Tracker for Software Developers

**Version:** 1.0

**Last Updated:** September 2026

---

# Purpose

This document defines the deployment architecture, deployment process, and operational standards for TimeZone.

The objectives are:

- Consistent deployments
- Repeatable deployments
- Minimal downtime
- Preservation of user data
- Simplified upgrades

---

# Deployment Architecture

## Logical Architecture

```text
+-------------------+
|     Browser       |
+----------+--------+
           |
           v
+-------------------+
|    IIS Website    |
+----------+--------+
           |
           v
+-------------------+
|  Blazor Server    |
|  TimeZone App     |
+----------+--------+
           |
           v
+-------------------+
|   SQL Server      |
|   TimeZone DB     |
+-------------------+
```

---

# Supported Environments

## Development

```text
Developer Workstation

Visual Studio

Local IIS (Optional)

SQL Server Express
```

---

## Production

```text
Windows Server

IIS

ASP.NET Core Hosting Bundle

SQL Server
```

---

# Deployment Model

TimeZone uses:

```text
Folder Deployment
```

published from:

```text
Visual Studio
```

to:

```text
IIS Hosted Site
```

---

# Recommended Folder Structure

```text
C:\Applications
│
└── TimeZone
    │
    ├── Current
    │
    ├── Releases
    │   ├── 1.0.0
    │   ├── 1.1.0
    │   └── 1.2.0
    │
    ├── Backups
    │   ├── Database
    │   └── Application
    │
    ├── Logs
    │
    └── Scripts
```

---

# Deployment Strategy

## Current Version Folder

IIS should point to:

```text
C:\Applications\TimeZone\Current
```

The Current folder contains the active release.

---

## Release Folder

Each deployed version should be archived.

Example:

```text
C:\Applications\TimeZone\Releases\1.0.0

C:\Applications\TimeZone\Releases\1.1.0
```

This supports rollback scenarios.

---

# IIS Configuration

## Website

Recommended name:

```text
TimeZone
```

---

## Application Pool

Recommended name:

```text
TimeZoneAppPool
```

---

## Application Pool Settings

### .NET CLR Version

```text
No Managed Code
```

---

### Pipeline Mode

```text
Integrated
```

---

### Start Mode

```text
AlwaysRunning
```

Recommended for production systems.

---

### Idle Timeout

Recommended:

```text
0
```

(disabled)

This avoids unnecessary application restarts.

---

# Publish Profile Standards

## Profile Name

Recommended:

```text
TimeZone_LocalIIS
```

---

## Publish Method

```text
Folder
```

---

## Target Folder

```text
C:\Applications\TimeZone\Current
```

---

# Application Configuration

Configuration values should be stored in:

```text
appsettings.json
```

and

```text
appsettings.Development.json
```

Do not hard-code:

- Connection Strings
- Environment Values
- Paths

---

## Example Configuration

```json
{
  "ConnectionStrings": {
    "TimeKeeperDB": "Server=localhost;Database=TimeZone;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

# Deployment Procedure

## Step 1

Review release notes.

Reference:

```text
docs/ReleaseNotes.md
```

---

## Step 2

Backup database.

Example:

```text
TimeZone_YYYYMMDD.bak
```

---

## Step 3

Backup application files.

Copy:

```text
Current
```

to:

```text
Backups\Application
```

---

## Step 4

Stop IIS Website.

```text
TimeZone
```

---

## Step 5

Stop Application Pool.

```text
TimeZoneAppPool
```

---

## Step 6

Deploy database updates.

Methods:

```text
SQL Script

or

Database Project Publish
```

---

## Step 7

Publish application.

Target:

```text
Current
```

folder.

---

## Step 8

Start Application Pool.

---

## Step 9

Start IIS Website.

---

## Step 10

Perform validation testing.

---

# Release Validation Checklist

## Application

- [ ] Site starts successfully
- [ ] Login page loads
- [ ] Dashboard loads
- [ ] Navigation works

---

## Database

- [ ] Database reachable
- [ ] Categories load
- [ ] Activities load
- [ ] Time Entries load

---

## Time Tracking

- [ ] Start Work
- [ ] Save Changes
- [ ] Stop Work
- [ ] Submit For Review

---

## Reporting

- [ ] Dashboard metrics populate
- [ ] Reports execute successfully

---

# Logging Strategy

## Current State

Minimal logging.

---

## Future State

Implement:

```text
ILogger

Structured Logging

Serilog (Optional)
```

---

## Log Folder

Recommended:

```text
C:\Applications\TimeZone\Logs
```

---

# Security Considerations

## Database

Use:

```text
Integrated Security
```

when possible.

---

## Passwords

Passwords must:

- Never be stored in plain text.
- Always be hashed.

Current password hashing implementation satisfies this requirement.

---

## File Permissions

Application Pool identity should have:

```text
Read

Execute
```

permissions on deployment folders.

---

# Backup Strategy

## Database

Frequency:

```text
Daily
```

minimum.

Recommended retention:

```text
30 Days
```

---

## Application Files

Backup before:

- Deployments
- Upgrades
- Major configuration changes

---

# Rollback Strategy

If deployment fails:

1. Stop IIS Site.
2. Restore previous application version.
3. Restore database backup.
4. Restart IIS.
5. Verify system operation.

Reference:

```text
docs/UpgradeGuide.md
```

---

# Recommended Release Workflow

```text
GitHub

    ↓

Feature Branch

    ↓

Testing

    ↓

Release Tag

    ↓

Publish

    ↓

Validation

    ↓

Production
```

---

# Version Management

## Release Tags

Recommended:

```text
v1.0.0

v1.1.0

v1.2.0
```

---

## Branch Strategy

```text
main

feature/*

release/*
```

Examples:

```text
feature/activity-number

feature/history-report

release/1.1.0
```

---

# Disaster Recovery

In the event of catastrophic failure:

1. Restore database backup.
2. Restore latest application backup.
3. Verify configuration.
4. Restart IIS.
5. Validate functionality.

---

# Operational Checklist

Before Deployment:

- [ ] Release Notes reviewed
- [ ] Database Backup completed
- [ ] Application Backup completed
- [ ] Database Script reviewed
- [ ] Application Build validated

After Deployment:

- [ ] Login verified
- [ ] Dashboard verified
- [ ] Categories verified
- [ ] Activities verified
- [ ] Time Tracking verified
- [ ] Reports verified

---

# Future Enhancements

Planned improvements:

- Automated deployments
- Automated backups
- Health checks
- Monitoring dashboard
- Structured logging
- Automated database migrations

---

# Document History

| Version | Description |
|----------|-------------|
| 1.0 | Initial deployment guide |