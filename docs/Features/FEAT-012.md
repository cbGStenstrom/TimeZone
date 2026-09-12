# FEAT-012 Start Work from WorkItem/Activitiy grid

**State:** open

**Created by:** @cbGStenstrom

**Created at:** 2026-09-11 13:45:06.000 UTC

----



User should be able to click on a button on an item in the grid on the **WorkItem Manager** and be able to "Start Work" on it. When doing so it should validate that there is not another workitem already being worked on and prompt the user what to do. 

- "Discard" the current workitem 
- "Close" the current work item
- "Cancel" and leave the current workitem active.

Currently the workflow to start work on a workitem from the WorkItem Manager is 

1. The user visits WorkItem Manger
2. User filters the grid to find a Work Item. 
3. The user then clicks on the "START WORK" button in the footer.
4. The Start Time Entry dialog box opens and the user has to select the Workitem from the dialog box.
5. If there is a WorkItem currently being worked, the "START WORK" button would not be visible, instead an "EDIT WORK" button would be visible.

The new workflow would look like ...

1. The user visits WorkItem Manger
2. User filters the grid to find a Work Item. ​‌
3. The user clicks on the WorkItem record ​‌
4. The Start Time Entry dialog box opens, the selected WorkItem is pre-selected as the WorkItem to start working on.​‌
5. When the user clicks on the WorkItem to start working on it, it is possible that another workitem is already being worked. We will need to detect this scenario, and prompt the user to determine how he/she wants to handle the currently active workitem. ​

----
----