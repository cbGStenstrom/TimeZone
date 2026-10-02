# TimeZone: current project context for ChatGPT planning discussions

Snapshot: October 2, 2026. This document describes the current local repository, including existing uncommitted My Day/review/history work. Source presence does not establish deployment status. It contains no feature proposals. Use it as factual context for product and feature discussions; request specific source files when details beyond this summary matter.

## Application purpose and vocabulary

TimeZone is a personal time tracker for software developers. The solution and code retain the name **TimeKeeper**. Users organize activities, record work intervals and accomplishment notes, resume work, submit activities for review, and inspect historical time and summary reports. Repository product/deployment documents describe a local IIS and SQL Server application and a single-user-first product context.

| Product terminology | Code terminology | Role |
| --- | --- | --- |
| User | `Laborer` | Person recording work |
| Category | `Project` | Grouping with key, short name, long name |
| Activity | `WorkItem` | Ticket, task, meeting, or other work |
| Time entry | `TimeEntry` | A work interval and accomplishment notes |

UI labels currently mix these vocabularies. An activity's number is a separate string from its title and integer database ID. `DisplayIdentifier` combines the project's key and activity number, such as `JCAM-7251`.

## Solution/project structure

Primary solution: `TimeKeeper.sln`.

| Project | Current responsibilities and contents |
| --- | --- |
| `TimeKeeper.App` | ASP.NET Core interactive server Blazor application; pages, reusable Razor components, layouts, Radzen UI, session/browser state, startup and DI, UI workflow services |
| `TimeKeeper.Domain` | Domain models and snapshots, service interfaces/implementations, MediatR commands/query handlers, handwritten entity/domain mappers, security/password validators, date utilities, report DTOs |
| `TimeKeeper.DataAccess` | EF Core `Entities/TimeKeeperDbContext.cs`, scaffolded EF entity classes, `EntityPartials/` |
| `TimeKeeper.Data` | SSDT SQL Server database project, `Tables/` definitions, schema comparison artifact, reporting SQL script |
| `TimeKeeper.Domain.Tests` | xUnit tests, currently six tests in `Utilities/DateTimeUtilsTests.cs` |

References: `App -> Domain -> DataAccess`; tests reference Domain. The database project is separately included in the solution.

The .NET projects target `net9.0`, enable nullable types and implicit usings; App sets language version `14.0`. Declared dependencies include Radzen.Blazor 8.4.1, EF Core/SQL Server/Tools 9.0.6, MediatR 13.0.0, MediatR DI extensions 11.1.0, AutoMapper 15.0.0, and ASP.NET Core Identity 2.3.1. Mapping code inspected here uses handwritten extensions, not AutoMapper profiles. The SQL project imports Visual Studio SSDT build targets and has a .NET Framework v4.7.2 project setting.

Important App directories:

```text
Pages/                         Routable screens and .razor.cs code-behind
Components/Dialogs/            Work item and time entry editor dialogs
Components/Forms/              Filters, forms, editable time-entry rows
Components/MyDay/              Daily activity cards and summaries
Components/Reports/            Summary, entry detail, and activity history
Components/TimeEntryComponents/ Newer time-entry editor
Components/WorkItemComponents/  Activity selection list
Shared/                        App shell, component/page bases, layouts
Api/                           DI, session/storage helpers, models, enums
Services/                      WorkLauncherService and interface
NewComponents/                 Shared page header and project grid components
NewApi/                        UI enums including icons and sort directions
wwwroot/                       CSS/LESS, styles, themes, fonts
```

`Api/` is support code, not evidence of a separate HTTP REST API. Folder names and component namespaces do not always match; for example `Shared/App.razor` declares `TimeKeeper.App.Components`.

## Runtime and domain structure

```text
Browser -> interactive server Blazor/Radzen UI
        -> domain service -> IMediator.Send(command/query)
        -> Domain handler -> EF Core DbContext -> SQL Server

EF entity -> mapping extension -> domain model -> component
```

