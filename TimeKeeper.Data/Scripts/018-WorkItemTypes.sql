SET XACT_ABORT ON;
BEGIN TRY
 BEGIN TRANSACTION;
 IF OBJECT_ID('dbo.WorkItemTypes') IS NULL
 BEGIN
  IF OBJECT_ID('dbo.WorkItems') IS NOT NULL
   EXEC(N'IF EXISTS(SELECT 1 FROM dbo.WorkItems WHERE WorkitemType NOT BETWEEN 0 AND 5)
    THROW 50018, ''Unknown legacy WorkitemType. Inspect distinct values and resolve their meanings before upgrading.'', 1;');
  CREATE TABLE dbo.WorkItemTypes
(
 Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkItemTypes PRIMARY KEY,
 Name NVARCHAR(120) COLLATE Latin1_General_100_CI_AS NOT NULL,
 Description NVARCHAR(1000) NULL,
 Icon NVARCHAR(120) NULL,
 IsEnabled BIT NOT NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
 CreatedBy NVARCHAR(120) NOT NULL DEFAULT 'AUTO',
 UpdatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
 UpdatedBy NVARCHAR(120) NOT NULL DEFAULT 'AUTO',
 CONSTRAINT UQ_WorkItemTypes_Name UNIQUE(Name),
 CONSTRAINT CK_WorkItemTypes_Name CHECK(LEN(LTRIM(RTRIM(Name))) > 0)
)
  SET IDENTITY_INSERT dbo.WorkItemTypes ON;
  INSERT dbo.WorkItemTypes(Id,Name,IsEnabled) VALUES
   (0,'Bugfix',1),(1,'Chore',1),(2,'Feature',1),(3,'Meeting',1),(4,'PTO',1),(5,'TechDebt',1);
  SET IDENTITY_INSERT dbo.WorkItemTypes OFF;
 END;
 IF OBJECT_ID('dbo.WorkItems') IS NOT NULL
  EXEC(N'IF EXISTS(SELECT 1 FROM dbo.WorkItems w LEFT JOIN dbo.WorkItemTypes t ON w.WorkitemType=t.Id WHERE t.Id IS NULL)
   THROW 50019, ''Orphan WorkitemType values exist. Resolve them before deploying the foreign key.'', 1;');
 COMMIT;
END TRY
BEGIN CATCH
 IF XACT_STATE() <> 0 ROLLBACK;
 THROW;
END CATCH;
