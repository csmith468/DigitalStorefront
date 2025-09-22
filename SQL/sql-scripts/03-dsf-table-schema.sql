USE DigitalStorefront;
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dsf')
BEGIN
    EXEC('CREATE SCHEMA [dsf]')
END
GO

CREATE TABLE [dsf].[user] (
    [userId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [username] NVARCHAR(50) NOT NULL,
    [firstName] NVARCHAR(50) NULL,
    [lastName] NVARCHAR(50) NULL,
    [email] NVARCHAR(50) NULL,
    [isActive] BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE [dsf].[auth] (
    [authId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [userId] INT NOT NULL REFERENCES [dsf].[user]([userId]),
    [passwordHash] VARBINARY(MAX) NOT NULL,
    [passwordSalt] VARBINARY(MAX) NOT NULL
);
GO