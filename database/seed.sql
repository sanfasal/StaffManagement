USE StaffManagement;
GO

IF OBJECT_ID(N'dbo.Staff', N'U') IS NULL
BEGIN
    RAISERROR('The dbo.Staff table does not exist. Run database/init.sql first.', 16, 1);
    RETURN;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffId = N'ST001')
BEGIN
    INSERT INTO dbo.Staff (StaffId, FullName, BirthDay, Gender)
    VALUES (N'ST001', N'Alice', '1998-05-12', 2);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffId = N'AB002')
BEGIN
    INSERT INTO dbo.Staff (StaffId, FullName, BirthDay, Gender)
    VALUES (N'AB002', N'Bob', '2001-03-04', 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffId = N'ST003')
BEGIN
    INSERT INTO dbo.Staff (StaffId, FullName, BirthDay, Gender)
    VALUES (N'ST003', N'Carol', '1988-07-20', 2);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffId = N'ST004')
BEGIN
    INSERT INTO dbo.Staff (StaffId, FullName, BirthDay, Gender)
    VALUES (N'ST004', N'Dana', NULL, 1);
END;
GO
