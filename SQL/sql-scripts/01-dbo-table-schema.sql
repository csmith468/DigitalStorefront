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

CREATE TABLE dbo.priceType (
    priceTypeId INT IDENTITY(1,1) PRIMARY KEY,
    priceTypeName NVARCHAR(20) NOT NULL UNIQUE,
    priceTypeCode NVARCHAR(10) NOT NULL UNIQUE
);
GO
  
CREATE TABLE dbo.category (
    categoryId INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    slug NVARCHAR(100) NOT NULL UNIQUE,
    displayOrder INT DEFAULT 0,
    isActive BIT DEFAULT 1,
    createdAt DATETIME DEFAULT GETDATE(),
    updatedAt DATETIME DEFAULT GETDATE()
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
    createdAt DATETIME DEFAULT GETDATE(),
    updatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (categoryId) REFERENCES dbo.category(categoryId)
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
    parentProductId INT NULL,
    sku NVARCHAR(50) NOT NULL,
    priceTypeId INT NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    premiumPrice DECIMAL(10,2) NOT NULL,
    createdAt DATETIME DEFAULT GETDATE(),
    updatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (productTypeId) REFERENCES dbo.productType(productTypeId),
    FOREIGN KEY (priceTypeId) REFERENCES dbo.priceType(priceTypeId)
);
GO

CREATE TABLE dbo.productSubcategory (
    productId INT NOT NULL,
    subcategoryId INT NOT NULL,
    displayOrder INT DEFAULT 0,
    createdAt DATETIME DEFAULT GETDATE(),
    
    PRIMARY KEY (productId, subcategoryId),
    FOREIGN KEY (productId) REFERENCES dbo.product(productId),
    FOREIGN KEY (subcategoryId) REFERENCES dbo.subcategory(subcategoryId)
);
GO

CREATE TABLE dbo.productImage (
    imageId INT IDENTITY(1,1) PRIMARY KEY,
    productId INT NOT NULL,
    imageUrl NVARCHAR(500) NOT NULL,
    imageType NVARCHAR(50),
    altText NVARCHAR(200),
    FOREIGN KEY (productId) REFERENCES dbo.product(productId)
);
GO