`Program.cs` registers interactive server components, Razor pages/server-side Blazor, Radzen, MediatR handlers, session/storage services, and domain/database services. It configures HTTPS redirection, antiforgery, static assets, and component endpoints.

`Api/DIServices/DIServiceExtensions.cs` registers domain services and `IWorkLauncherService` as transient; service option records are scoped. `SessionService`, `BrowserStorageService`, and theme state are scoped. DbContext is transient and uses SQL Server with connection-string key `TimeKeeperDB`. No configuration values or secrets are reproduced here.

| Concept | Important data/behavior |
| --- | --- |
| `Project` | Key, short/long names, work items, audit fields |
| `WorkItem` | Project ID, ActivityNumber, title, description, type, IsOpen, IsInReview, IsBillable, audit fields |
| `TimeEntry` | Laborer ID, WorkItem ID, nullable StartWork/EndWork, Accomplishment, calculated HoursWorked, audit fields |
| `Laborer` | Name, email, username, password storage/input, related time entries |
| `ModelBase<T>` | Snapshot/revert editing; concrete models implement dirty checks |
| `WorkItemDateRangeSummaryDto` | Work-item flags/title/ID and total minutes for reporting |

Relationships: Project has many WorkItems; WorkItem and Laborer each have many TimeEntries. Activity types: Bugfix, Chore, Feature, Meeting, PTO, TechDebt. Open/review/billable are independent flags, not one unified lifecycle enum.

## Shared shell and navigation

Most main pages explicitly use `Shared/Layout/AppLayout.razor(.cs)`. Display structure:

1. Global `RadzenDialog` host.
2. Left sidebar, alongside the page body.
3. Page body, receiving named cascading values `Layout` and `ActiveTimeEntry`.
4. Footer containing horizontal `Components/TimeEntryStatusComponent.razor(.cs)`.

Sidebar order and destination:

| Item | Behavior/destination |
| --- | --- |
| Timekeeper brand | Toggles sidebar between icons and icons/text |
| Home | `/` |
| Dashboard | `/Dashboard` |
| Project Mgr | `/ProjectManager` |
| Workitem Mgr | `/WorkItemManager` |
| Time Entries | `/TimeEntryManager` |
| Log Out | Clears session user and navigates to `/login` |
| Test Page | `/TestPage` |
| My Day | `/MyDay` |

The shell footer shows **Edit Work**, work-item identifier/title, and start time when work is active; otherwise it shows **Start Work**. Both paths use `TimeEntryEditorDialog`. Layout methods refresh shared active-entry state after saves.

`Components/Routes.razor` uses `RouteView` with `MainLayout` as default and focus-on-navigation targeting `h1`. Main pages override that default with AppLayout. `Shared/Layout/MainLayout.razor` is a separate older shell. Login uses `SplashScreenLayout`, which renders only its body. AppLayout explicitly uses interactive server rendering with prerender disabled.

## Pages: visible composition, behavior, and dependencies

Display order below is top-to-bottom; columns and horizontal controls are listed left-to-right. Main AppLayout pages also receive the sidebar and active-work footer described above. `.razor(.cs)` means markup and its corresponding code-behind file.

### Home — `/`

File: `Pages/HomePage.razor(.cs)`. Layout: AppLayout; base: CbPageBase.

Visible order:

1. Header greeting, `Hello {FirstName}!!`.
2. Vertical `TimeEntryStatusComponent`: active-work edit link/details, or Start Work.

A larger editor card remains in markup but is explicitly `Visible="false"`; its activity lookup, dropdown, accomplishment editor, and submit/start controls are not a visible Home module.

Dependencies/concepts: SessionService/Laborer for greeting; ITimeEntryService for active work and changes; IWorkItemService/IProjectService support the retained editing code. The footer duplicates the status function horizontally. No dedicated history navigation is rendered on this page.

### My Day — `/MyDay`

File: `Pages/MyDayPage.razor(.cs)`. Layout: AppLayout; base: CbPageBase.

Visible order:

