-- Performance indexes (foreign keys)
CREATE NONCLUSTERED INDEX IX_Product_ProductTypeId ON dbo.product(productTypeId);
CREATE NONCLUSTERED INDEX IX_Product_PriceTypeId ON dbo.product(priceTypeId);
CREATE NONCLUSTERED INDEX IX_ProductSubcategory_ProductId ON dbo.productSubcategory(productId);
CREATE NONCLUSTERED INDEX IX_ProductSubcategory_SubcategoryId ON dbo.productSubcategory(subcategoryId);
CREATE NONCLUSTERED INDEX IX_ProductImage_ProductId ON dbo.productImage(productId);
CREATE NONCLUSTERED INDEX IX_Subcategory_CategoryId ON dbo.subcategory(categoryId);
CREATE NONCLUSTERED INDEX IX_User_Username ON dsf.[user](username);
CREATE NONCLUSTERED INDEX IX_User_Email ON dsf.[user](email);
CREATE NONCLUSTERED INDEX IX_UserRole_UserId ON dsf.[userRole](userId);
CREATE NONCLUSTERED INDEX IX_UserRole_RoleId ON dsf.[userRole](roleId);
CREATE NONCLUSTERED INDEX IX_Auth_UserId ON dsf.auth(userId);

-- Search performance
CREATE UNIQUE NONCLUSTERED INDEX IX_Product_Name ON dbo.product(name) INCLUDE (slug, productTypeId, price);
CREATE UNIQUE NONCLUSTERED INDEX IX_Product_Slug_Unique ON dbo.product(slug);
CREATE UNIQUE NONCLUSTERED INDEX IX_Category_Slug ON dbo.category(slug);
CREATE UNIQUE NONCLUSTERED INDEX IX_Subcategory_Slug ON dbo.subcategory(slug);
CREATE NONCLUSTERED INDEX IX_Product_CreatedBy ON dbo.product(createdBy);

-- Composite indexes for common queries
CREATE NONCLUSTERED INDEX IX_Product_IsDemoProduct_ProductId
    ON dbo.product(isDemoProduct, productId)
    INCLUDE (name, slug, price, premiumPrice);
CREATE NONCLUSTERED INDEX IX_Category_IsActive_DisplayOrder
    ON dbo.category(isActive, displayOrder);
CREATE NONCLUSTERED INDEX IX_Subcategory_CategoryId_IsActive
    ON dbo.subcategory(categoryId, isActive)
    INCLUDE (displayOrder);
CREATE NONCLUSTERED INDEX IX_ProductImage_ProductId_DisplayOrder
    ON dbo.productImage(productId, displayOrder);