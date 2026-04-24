IF DB_ID(N'StaffManagement') IS NULL
BEGIN
    CREATE DATABASE StaffManagement;
END;
GO

USE StaffManagement;
GO

IF OBJECT_ID(N'dbo.Staff', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Staff
    (
        StaffId NVARCHAR(8) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        BirthDay DATE NULL,
        Gender INT NULL,
        CONSTRAINT PK_Staff PRIMARY KEY (StaffId)
    );
END;
GO