1. Header: My Day, current date label.
2. `Components/MyDay/CurrentActivityCardComponent.razor(.cs)`: Currently Working On, identifier/title and start timestamp, or No Active Work.
3. `ContinueWorkingComponent.razor(.cs)`: Continue Working heading, recent activity rows with resume icon, identifier/title, and last-worked text.
4. `ActivitiesInReviewComponent.razor(.cs)`: Activities In Review heading; resume, identifier/title, last-worked value, View History per row; explicit empty message when none.
5. Repeated `YesterdayActivitySummaryComponent.razor(.cs)` cards: expand icon, identifier/title, time logged, entry count, last worked. Expanded content shows entry start times and accomplishment HTML, then Resume Work and View History buttons.
6. `TimeEntryEditorDialog` is declared after the page layout and appears only when opened.

Dependencies: ITimeEntryService, IWorkItemService, IWorkLauncherService, session user, DialogService, NavigationManager. View models are `Models/MyDayActivitySummary.cs` and `MyDayActivityEntry.cs`.

Recent activities are grouped from entries created in the last seven days, ordered by last work, limited to five. Review activities use all work items flagged IsInReview and their last recorded work; the review queue is not limited to yesterday/recent dates. Yesterday groups entries by activity using CreatedDate bounds and sums calculated entry hours. Only one yesterday card is expanded at a time. Review and yesterday history buttons navigate to `/WorkItemHistory/{id}`; resume invokes WorkLauncherService and the time-entry dialog.

`YesterdayAccomplishmentsCardComponent.razor(.cs)` also exists but is not mounted in the current My Day markup.

### Dashboard — `/Dashboard`

File: `Pages/DashboardPage.razor(.cs)`. Layout: AppLayout; base: CbPageBase.

Visible order:

1. Header: Dashboard and date label.
2. Filter card: Work Day/Work Week dropdown, then selected-date picker.
3. `Components/Reports/WorkSummaryComponent.razor(.cs)`:
   - Metric tiles left-to-right: Total Hours, Billable Hours, Non-Billable Hours, Workitems Closed, Workitems In Review.
   - Activity summary list or empty-period message. Rows show status/billable icons, title, and hours; list includes a total.
   - When an activity is selected, a neighboring detail card with close button and `WorkItemEntriesComponent.razor(.cs)`.
4. Page-local footer containing two literal planning-note sentences about review/history. These are rendered text, not additional implemented modules.

WorkItemEntriesComponent renders a heading and repeated `WorkItemEntriesItemComponent.razor(.cs)` entries. Each entry shows edit icon, date interval, hours, then accomplishment HTML. Edit mode replaces notes with an HTML editor followed by Cancel/Save.

Dependencies: IWorkItemService for date-range summary DTOs; ITimeEntryService for entry detail; DateTimeUtils for boundaries/rounding; session user for editing. Metric tile clicks filter the list. Activity row clicks select detail within WorkSummaryComponent; this visible drill-down is not a route transition to the standalone history page.

### Work Item Manager — `/WorkItemManager`

File: `Pages/WorkItemMgrPage.razor(.cs)`. Layout: AppLayout; base: CbPageBase.

Visible order:

1. `NewComponents/AppLayoutHeaderComponent.razor(.cs)` with Work Item Manager title.
2. Toolbar: New Work Item, then History link.
3. Search Term input, then `Components/Forms/WorkItemLookupFilter.razor(.cs)`.
4. Compact RadzenDataGrid. Column order: action buttons (start/edit/delete), Type, Activity Number, Title, Open, Review, Billable, Project key.
5. TimeEntryEditorDialog declaration, visible only when opened.

Grid permits sorting/resizing, disables paging and built-in filtering, and supports row double-click editing. Search checks title, ActivityNumber, and Project.Key. WorkItemLookupFilter renders project selector, open/review status selector, billable selector, Clear Filter; status defaults to Open. New/edit opens `Components/Dialogs/WorkItemEditorDialog.razor(.cs)`, containing `WorkItemForm.razor(.cs)` and Cancel/Save. The form captures project, type, activity number, title, open/review/billable flags, and description.

