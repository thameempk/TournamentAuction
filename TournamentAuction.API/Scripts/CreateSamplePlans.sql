-- Script to create sample subscription plans
-- Run this after creating the database

USE Auction_Tournament
GO

-- Create Basic Plan (Free)
INSERT INTO SubscriptionPlans (PlanId, PlanName, Price, DurationInDays, MaxTournaments, MaxTeams, MaxAuctions, IsActive, CreatedAt)
VALUES (
    NEWID(),
    'Basic Plan',
    0.00,
    30,
    1,
    4,
    5,
    1,
    GETDATE()
);
GO

-- Create Standard Plan
INSERT INTO SubscriptionPlans (PlanId, PlanName, Price, DurationInDays, MaxTournaments, MaxTeams, MaxAuctions, IsActive, CreatedAt)
VALUES (
    NEWID(),
    'Standard Plan',
    59.99,
    30,
    10,
    30,
    15,
    1,
    GETDATE()
);
GO

-- Create Premium Plan
INSERT INTO SubscriptionPlans (PlanId, PlanName, Price, DurationInDays, MaxTournaments, MaxTeams, MaxAuctions, IsActive, CreatedAt)
VALUES (
    NEWID(),
    'Premium Plan',
    99.99,
    30,
    NULL, -- Unlimited
    NULL, -- Unlimited
    NULL, -- Unlimited
    1,
    GETDATE()
);
GO

PRINT 'Sample subscription plans created successfully!'
GO

