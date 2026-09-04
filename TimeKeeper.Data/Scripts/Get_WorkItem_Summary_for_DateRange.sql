-- SQL Get WorkItem Summary for DateRange
USE TimeKeeperDB;

SELECT 
   [item].ID AS WorkitemID
 , [item].IsOpen
 , [item].Title
 , SUM(DATEDIFF(MINUTE, [time].StartWork, [time].EndWork)) AS HoursWorked 
FROM TimeEntries AS [time]
	INNER JOIN WorkItems AS [item] ON
		[time].WorkItemID = [item].ID
WHERE 
	[time].StartWork >= '09/30/2025 00:00:00'
	AND 
	[time].EndWork <= '10/05/2025 23:59:59'
GROUP BY
	[item].ID, [item].IsOpen, [item].Title


SELECT * FROM WorkItems WHERE ID = 17013;
SELECT * FROM TimeEntries ORDER BY WorkItemID DESC;