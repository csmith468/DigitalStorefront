CREATE TABLE dsf.[order] (
    orderId INT IDENTITY(1,1) PRIMARY KEY,
    userId INT NULL, -- guest checkout for demo
    stripeSessionId NVARCHAR(200) NULL,
    stripePaymentIntentId NVARCHAR(200) NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    totalCents INT NOT NULL,
    createdAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    updatedAt DATETIME2 NULL DEFAULT GETUTCDATE(),
    paymentCompletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Order_User FOREIGN KEY (userId) REFERENCES dsf.[user](userId),
    CONSTRAINT UQ_Order_StripeSessionId UNIQUE (stripeSessionId),
    CONSTRAINT UQ_Order_StripePaymentIntentId UNIQUE (stripePaymentIntentId)
);

CREATE TABLE dsf.orderItem (
    orderItemId INT IDENTITY(1,1) PRIMARY KEY,
    orderId INT NOT NULL,
    productId INT NOT NULL,
    productName NVARCHAR(200) NOT NULL,
    unitPriceCents INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    createdAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_OrderItem_Order FOREIGN KEY (orderId) REFERENCES dsf.[order](orderId),
    CONSTRAINT FK_OrderItem_Product FOREIGN KEY (productId) REFERENCES dbo.[product](productId)
);

CREATE INDEX IX_Order_UserId ON dsf.[order](userId);
CREATE INDEX IX_Order_Status ON dsf.[order](status);
CREATE INDEX IX_Order_StripeSessionId ON dsf.[Order](StripeSessionId);
CREATE INDEX IX_Order_StripePaymentIntentId ON dsf.[Order](StripePaymentIntentId);
CREATE INDEX IX_OrderItem_OrderId ON dsf.OrderItem(OrderId);