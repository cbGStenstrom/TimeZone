# Current Sprint

## In Progress

- FEAT-001 Activity Number Support

## Next

- FEAT-012 Start Work from WorkItem Grid
- FEAT-013 Daily Standup

## Completed

- TZ-001 Configuration Cleanup
- TZ-002 IIS Deployment
- BUG-001 Add Project
- BUG-002 Complete New WorkItem Manager


---
---



# FEAT-001 - Activity Number Support

**State:** open

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

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-11 13:45:06.000 UTC

----

User should be able to click on a button on an item in the grid on the **WorkItem Manager** and be able to "Start Work" on it. When doing so it should validate that there is not another workitem already being worked on and prompt the user what to do. 

- "Discard" the current workitem 
- "Close" the current work item
- "Cancel" and leave the current workitem active.


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
