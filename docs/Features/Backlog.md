# Current Sprint

## In Progress

- FEAT-012 Start Work from WorkItem Grid

## Next

- FEAT-013 Daily Standup

## Completed

- TZ-001 Configuration Cleanup
- TZ-002 IIS Deployment
- BUG-001 Add Project
- BUG-002 Complete New WorkItem Manager
- FEAT-001 Activity Number Support


---
---



# FEAT-001 - Activity Number Support

**State:** closed

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:17:06.000 UTC

----

Separate Activity Number from Activity Title.

**Example**
Current:
`JCAM-7251 Blazor Custom Query UI Component`

Future:

```
Category Key = JCAM
Activity Number = 7251
Title = Blazor Custom Query UI Component
```

Display format:

`JCAM-7251 Blazor Custom Query UI Component`


**Acceptance Criteria**

- Activity Number stored separately.
- Existing activities migrated.
- Search supports Activity Number.
- Reporting supports Activity Number.

----
----

# FEAT-002 - Submit For Review And Stop Work

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:18:16.000 UTC

----

Add a workflow allowing users to submit activities for review while simultaneously stopping work.

**Acceptance Criteria**
Add option:

`Submit For Review & Stop Work`


Preserve existing options:

```
Save Changes
Stop Work
Submit For Review
```

----
----

# FEAT-003 - Enhanced Activity Filtering

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:19:02.000 UTC

----

Improve activity filtering capabilities.

**Acceptance Criteria**
Support filtering by:

- Category
- Status
- Activity Type
- Text Search
- Billable
- In Review
----
----
# FEAT-004 - Compact User Interface On Workitem Mgr

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:42:57.000 UTC

----

Reduce oversized controls and improve information density.

**Acceptance Criteria**

- Reduced spacing.
- More efficient layouts.
- Improved grid density.
- Modernized presentation.
----
----
# FEAT-005 - Activity History

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:46:59.000 UTC

----

Display all Time Entries associated with an Activity.

**Acceptance Criteria**

- History screen exists.
- History dialog exists.
- Time entries displayed chronologically.
- Drill-down from Activity Manager supported.
----
----
# FEAT-006 - Dashboard Review Queue

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:47:34.000 UTC

----

Display all activities currently in review.

**Acceptance Criteria**

- Review queue panel added.
- Click-through navigation supported.
- Not limited to current date range.
----
----# FEAT-007 - Weekly Summary Report

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:48:09.000 UTC

----

Summarize time spent during a selected week.

**Acceptance Criteria**

- Hours by Category.
- Hours by Activity.
- Totals displayed.

----
----

# FEAT-008 - Monthly Summary Report

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:48:40.000 UTC

----

Summarize time spent during a selected month.

**Acceptance Criteria**

- Category totals.
- Activity totals.
- Billable totals.
- Non-billable totals.

----
----

# FEAT-009 - Role Management

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:51:02.000 UTC

----

Introduce role-based security.

**Roles**
```
SuperUser
Administrator
Developer
```

----
----
# FEAT-010 - User Administration

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:51:55.000 UTC

----

Allow Administrators to manage users.

**Acceptance Criteria**

- Create User
- Disable User
- Reset Password
- Assign Role


----
----
# FEAT-011 - Authentication Modernization

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:52:45.000 UTC

----

Move from SessionService authentication to ASP.NET authentication.

**Acceptance Criteria**

- Cookie Authentication
- ClaimsPrincipal
- Authorization Policies

----
----

# FEAT-012 Start Work from WorkItem/Activitiy grid

# FEAT-012 - Start Work from WorkItem Grid

**Status**: Closed

**Completed**: 2026-09-23


## Summary

Implemented the ability to start work directly from the Work Item Manager without requiring the user to manually select the WorkItem from the Start Time Entry dialog.

The new workflow significantly reduces the number of clicks required to begin work and makes the WorkItem Manager the primary entry point for daily activity tracking.

### Changes Implemented
#### WorkItem Manager Enhancements

Added a **Start Work** action to the WorkItem Manager grid.

