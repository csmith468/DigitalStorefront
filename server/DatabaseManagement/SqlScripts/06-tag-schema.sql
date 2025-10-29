IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tag' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.tag (
             tagId INT IDENTITY(1,1) PRIMARY KEY,
             name NVARCHAR(50) NOT NULL UNIQUE,
             createdAt DATETIME2 DEFAULT GETUTCDATE()
        );
    END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'productTag' AND schema_id = SCHEMA_ID('dbo'))
    BEGIN
        CREATE TABLE dbo.productTag (
            productTagId INT PRIMARY KEY IDENTITY(1,1),
            productId INT NOT NULL,
            tagId INT NOT NULL,

            CONSTRAINT UQ_ProductTag UNIQUE (productId, tagId),
            CONSTRAINT FK_ProductTag_Product
                FOREIGN KEY (productId) REFERENCES dbo.product(productId) ON DELETE CASCADE,
            CONSTRAINT FK_ProductTag_Tag
                FOREIGN KEY (tagId) REFERENCES dbo.tag(tagId) ON DELETE CASCADE
        );
    END
GO

CREATE NONCLUSTERED INDEX IX_ProductTag_ProductId ON dbo.productTag(productId);
CREATE NONCLUSTERED INDEX IX_ProductTag_TagId ON dbo.productTag(tagId);
CREATE NONCLUSTERED INDEX IX_Tag_Name ON dbo.tag(name);
