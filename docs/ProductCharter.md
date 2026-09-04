# TimeZone Product Charter

**Product:** TimeZone - Personal Time Tracker for Software Developers

**Version:** 1.0

**Status:** Successfully deployed under IIS

**Last Updated:** September 2026

**Architecture:** Established

**Documentation:** Validated

**Roadmap:** Defined

**Current Focus:** Sprint 1 - UX Improvements

---

# Product Overview

## Mission Statement

TimeZone enables software developers to accurately track how work time is spent throughout the day, organize work by activity, maintain historical records of accomplishments, and generate meaningful reports.

The application should remain lightweight enough for individual developers while evolving toward support for teams and organizations.

---

# Problem Statement

Developers routinely split their time across:

- Meetings
- Feature Development
- Defect Resolution
- Research
- Documentation
- Deployments
- Administrative Tasks

Many organizations require reporting on this effort, but developers often lack an effective mechanism to answer questions such as:

- What did I work on today?
- Where was my time spent this week?
- How much time was allocated to Project X?
- How much time was spent on meetings versus development?
- What accomplishments were completed during a given period?

TimeZone exists to answer these questions.

---

# Target Audience

## Phase 1

### Individual Developers

- Software Engineers
- Architects
- Consultants
- Team Leads

---

## Phase 2

### Small Teams

- Internal Development Teams
- Government Contractors
- Consulting Organizations

---

## Phase 3

### Organizations

- Software Development Departments
- Consulting Firms
- Managed Service Providers

---

# Product Principles

## Tracking Must Be Fast

Recording work activity should require only a few seconds.

## Activity-Centric Design

Users think in terms of:

- Tickets
- Features
- Defects
- Tasks
- Activities

The Activity (Work Item) is the center of the application.

## Preserve Context

Every Time Entry should answer:

- What was worked on?
- When was it worked on?
- How long was spent?
- What was accomplished?

## Local First

The application must function successfully in:

- Local IIS
- SQL Server
- Single User

before prioritizing enterprise scenarios.

---

# Business Terminology

| Current | Future Business Term |
|----------|----------|
| Laborer | User |
| Project | Category |
| WorkItem | Activity |
| TimeEntry | Time Entry |

---

# Version Roadmap

## Version 1.0

- IIS Deployment
- Installation Documentation
- Upgrade Strategy
- Configuration Management
- Existing Feature Stabilization

## Version 1.1

- Restore Add Category Functionality
- Complete New Activity Manager
- Activity Number Support
- Enhanced Filtering
- Improved Workflow

## Version 2.0

- Roles
- Authorization
- User Administration

---

# Definition of Success

TimeZone Version 1.0 will be considered successful when:

- The application runs from IIS.
- Visual Studio is not required for daily use.
- Installation steps are documented.
- Upgrade steps are documented.
- Existing functionality is preserved.
- Future development can proceed using an established roadmap.




