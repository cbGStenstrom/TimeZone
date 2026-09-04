# TimeZone Release Notes

This document tracks all published releases of TimeZone and provides a history of:

- New Features
- Enhancements
- Bug Fixes
- Security Improvements
- Database Changes
- Breaking Changes

---

# Release Numbering

TimeZone uses semantic versioning:

```text
MAJOR.MINOR.PATCH
```

Examples:

```text
1.0.0
1.1.0
1.1.1
2.0.0
```

Definitions:

| Version Component | Meaning |
|------------------|----------|
| MAJOR | Breaking changes or significant new functionality |
| MINOR | New features and enhancements |
| PATCH | Bug fixes and minor improvements |

---

# Upcoming Release

## TimeZone 1.0.0

### Status

In Development

### Release Type

Initial Public Release

---

### Goals

- Productize existing application
- Deploy to IIS
- Document installation process
- Document upgrade process
- Establish architectural standards
- Stabilize existing feature set

---

### New Features

#### TZ-001

Externalized Configuration

- Connection strings retrieved from configuration
- Environment-specific configuration support
- Removal of hard-coded configuration dependencies

---

#### TZ-002

IIS Deployment Support

- IIS deployment documentation
- Application Pool configuration guidance
- Publish workflow documentation

---

#### TZ-003

Installation Guide

- SQL Server installation guidance
- IIS installation guidance
- First-time setup instructions

---

#### TZ-004

Upgrade Process

- Upgrade documentation
- Rollback procedures
- Backup requirements

---

### Enhancements

#### Documentation Foundation

Added:

```text
ProductCharter.md
Architecture.md
Backlog.md
Installation.md
UpgradeGuide.md
ReleaseNotes.md
```

---

### Bug Fixes

None currently.

---

### Breaking Changes

None.

---

### Database Changes

None currently planned.

---

### Known Issues

#### BUG-001

Add Category (Project) capability missing from revised Category Manager.

Status:

```text
Planned for Version 1.1
```

---

#### BUG-002

Legacy Activity Manager and New Activity Manager currently coexist.

Status:

```text
Planned for Version 1.1
```

---

# Planned Release

## TimeZone 1.1.0

### Status

Planned

### Release Type

User Experience Release

---

### Goals

Improve usability and workflow efficiency.

---

### Planned Features

#### FEAT-001

Activity Number Support

Current:

```text
JCAM-7251 Blazor Custom Query UI Component
```

Future:

```text
Category Key: JCAM
Activity Number: 7251
Title: Blazor Custom Query UI Component
```

Display:

```text
JCAM-7251 Blazor Custom Query UI Component
```

---

#### FEAT-002

Submit For Review And Stop Work

New workflow option:

```text
Submit For Review & Stop Work
```

---

#### FEAT-003

Enhanced Activity Filtering

Support filtering by:

- Category
- Status
- Type
- Search Text
- Billable
- In Review

---

#### FEAT-004

Compact User Interface

Goals:

- Reduced spacing
- Higher information density
- Modernized visuals

---

### Bug Fixes

#### BUG-001

Restore Add Category functionality.

---

#### BUG-002

Retire legacy Activity Manager after new manager reaches feature parity.

---

### Breaking Changes

None anticipated.

---

# Planned Release

## TimeZone 1.2.0

### Status

Planned

### Release Type

Reporting Release

---

### Goals

Expand reporting and historical analysis capabilities.

---

### Planned Features

#### FEAT-005

Activity History

Display complete Time Entry history for an Activity.

---

#### FEAT-006

Dashboard Review Queue

Display all activities currently marked:

```text
In Review
```

---

#### FEAT-007

Weekly Summary Report

Display:

- Hours By Category
- Hours By Activity
- Weekly Totals

---

#### FEAT-008

Monthly Summary Report

Display:

- Category Totals
- Activity Totals
- Billable Totals
- Non-Billable Totals

---

### Breaking Changes

None anticipated.

---

# Planned Release

## TimeZone 2.0.0

### Status

Future

### Release Type

Multi-User Release

---

### Goals

Introduce enterprise-ready user and security features.

---

### Planned Features

#### FEAT-009

Role Management

Roles:

```text
SuperUser
Administrator
Developer
```

---

#### FEAT-010

User Administration

Capabilities:

- Create Users
- Disable Users
- Reset Passwords
- Assign Roles

---

#### FEAT-011

Authentication Modernization

Replace custom session authentication with:

- Cookie Authentication
- ClaimsPrincipal
- Authorization Policies

---

### Breaking Changes

Potential schema and authentication changes will be documented in detail before release.

---

# Hotfix Template

Use this section format for future patch releases.

## TimeZone X.Y.Z

### Release Date

YYYY-MM-DD

### Bug Fixes

- Description

### Enhancements

- Description

### Database Changes

- Description

### Breaking Changes

- Description

---

# Release Checklist

Before every release:

- [ ] Code reviewed
- [ ] Build succeeds
- [ ] Database deployment tested
- [ ] Installation guide reviewed
- [ ] Upgrade process tested
- [ ] Release notes updated
- [ ] Git tag created

---

# Support Matrix

| Version | Status |
|----------|----------|
| 1.0.x | Active Development |
| 1.1.x | Planned |
| 1.2.x | Planned |
| 2.0.x | Future |

---

# Change History

| Date | Description |
|--------|-------------|
| 2026-09-03 | Initial Release Notes document created |