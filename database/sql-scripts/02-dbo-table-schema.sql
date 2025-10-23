IF DB_ID('DigitalStorefront') IS NULL
    CREATE DATABASE DigitalStorefront;
GO

USE DigitalStorefront;
GO

CREATE TABLE dbo.productType (
    productTypeId INT IDENTITY(1,1) PRIMARY KEY,
    typeName NVARCHAR(50) NOT NULL UNIQUE,
    typeCode NVARCHAR(20) NOT NULL UNIQUE,
    description NVARCHAR(200)
);
GO
  
CREATE TABLE dbo.category (
    categoryId INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    slug NVARCHAR(100) NOT NULL UNIQUE,
    displayOrder INT DEFAULT 0,
    isActive BIT DEFAULT 1,
    createdAt DATETIME DEFAULT GETUTCDATE(),
    updatedAt DATETIME NULL,
    createdBy INT NOT NULL,
    updatedBy INT NULL,
    FOREIGN KEY (createdBy) REFERENCES dsf.[user](userId),
    FOREIGN KEY (updatedBy) REFERENCES dsf.[user](userId)
);
GO

CREATE TABLE dbo.subcategory (
    subcategoryId INT IDENTITY(1,1) PRIMARY KEY,
    categoryId INT NOT NULL, 
    name NVARCHAR(100) NOT NULL,
    slug NVARCHAR(100) NOT NULL UNIQUE,
    displayOrder INT DEFAULT 0,
    imageUrl NVARCHAR(500) NULL, 
    isActive BIT DEFAULT 1,
    createdAt DATETIME DEFAULT GETUTCDATE(),
    updatedAt DATETIME NULL,
    createdBy INT NOT NULL,
    updatedBy INT NULL,
    FOREIGN KEY (categoryId) REFERENCES dbo.category(categoryId),
    FOREIGN KEY (createdBy) REFERENCES dsf.[user](userId),
    FOREIGN KEY (updatedBy) REFERENCES dsf.[user](userId)
);
GO

CREATE TABLE dbo.product (
    productId INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(200) NOT NULL,
    slug NVARCHAR(200) NOT NULL UNIQUE,
    description NVARCHAR(MAX) NULL,
    productTypeId INT NOT NULL,
    isTradeable BIT DEFAULT 0,
    isNew BIT DEFAULT 0,
    isPromotional BIT DEFAULT 0,
    isExclusive BIT DEFAULT 0,
    isActive BIT DEFAULT 1,
    parentProductId INT NULL,
    sku NVARCHAR(50) NOT NULL,
    priceTypeId INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    premiumPrice DECIMAL(10,2) NOT NULL,
    isDemoProduct BIT DEFAULT 0,
    createdAt DATETIME DEFAULT GETUTCDATE(),
    updatedAt DATETIME NULL,
    createdBy INT NOT NULL,
    updatedBy INT NULL,
    FOREIGN KEY (productTypeId) REFERENCES dbo.productType(productTypeId),
    FOREIGN KEY (createdBy) REFERENCES dsf.[user](userId),
    FOREIGN KEY (updatedBy) REFERENCES dsf.[user](userId)
);
GO

CREATE TABLE dbo.productSubcategory (
    productSubcategoryId INT PRIMARY KEY IDENTITY,
    productId INT NOT NULL,
    subcategoryId INT NOT NULL,
    displayOrder INT DEFAULT 0,
    createdAt DATETIME DEFAULT GETUTCDATE(),
    updatedAt DATETIME NULL,
    createdBy INT NOT NULL,
    updatedBy INT NULL,

    CONSTRAINT UQ_ProductSubcategory UNIQUE (productId, subcategoryId),
    FOREIGN KEY (productId) REFERENCES dbo.product(productId) ON DELETE CASCADE,
    FOREIGN KEY (subcategoryId) REFERENCES dbo.subcategory(subcategoryId) ON DELETE CASCADE,
    FOREIGN KEY (createdBy) REFERENCES dsf.[user](userId),
    FOREIGN KEY (updatedBy) REFERENCES dsf.[user](userId)
);

CREATE TABLE dbo.productImage (
    productImageId INT IDENTITY(1,1) PRIMARY KEY,
    productId INT NOT NULL,
    imageUrl VARCHAR(500) NOT NULL,
    altText VARCHAR(255),
    displayOrder INT,
    createdAt DATETIME DEFAULT GETUTCDATE(),
    updatedAt DATETIME NULL,
    createdBy INT NOT NULL,
    updatedBy INT NULL,
    FOREIGN KEY (productId) REFERENCES dbo.product(productId),
    FOREIGN KEY (createdBy) REFERENCES dsf.[user](userId),
    FOREIGN KEY (updatedBy) REFERENCES dsf.[user](userId)
);