Dependencies: IWorkItemService, ITimeEntryService (including associated-entry count for deletion), IProjectService through filter/form, IWorkLauncherService, DialogService, session user. Deleting an activity with recorded time is blocked by this page. Starting work uses the shared launcher. History links to bare `/WorkItemHistory` without a selected ID.

### Activity history — `/WorkItemHistory`, `/WorkItemHistory/{WorkItemId:int}`

File: `Pages/WorkItemHistoryPage.razor(.cs)`; contains `Components/Reports/WorkItemHistoryComponent.razor(.cs)`. Layout: AppLayout. Page inherits CbComponentBase rather than CbPageBase; the layout still checks login.

Visible order:

1. Without an ID: heading `WorkItemHistoryComponent` only.
2. With an ID that is not found: Activity not found message.
3. With a matching activity: identifier/title heading, then entry components, or No time entries for this activity.

Entries use WorkItemEntriesItemComponent, showing date interval/hours and accomplishment text with inline editing. They sort by StartWork descending, then ID descending. Dependencies: IWorkItemService and ITimeEntryService; editing uses the session user and snapshots. My Day supplies activity IDs to this route. The bare route has no activity selector in its current markup.

### Time Entry Manager — `/TimeEntryManager`

File: `Pages/TimeEntryManagerPage.razor(.cs)`. Layout: AppLayout; base: CbPageBase.

Visible order:

1. Time Entry Manager heading and separator.
2. New Time Entry button.
3. Column labels: WorkItem, Accomplishment, Start, End.
4. `Components/Forms/TimeEntryFilter.razor(.cs)`: start/end date pickers, Apply Filter; activity dropdown code is commented out.
5. Repeated `TimeEntryRow.razor(.cs)` rows ordered by StartWork descending, separated by rules.
6. Total hours row.
7. TimeEntryEditorDialog declaration, opened for insertion.

Dependencies: ITimeEntryService for load/create/update/delete; IWorkItemService for row activity choices; SessionService for actor context. Rows support editing/saving/deleting existing entries. New entry uses the same editor dialog with an insertion action. This is a record-management screen, distinct from the active-work footer.

### Project Manager — `/ProjectManager`

File: `Pages/ProjectManagerPage.razor(.cs)`. Layout: AppLayout; base: CbPageBase. This is the sidebar's project destination.

Visible order:

1. AppLayoutHeaderComponent with Project Manager title.
2. `NewComponents/ProjectComponents/ProjectGridComponent.razor(.cs)` header: add button, Acronym, Short Name, Long Name, Updated By, Updated Date.
3. Repeated `ProjectGridRowComponent.razor(.cs)` rows in the same column order. Read rows show edit/delete controls; editable rows show cancel/save controls and text inputs for key/short/long names. New rows begin editable.

Dependencies: IProjectService for project CRUD, session Laborer for audit actor, DialogService for feedback. Grid headings implement sorting. Grid/row EventCallbacks delegate persistence to the page; edits use domain snapshots.

### Project Management — `/ProjectManagement`

File: `Pages/ProjectManagementPage.razor(.cs)`. Layout: AppLayout; base: CbPageBase. A separate existing project-management route, not the current sidebar Project Mgr target.

Visible order:

1. Project Manager heading.
2. New Project button and project-selection dropdown.
3. When a project is selected: `Components/Forms/ProjectForm.razor(.cs)` panel.
4. Audit panel: Last Updated On, Last Updated By, Created Date, Created By.
5. Cancel, Delete, Save buttons, with state-based disabled conditions.

Dependencies: IProjectService, SessionService, Project snapshots/dirty checks. This page uses fixed 900px content widths and a selection/form workflow rather than the newer inline grid.

### Login — `/login`

File: `Pages/LoginPage.razor(.cs)`. Layout: SplashScreenLayout; inherits ComponentBase.

Visible order: centered panel title; username input; password input; optional user message; Create New User and Login buttons. In create-user mode the inputs are replaced by `Components/Forms/CreateUserForm.razor(.cs)`, optional message, then Cancel and Save User buttons.