New Actions Column:
```
▶ Start Work
✏ Edit Work Item
🗑 Delete Work Item
```

Users can now initiate work directly from the selected WorkItem.

---

#### Time Entry Workflow Integration

Refactored the workflow to reuse the existing:

```
TimeEntryEditorDialog
TimeEntryEditorComponent
```

instead of introducing a separate start-work experience.

Benefits:

- Consistent user experience
- Single editing workflow
- Reduced code duplication
- Improved maintainability
---

#### Automatic WorkItem Selection

When starting work from the WorkItem grid:

```
WorkItem Manager
    ↓
Click ▶
    ↓
Time Entry Editor Opens
    ↓
Selected WorkItem Pre-Populated
```

The user no longer needs to select the WorkItem from within the editor.

---

#### Active Time Entry Conflict Detection

Implemented validation to prevent multiple concurrent active TimeEntries.

#### No Active Time Entry

``` 
Start Work 
```
opens normally.

#### Active Entry For Same WorkItem

```
▶ Same WorkItem
```

opens the existing active TimeEntry for editing.

No duplicate TimeEntry is created.


#### Active Entry For Different WorkItem

User is prompted:
```
Current Activity:
    <Current Activity>
 
Selected Activity:
    <Selected Activity>
 
[ Stop Current And Start New ]
[ Continue Current Activity ]
```
This ensures only one active TimeEntry exists at any given time.

### Benefits
- Reduced clicks when beginning work.
- Improved daily workflow efficiency.
- Prevented duplicate active TimeEntries.
- Improved integration between Activity Management and Time Tracking.
- Established a unified TimeEntry editing experience.
- Improved overall usability of the WorkItem Manager.


### Technical Notes
#### New Behavior
```
▶ Same Activity
    → Open Existing Active Entry
 
▶ Different Activity
    → Prompt User
 
▶ No Active Activity
    → Start New Entry
```

### Architectural Direction

This feature further establishes:

```
TimeEntryEditorDialog
TimeEntryEditorComponent
```

as the preferred workflow for creating and maintaining TimeEntries.

Future technical debt item:

```
TECH-002
Evaluate retirement of:
 
- TimeEntryStartWorkComponent
- TimeEntryEndWorkComponent
 
and standardize all TimeEntry workflows through
TimeEntryEditorComponent.
 
```

### Acceptance Criteria

| Requirement	 | Status |
|:---------|:---------|
| Start Work available from WorkItem Manager	   | ✅   |
| WorkItem pre-selected	| ✅ |
| User may enter accomplishments when starting work |	✅|
| Existing Time Entry Editor reused	| ✅| 
| Active TimeEntry conflicts detected	| ✅| 
| Same WorkItem opens existing TimeEntry | ✅| 
| Different WorkItem prompts user	| ✅| 
| Duplicate active TimeEntries prevented	| ✅| 
| Workflow fully tested	| ✅| 

----
----

# FEAT-013 Daily Standup

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-11 14:53:05.000 UTC

----

Create a page which provides a "Daily Standup" report for the user, describing anything completed the day before. Maybe allowing him/her to choose what they are working on today?


----
----

# FEAT-100 - Favorite Activities

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:53:11.000 UTC

----

Allow users to pin frequently used activities.


----
----
# FEAT-100 - Favorite Activities

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:53:11.000 UTC

----

Allow users to pin frequently used activities.


----
----

# FEAT-102 - Export To Excel

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:53:45.000 UTC

----

Export reports to Excel.

----
----
# FEAT-103 - Export To PDF

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:54:06.000 UTC

----

Export reports to PDF.


----
----
# FEAT-104 - Team Reporting

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-04 17:54:21.000 UTC

----

Cross-user reporting for Administrators.


----
----

# TECH-002 Consolidate all Start/Edit workflows into TimeEntryEditorComponent.

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-12 06:29:05.000 UTC

---


TECH-002

- Retire TimeEntryStartWorkComponent- 
- Retire TimeEntryEndWorkComponent- 
- Consolidate all Start/Edit workflows into TimeEntryEditorComponent.

---
---