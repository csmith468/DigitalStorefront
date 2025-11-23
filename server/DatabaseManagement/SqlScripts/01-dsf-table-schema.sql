IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dsf')
BEGIN
    EXEC('CREATE SCHEMA [dsf]')
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'user' AND schema_id = SCHEMA_ID('dsf'))
    BEGIN
        CREATE TABLE dsf.[user] (
            userId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            username NVARCHAR(50) NOT NULL,
            firstName NVARCHAR(50) NULL,
            lastName NVARCHAR(50) NULL,
            email NVARCHAR(50) NULL,
            isActive BIT NOT NULL DEFAULT 1,
            createdAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            updatedAt DATETIME2 NULL
        );
    END 
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'auth' AND schema_id = SCHEMA_ID('dsf'))
    BEGIN
        CREATE TABLE dsf.auth (
            authId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            userId INT NOT NULL REFERENCES [dsf].[user]([userId]),
            passwordHash VARBINARY(MAX) NOT NULL,
            passwordSalt VARBINARY(MAX) NOT NULL,
        
            CONSTRAINT FK_Auth_User 
                FOREIGN KEY (userId) REFERENCES dsf.[user](userId) ON DELETE CASCADE,
        );
    END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'role' AND schema_id = SCHEMA_ID('dsf'))
    BEGIN
        CREATE TABLE dsf.role (
            roleId INT PRIMARY KEY,
            roleName VARCHAR(50) NOT NULL UNIQUE,
            description VARCHAR(255) NULL,
            createdAt DATETIME2 DEFAULT GETUTCDATE()
        );
    END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'userRole' AND schema_id = SCHEMA_ID('dsf'))
    BEGIN
        CREATE TABLE dsf.userRole (
            userRoleId INT PRIMARY KEY IDENTITY(1,1),
            userId INT NOT NULL,
            roleId INT NOT NULL,
            createdAt DATETIME2 DEFAULT GETUTCDATE(),
        
            CONSTRAINT FK_UserRole_User 
                FOREIGN KEY (userId) REFERENCES dsf.[user](userId) ON DELETE CASCADE,
            CONSTRAINT FK_UserRole_Role 
                FOREIGN KEY (roleId) REFERENCES dsf.role(roleId) ON DELETE CASCADE,
            CONSTRAINT UQ_UserRole UNIQUE (userId, roleId)
        );
    END
GO