Dependencies: ILaborerService for credentials/account creation, SessionService, NavigationManager, Laborer and password validation. Successful login assigns SessionService.User and navigates to `/`. It does not use the main sidebar/footer.

### Test and error routes

`Pages/TestPage.razor(.cs)`, `/TestPage`, AppLayout/CbPageBase: Click Me button, numeric work-item ID input, Open Time Entry Editor button; conditionally selected-item ID/title/description/open/review details followed by an accordion of entries. Uses IWorkItemService, ITimeEntryService, DialogService and TimeEntryEditorComponent. A `TimeEntryComponent` reference is present in its accordion markup; no corresponding file was found in the inspected source inventory, so its rendering is not verified here.

`Pages/ErrorPage.razor`, `/Error`: error headings, optional request ID, Development Mode heading and explanatory text. Uses Activity/HttpContext for request identification; no explicit AppLayout declaration, so router default layout applies.

## UI and layout conventions

- Razor markup usually pairs with `.razor.cs` partial classes. `Shared/CbComponentBase.cs` injects domain/Radzen/JS/navigation/session services and exposes cascading Layout; `Shared/CbPageBase.cs` adds session checks and cascading ActiveTimeEntry.
- Radzen stacks, rows/columns, cards, layouts, grids, dialogs, date pickers and HTML editors are the main UI primitives. Newer page headers often use a 3.7rem title/date area or AppLayoutHeaderComponent. Some older pages use h2 headings and horizontal separators instead.
- AppLayout uses full viewport height, zero body padding, a non-responsive left sidebar, and active-work footer. Pages/components set their own spacing and widths; several content/dialog/detail areas have fixed widths.
- Styling uses `cb-*` spacing/typography/component classes, Radzen `rz-*` utilities and CSS variables, custom LESS/CSS palettes, and local fonts. `compilerconfig.json` configures stylesheet compilation. `Shared/App.razor` loads Radzen's software theme, app/scoped styles, Bootstrap 5.3.7 from CDN, Blazor and Radzen scripts.
- Accomplishments are edited as HTML and rendered with MarkupString in daily/history/detail views. They are not uniformly plain-text notes.
- Dialog workflows use references, DialogService, and EventCallbacks. CbComponentBase persists dialog geometry under localStorage key `DialogSettings`. Parent callbacks reload records and refresh shared active-work state.

## Existing workflow conventions and features

`Services/WorkLauncherService.cs` implements the shared start/resume behavior used by My Day and the activity manager: no active entry opens a new start dialog; the same activity opens the existing entry for editing; another activity prompts to stop the current activity and start the selected one. On acceptance it saves the current end timestamp before opening the new dialog. Those operations are separate.

`Components/Dialogs/TimeEntryEditorDialog.razor(.cs)` hosts `Components/TimeEntryComponents/TimeEntryEditorComponent.razor(.cs)`. Editor order: select/change activity and title, conditionally visible start/end date/time inputs, accomplishment HTML editor, dialog action row. Existing entries show Save split-button with Stop Work and Submit for Review; new entries show Save. Discard Time Entry and Cancel follow. Activity selection opens `Components/WorkItemComponents/WorkItemListComponent.razor(.cs)`.

In the current editor handler, Stop Work finalizes EndWork; Submit for Review saves and sets WorkItem.IsInReview, without automatically finalizing EndWork. `TimeEntrySaveActions` contains StartWork, Update, ContinueLater, PRSubmitted, Complete, InsertTimeEntry, and None; enum values are internal actions and do not all correspond to visible buttons. Domain EndWorkOnWorkItem also supports review/close flags. Older `TimeEntryStartWorkComponent`, `TimeEntryEndWorkComponent`, and forms remain in the tree alongside the newer editor.

Major implemented source features include custom login/account creation, project CRUD, activity CRUD/search/status and billable filters, separate activity numbers, start/resume/switch/stop time workflows, manual time-entry management, rich accomplishment editing, daily/recent/review views, linked activity history, and day/week summaries with metric filtering and entry drill-down. Existing feature/backlog documents can lag the code; they are not a definitive inventory of delivered behavior.

