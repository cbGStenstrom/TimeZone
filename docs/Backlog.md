# TimeZone Product Backlog

**Product:** TimeZone - Personal Time Tracker for Software Developers

**Version:** 1.0

**Last Updated:** September 2026

---

# Backlog Prioritization

Priority levels:

| Priority | Description |
|----------|----------|
| Critical | Required for release |
| High | Significant business value |
| Medium | Important enhancement |
| Low | Future improvement |

---

# Sprint 0 - Productization

## TZ-001 - Externalize Configuration

### Priority

Critical

### Status

Completed

### Description

Remove all hard-coded configuration values and ensure all configuration is sourced from application configuration files.

### Acceptance Criteria

- Connection strings stored in configuration.
- No hard-coded connection strings remain in code.
- Environment-specific configuration supported.
- DbContext configured through dependency injection.

---

## TZ-002 - IIS Deployment Process

### Priority

Critical

### Status

Completed

### Description

Create a repeatable deployment process for local IIS installations.

### Acceptance Criteria

- IIS prerequisites documented.
- Application Pool configuration documented.
- Publish process documented.
- Deployment verified from a clean machine.

---

## TZ-003 - Installation Guide

### Priority

Critical

### Status

Not Started

### Description

Create installation documentation.

### Acceptance Criteria

- SQL Server setup documented.
- Database deployment documented.
- IIS deployment documented.
- First-time startup documented.

---

## TZ-004 - Upgrade Strategy

### Priority

Critical

### Status

Not Started

### Description

Define the process for upgrading an existing installation without data loss.

### Acceptance Criteria

- Backup process documented.
- Schema migration process documented.
- Release deployment process documented.
- Rollback process documented.

---

## TZ-005 - Backup and Restore Process

### Priority

High

### Status

Not Started

### Description

Document database backup and restore procedures.

### Acceptance Criteria

- Full backup process documented.
- Restore process documented.
- Recovery verification documented.

---

# Sprint 1 - UX Improvements

## BUG-001 - Restore Add Category Functionality

### Priority

High

### Status

Completed

### Description

Restore the ability to create new Categories (Projects) from Category Management.

### Acceptance Criteria

- New Category button exists.
- Category form opens.
- New category is persisted.
- Grid refreshes automatically.

---

## BUG-002 - Complete New Activity Manager

### Priority

High

### Status

Not Started

### Description

Finish implementation of the new Activity Manager and retire the legacy version.

### Acceptance Criteria

- Feature parity with legacy manager.
- Filtering support.
- Editing support.
- Creation support.
- Deletion support.
- Legacy page removed from navigation.

---

## FEAT-001 - Activity Number Support

### Priority

High

### Status

Not Started

### Description

Separate Activity Number from Activity Title.

### Example

Current:

```text
JCAM-7251 Blazor Custom Query UI Component
```

Future:

```text
Category Key = JCAM
Activity Number = 7251
Title = Blazor Custom Query UI Component
```

Display format:

```text
JCAM-7251 Blazor Custom Query UI Component
```

### Acceptance Criteria

- Activity Number stored separately.
- Existing activities migrated.
- Search supports Activity Number.
- Reporting supports Activity Number.

---

## FEAT-002 - Submit For Review And Stop Work

### Priority

High

### Status

Not Started

### Description

Add a workflow allowing users to submit activities for review while simultaneously stopping work.

### Acceptance Criteria

Add option:

```text
Submit For Review & Stop Work
```

Preserve existing options:

```text
Save Changes
Stop Work
Submit For Review
```

---

## FEAT-003 - Enhanced Activity Filtering

### Priority

High

### Status

Not Started

### Description

Improve activity filtering capabilities.

### Acceptance Criteria

Support filtering by:

- Category
- Status
- Activity Type
- Text Search
- Billable
- In Review

---

## FEAT-004 - Compact User Interface

### Priority

Medium

### Status

Not Started

### Description

Reduce oversized controls and improve information density.

### Acceptance Criteria

- Reduced spacing.
- More efficient layouts.
- Improved grid density.
- Modernized presentation.

---

# Sprint 2 - Reporting

## FEAT-005 - Activity History

### Priority

Medium

### Status

Not Started

### Description

Display all Time Entries associated with an Activity.

### Acceptance Criteria

- History screen exists.
- History dialog exists.
- Time entries displayed chronologically.
- Drill-down from Activity Manager supported.

---

## FEAT-006 - Dashboard Review Queue

### Priority

Medium

### Status

Not Started

### Description

Display all activities currently in review.

### Acceptance Criteria

- Review queue panel added.
- Click-through navigation supported.
- Not limited to current date range.

---

## FEAT-007 - Weekly Summary Report

### Priority

Medium

### Status

Not Started

### Description

Summarize time spent during a selected week.

### Acceptance Criteria

- Hours by Category.
- Hours by Activity.
- Totals displayed.

---

## FEAT-008 - Monthly Summary Report

### Priority

Medium

### Status

Not Started

### Description

Summarize time spent during a selected month.

### Acceptance Criteria

- Category totals.
- Activity totals.
- Billable totals.
- Non-billable totals.

---

# Sprint 3 - Security

## FEAT-009 - Role Management

### Priority

Medium

### Status

Not Started

### Description

Introduce role-based security.

### Roles

```text
SuperUser
Administrator
Developer
```

---

## FEAT-010 - User Administration

### Priority

Medium

### Status

Not Started

### Description

Allow Administrators to manage users.

### Acceptance Criteria

- Create User
- Disable User
- Reset Password
- Assign Role

---

## FEAT-011 - Authentication Modernization

### Priority

Medium

### Status

Not Started

### Description

Move from SessionService authentication to ASP.NET authentication.

### Acceptance Criteria

- Cookie Authentication
- ClaimsPrincipal
- Authorization Policies

---

# Future Ideas

## FEAT-100 - Favorite Activities

Allow users to pin frequently used activities.

---

## FEAT-101 - Recent Activities

Quick access to recently used activities.

---

## FEAT-102 - Export To Excel

Export reports to Excel.

---

## FEAT-103 - Export To PDF

Export reports to PDF.

---

## FEAT-104 - Team Reporting

Cross-user reporting for Administrators.

---

# Release Plan

## Version 1.0

Productization Release

### Target Deliverables

- IIS Deployment
- Installation Documentation
- Upgrade Process
- Existing Feature Stabilization

---

## Version 1.1

User Experience Release

### Target Deliverables

- New Activity Manager
- Activity Number Support
- Workflow Improvements

---

## Version 1.2

Reporting Release

### Target Deliverables

- Activity History
- Dashboard Enhancements
- Weekly/Monthly Reporting

---

## Version 2.0

Multi-User Release

### Target Deliverables

- Roles
- Authorization
- User Administration

---