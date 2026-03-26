-- =============================================================================
-- Author: Vladyslav Zaiets | https://sarmkadan.com
-- CTO & Software Architect
-- =============================================================================

-- Database initialization script for DotnetMicroOrm examples

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DotnetMicroOrmExamples')
BEGIN
    CREATE DATABASE DotnetMicroOrmExamples;
END
GO

USE DotnetMicroOrmExamples;
GO

-- ============================================================================
-- Products Table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        Price DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME,
        Version INT NOT NULL DEFAULT 1
    );

    -- Create indexes for common queries
    CREATE INDEX IX_Products_Price ON Products(Price);
    CREATE INDEX IX_Products_Name ON Products(Name);
    CREATE INDEX IX_Products_Stock ON Products(StockQuantity);
END
GO

-- ============================================================================
-- Categories Table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME,
        Version INT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_Categories_Name ON Categories(Name);
END
GO

-- ============================================================================
-- Orders Table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME,
        Version INT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_Orders_Status ON Orders(Status);
    CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);
END
GO

-- ============================================================================
-- OrderItems Table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
BEGIN
    CREATE TABLE OrderItems (
        Id INT PRIMARY KEY IDENTITY(1,1),
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        Version INT NOT NULL DEFAULT 1,

        FOREIGN KEY (OrderId) REFERENCES Orders(Id),
        FOREIGN KEY (ProductId) REFERENCES Products(Id)
    );

    CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);
END
GO

-- ============================================================================
-- Users Table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Email NVARCHAR(255) NOT NULL UNIQUE,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX),
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME,
        Version INT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_Username ON Users(Username);
    CREATE INDEX IX_Users_IsActive ON Users(IsActive);
END
GO

-- ============================================================================
-- AuditLogs Table
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        EntityType NVARCHAR(100) NOT NULL,
        EntityId INT NOT NULL,
        Action NVARCHAR(50) NOT NULL, -- 'Insert', 'Update', 'Delete'
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX),
        ChangedBy NVARCHAR(255),
        ChangedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        IpAddress NVARCHAR(50)
    );

    CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
    CREATE INDEX IX_AuditLogs_EntityId ON AuditLogs(EntityId);
    CREATE INDEX IX_AuditLogs_ChangedAt ON AuditLogs(ChangedAt);
END
GO

-- ============================================================================
-- Sample Data
-- ============================================================================

IF (SELECT COUNT(*) FROM Products) = 0
BEGIN
    INSERT INTO Products (Name, Description, Price, StockQuantity, CreatedAt)
    VALUES
        ('MacBook Pro 16"', 'Professional laptop with M3 Max', 2499.99, 10, GETUTCDATE()),
        ('iPhone 15 Pro', 'Latest flagship smartphone', 1199.99, 25, GETUTCDATE()),
        ('iPad Air', 'Versatile tablet computer', 599.99, 15, GETUTCDATE()),
        ('AirPods Pro', 'Premium wireless earbuds', 249.99, 50, GETUTCDATE()),
        ('Apple Watch Series 9', 'Smartwatch with health features', 399.99, 20, GETUTCDATE());
END
GO

IF (SELECT COUNT(*) FROM Categories) = 0
BEGIN
    INSERT INTO Categories (Name, Description, CreatedAt)
    VALUES
        ('Electronics', 'Electronic devices and gadgets', GETUTCDATE()),
        ('Computers', 'Laptops and desktop computers', GETUTCDATE()),
        ('Mobile', 'Smartphones and tablets', GETUTCDATE()),
        ('Accessories', 'Cables, chargers, and accessories', GETUTCDATE());
END
GO

-- ============================================================================
-- Summary
-- ============================================================================

PRINT '=== Database Initialization Complete ===';
PRINT '';
PRINT 'Created Tables:';
PRINT '  - Products';
PRINT '  - Categories';
PRINT '  - Orders';
PRINT '  - OrderItems';
PRINT '  - Users';
PRINT '  - AuditLogs';
PRINT '';
PRINT 'Sample data has been inserted into Products and Categories.';
PRINT 'Database is ready for DotnetMicroOrm examples!';
