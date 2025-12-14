ALTER TABLE dsf.[order]
ADD email NVARCHAR(100) NULL;

-- I'm just going to put the template into C# but I'd likely make a template table
-- and not explicitly include orderId if using this for more than just orders
CREATE TABLE dsf.emailDelivery (
    emailDeliveryId INT IDENTITY (1,1) PRIMARY KEY,
    orderId INT NOT NULL,
    email NVARCHAR(50) NOT NULL,
    subject NVARCHAR(200) NOT NULL,
    body NVARCHAR(MAX) NOT NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    attemptCount INT NOT NULL DEFAULT 0,
    lastAttemptAt DATETIME2 NULL,
    sentAt DATETIME2 NULL,
    failedReason NVARCHAR(500) NULL,
    createdAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    updatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_EmailDelivery_Order FOREIGN KEY (orderId) REFERENCES dsf.[order](orderId)
);

CREATE INDEX IX_EmailDelivery_Status ON dsf.emailDelivery(status) 
    WHERE status IN ('Pending', 'Failed');