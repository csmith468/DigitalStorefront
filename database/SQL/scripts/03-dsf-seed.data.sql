USE DigitalStorefront;
GO

INSERT INTO dsf.Role (roleName, description) VALUES
    ('Admin', 'Full system access including demo product management'),
    ('ProductWriter', 'Can create and edit non-demo products'),
    ('ImageManager', 'Can manage non-demo product images');

INSERT INTO [dsf].[user] (username, firstName, lastName, email, isActive, isAdmin)
VALUES ('seedUser', 'Seed', 'User', NULL, 0, 1);

DECLARE @userId INT = SCOPE_IDENTITY();

INSERT INTO dsf.UserRole (userId, roleId)
SELECT @userId, roleId
FROM dsf.Role
WHERE roleName IN ('Admin', 'ProductWriter', 'ImageManager');

