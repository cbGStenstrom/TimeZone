# TimeZone Upgrade Guide

**Product:** TimeZone - Personal Time Tracker for Software Developers

**Version:** 1.0

**Last Updated:** September 2026

---

# Purpose

This document defines the standard process for upgrading an existing TimeZone installation while preserving:

- User Accounts
- Categories
- Activities
- Time Entries
- Configuration
- Historical Data

The primary goal of the upgrade process is:

> Zero preventable data loss.

---

# Upgrade Philosophy

TimeZone upgrades should:

- Preserve existing data
- Preserve existing configuration
- Be repeatable
- Be reversible
- Be documented

Before any upgrade:

1. Backup the database.
2. Backup the application files.
3. Verify rollback procedures.
4. Record the current version.

---

# Supported Upgrade Paths

## Supported

```text
1.0.0 -> 1.0.1

1.0.1 -> 1.1.0

1.1.0 -> 1.2.0

1.x.x -> 2.0.0
```

---

## Unsupported

Skipping major releases may not be supported.

Example:

```text
1.0.0 -> 3.0.0
```

Future releases should include intermediary upgrade requirements.

---

# Upgrade Checklist

Before beginning:

- [ ] Current application version identified
- [ ] Database backup completed
- [ ] Application backup completed
- [ ] New release package available
- [ ] Change log reviewed
- [ ] Upgrade window scheduled

---

# Step 1 – Identify Current Version

Determine the currently installed version:

```text
TimeZone 1.0.0
```

Record:

```text
Current Version:
Application Path:
Database Name:
Upgrade Date:
```

---

# Step 2 – Backup Database

## SQL Server Management Studio

Open:

```text
SSMS
```

Navigate:

```text
Databases
    TimeZone
```

Right-click:

```text
Tasks
    Backup
```

Create backup:

```text
TimeZone_YYYYMMDD.bak
```

Example:

```text
TimeZone_20260903.bak
```

Save to:

```text
D:\Backups\Database
```

---

# Step 3 – Backup Application Files

Backup the IIS deployment folder.

Example:

```text
C:\Applications\TimeZone
```

Copy to:

```text
D:\Backups\Application\TimeZone_1.0.0
```

Verify backup completion before continuing.

---

# Step 4 – Review Release Notes

Read:

```text
ReleaseNotes.md
```

Review:

- New Features
- Bug Fixes
- Database Changes
- Configuration Changes
- Breaking Changes

Never upgrade without reviewing release notes.

---

# Step 5 – Stop IIS Site

Open:

```text
IIS Manager
```

Stop:

```text
TimeZone Site
```

Stop Application Pool:

```text
TimeZoneAppPool
```

Verify:

```text
Site Status = Stopped
Application Pool Status = Stopped
```

---

# Step 6 – Deploy Database Changes

## Method 1 – Database Project Publish

Open:

```text
TimeKeeper.Data.sqlproj
```

Publish to:

```text
TimeZone Database
```

Review generated script before execution.

---

## Method 2 – Upgrade Script

Future releases may provide:

```text
Upgrade_1.0.0_To_1.1.0.sql
```

Execute using:

```text
SQL Server Management Studio
```

Verify successful completion.

---

# Step 7 – Deploy Application Files

Publish the new application build to:

```text
C:\Applications\TimeZone
```

Recommended:

### Blue/Green Folder Structure

```text
C:\Applications\TimeZone

    Current

    Releases
        1.0.0
        1.1.0

    Backups
```

Deployment example:

```text
Releases\1.1.0
```

Update IIS to point to:

```text
Current
```

or replace files directly depending on deployment strategy.

---

# Step 8 – Review Configuration

Compare:

```text
appsettings.json
```

between:

```text
Current Release

New Release
```

Verify:

- Connection Strings
- Logging Settings
- Environment Settings

Preserve machine-specific values.

---

# Step 9 – Start Application

Start:

```text
TimeZoneAppPool
```

Start:

```text
TimeZone Site
```

Verify:

```text
Site running
Application pool running
```

---

# Step 10 – Validate Installation

Perform validation tests:

## Authentication

- [ ] Login successful
- [ ] Logout successful

---

## Categories

- [ ] Categories load
- [ ] Categories editable

---

## Activities

- [ ] Activities load
- [ ] Activities editable

---

## Time Tracking

- [ ] Start Work
- [ ] Edit Work
- [ ] Stop Work

---

## Reporting

- [ ] Dashboard loads
- [ ] Reports load
- [ ] Time totals appear correct

---

# Post-Upgrade Validation

Verify:

- Existing users remain intact
- Existing activities remain intact
- Existing time entries remain intact
- No data corruption occurred

Confirm with sample records.

---

# Rollback Procedure

If upgrade validation fails:

## Step 1

Stop:

```text
TimeZone Site
```

---

## Step 2

Restore application files.

Restore:

```text
D:\Backups\Application
```

to:

```text
C:\Applications\TimeZone
```

---

## Step 3

Restore database backup.

Using SSMS:

```text
Restore Database
```

from:

```text
TimeZone_YYYYMMDD.bak
```

---

## Step 4

Restart IIS.

---

## Step 5

Validate original version.

Confirm:

- Login works
- Data exists
- Reports function

---

# Database Migration Standards

Future database changes should follow these rules.

## Rule 1

Never drop production tables without an explicit migration strategy.

---

## Rule 2

Prefer additive changes.

Examples:

```sql
ALTER TABLE WorkItems
ADD ActivityNumber INT NULL;
```

Preferred over:

```sql
DROP COLUMN
```

operations.

---

## Rule 3

Backfill data when changing schema.

Example:

```text
Title:
JCAM-7251 Custom Query UI
```

Migration:

```text
ActivityNumber = 7251

Title = Custom Query UI
```

---

## Rule 4

All database changes must be versioned.

Example:

```text
Scripts

    1.0.0

    1.1.0

    1.2.0
```

---

# Configuration Migration Standards

Configuration values should never be overwritten blindly.

Compare:

```text
Old Configuration

New Configuration
```

Manually 