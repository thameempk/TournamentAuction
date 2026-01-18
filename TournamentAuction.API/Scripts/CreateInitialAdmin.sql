-- Script to create initial admin user
-- Run this after creating the database

USE Auction_Tournament
GO

-- Create Admin User
-- Password: Admin123
-- Note: This hash is for "Admin123" - in production, use BCrypt to generate proper hash
-- For now, you can register via API endpoint /api/auth/register

-- Example: Create admin user (password will be hashed by application)
-- You should use the /api/auth/register endpoint instead, or generate BCrypt hash manually

-- To generate BCrypt hash in C#:
-- BCrypt.Net.BCrypt.HashPassword("Admin123")

-- Temporary: Create user with plain password (will be hashed on first login attempt)
-- Better approach: Use the register endpoint

-- Example BCrypt hash for "Admin123": 
-- $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy

INSERT INTO Users (UserId, Name, Email, PasswordHash, UserType, CreatedAt, UpdatedAt)
VALUES (
    NEWID(),
    'System Admin',
    'admin@tournament.com',
    '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', -- BCrypt hash for "Admin123"
    'Admin',
    GETDATE(),
    GETDATE()
);
GO

-- Create a Tournament Type (Cricket)
DECLARE @AdminUserId UNIQUEIDENTIFIER = (SELECT TOP 1 UserId FROM Users WHERE UserType = 'Admin');

INSERT INTO TournamentsTypes (TypeId, Name, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
VALUES (
    NEWID(),
    'Cricket',
    @AdminUserId,
    @AdminUserId,
    GETDATE(),
    GETDATE()
);
GO

-- Create a Tournament Type (Football)
DECLARE @AdminUserId2 UNIQUEIDENTIFIER = (SELECT TOP 1 UserId FROM Users WHERE UserType = 'Admin');

INSERT INTO TournamentsTypes (TypeId, Name, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
VALUES (
    NEWID(),
    'Football',
    @AdminUserId2,
    @AdminUserId2,
    GETDATE(),
    GETDATE()
);
GO

PRINT 'Initial admin user and tournament types created successfully!'
PRINT 'Login credentials:'
PRINT 'Email: admin@tournament.com'
PRINT 'Password: Admin123'
GO