## Constraints and coupling relevant to feature discussions

- Architectural guidance places business rules in Domain and database access in handlers/EF. Current filtered service APIs accept expression trees over EF entities, and some UI callers build them. Presentation therefore has knowledge of persistence types. UI pages also perform aggregation and some workflow logic.
- SessionService.User is custom circuit-scoped login state. CbPageBase and AppLayout redirect to login when it is absent. Program does not configure a standard authentication/authorization pipeline. Password utilities use Identity's PasswordHasher and custom validators; this is not a full Identity user/role system.
- GetActiveTimeEntry returns the first entry with null EndWork, without a laborer predicate or explicit order. Active-work state is shared through layout cascading values and manually refreshed callbacks. Per-user isolation and a database-enforced one-active-entry invariant are not established by this implementation.
- Mappers generally store UTC and return local timestamps. Local conversion uses TimeZoneInfo.Local, and UI uses DateTime.Now/Today, so the server timezone determines local dates. SQL fields are DATETIME without timezone metadata; some UI filter predicates compare local bounds directly with persisted values.
- My Day uses CreatedDate for recent/yesterday selection. Date-range summary queries instead test whether StartWork or EndWork falls within bounds and sum full durations. Entry-detail queries require both start and end within bounds. These screens do not share one interval-selection rule.
- TimeEntry.HoursWorked and DateTimeUtils round to quarter hours using minute bands 0-8, 9-21, 22-36, 37-50, 51-59. Incomplete TimeEntry.HoursWorked is zero; report detail can display N/A. Summing rounded entry hours differs from rounding aggregated minutes.
- SQL foreign keys cascade project deletion to activities and activity/laborer deletion to entries. The activity manager blocks deletion when entries exist, but schema behavior and UI guards are separate concerns.
- The SQL WorkItems schema contains additional WorkItemNumber, IsComplete, IsDeployed, ProductionVersion, ReviewDate fields absent from the inspected domain model. Schema presence alone does not establish corresponding UI workflows.
- Older/newer shells, forms, editor paths, and two project-management pages coexist. The current sidebar and rendered markup establish which paths are exposed. AppLayout retains an unused branch navigating to `/WorkItemManagerOld`; no matching @page route was found.
- Database definitions, scaffolded mappings, domain models and mappers are separate artifacts. `DataAccess/ScaffoldCommands.txt` documents scaffolding; generated files must not be edited under repository rules.

## Build/testing conventions from AGENTS.md

```powershell
msbuild TimeKeeper.sln
dotnet test TimeKeeper.Domain.Tests/TimeKeeper.Domain.Tests.csproj
```

AGENTS.md requires understanding the change, inspecting relevant code, producing a plan, making the smallest reasonable implementation, building, running relevant tests, and explaining changes when modifying code. It requires preserving architecture unless the task calls for a change, retaining tests, explaining warning suppression, avoiding generated-file edits and unnecessary NuGet packages, and never committing secrets.

Full-solution builds require compatible .NET/C# tooling and Visual Studio SSDT targets. Current tests cover selected DateTimeUtils date/hour conversion and rounding cases; they do not validate UI workflows, authentication or persistence. This documentation-only task does not establish a fresh build/test result.

## Reference documents and snapshot scope

Existing context: `docs/ProductCharter.md`, `docs/Architecture.md`, `docs/Features/`, `docs/Backlog.md`, `docs/TechDebt/`, `docs/Installation.md`, `docs/Deployment.md`, `docs/UpgradeGuide.md`. These include product intentions and historical planning text; this document's screen inventory is based on current source.

Local modifications already present include MyDayPage, ActivitiesInReviewComponent, WorkItemHistoryPage and WorkItemHistoryComponent. Their current composition is included above. The live IIS instance, live database, browser rendering, and completeness of pending changes were not verified. Configuration values, credentials, and private deployment details are intentionally excluded.
