CREATE TABLE dbo.idempotencyKey (
    idempotencyKeyId INT IDENTITY(1,1) PRIMARY KEY,
    clientKey NVARCHAR(100) NOT NULL,
    endpoint NVARCHAR(200) NOT NULL,
    requestHash NVARCHAR(100) NOT NULL,
    statusCode INT NOT NULL,
    response NVARCHAR(MAX) NOT NULL,
    createdAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    expiresAt DATETIME2 NOT NULL,

    CONSTRAINT UQ_IdempotencyKey_ClientKey_Endpoint UNIQUE (clientKey, endpoint)
);

CREATE INDEX IX_IdempotencyKey_ExpiresAt ON dbo.idempotencyKey(expiresAt);