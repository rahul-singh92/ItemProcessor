-- Item Processor Database Script
-- Created for Assessment Submission

-- Step 1: Create Database
CREATE DATABASE ItemProcessorDB;
GO

USE ItemProcessorDB;
GO

-- Step 2: Create Users Table
CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     VARCHAR(100)  NOT NULL,
    Email        VARCHAR(150)  NOT NULL UNIQUE,
    PasswordHash VARCHAR(255)  NOT NULL,
    CreatedAt    DATETIME      DEFAULT GETDATE(),
    IsActive     BIT           DEFAULT 1
);
GO

-- Step 3: Create Items Table
CREATE TABLE Items (
    ItemId      INT IDENTITY(1,1) PRIMARY KEY,
    ItemName    VARCHAR(200)  NOT NULL,
    Weight      DECIMAL(10,3) NOT NULL,
    Description VARCHAR(500)  NULL,
    CreatedBy   INT           NOT NULL,
    CreatedAt   DATETIME      DEFAULT GETDATE(),
    IsActive    BIT           DEFAULT 1,

    CONSTRAINT FK_Items_Users
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
GO

-- Step 4: Create ProcessedItems Table
CREATE TABLE ProcessedItems (
    ProcessedItemId  INT IDENTITY(1,1) PRIMARY KEY,
    ParentItemId     INT            NOT NULL,
    ChildItemId      INT            NOT NULL,
    OutputWeight     DECIMAL(10,3)  NOT NULL,
    ProcessedBy      INT            NOT NULL,
    ProcessedAt      DATETIME       DEFAULT GETDATE(),
    Notes            VARCHAR(500)   NULL,
    IsProcessed      BIT            DEFAULT 0,

    CONSTRAINT FK_ProcessedItems_Parent
        FOREIGN KEY (ParentItemId) REFERENCES Items(ItemId),

    CONSTRAINT FK_ProcessedItems_Child
        FOREIGN KEY (ChildItemId) REFERENCES Items(ItemId),

    CONSTRAINT FK_ProcessedItems_User
        FOREIGN KEY (ProcessedBy) REFERENCES Users(UserId)
);
GO

-- Step 5: Create Indexes
CREATE INDEX IX_ProcessedItems_ParentItemId
    ON ProcessedItems(ParentItemId);

CREATE INDEX IX_ProcessedItems_ChildItemId
    ON ProcessedItems(ChildItemId);

CREATE INDEX IX_Items_CreatedBy
    ON Items(CreatedBy);
GO

-- Step 6: Insert Default Admin User
-- Password is 'Admin@123' hashed with SHA256
INSERT INTO Users (Username, Email, PasswordHash)
VALUES (
    'Admin',
    'admin@test.com',
    'tBQdSVSNjnKUHM7UE3pKhl1zhSfUfgDxGV3KCdSUgds='
);
GO

-- Step 7: Insert Sample Items
INSERT INTO Items (ItemName, Weight, Description, CreatedBy)
VALUES
    ('Raw Material A', 100.000, 'Base raw material', 1),
    ('Raw Material B',  50.000, 'Second raw material', 1),
    ('Raw Material C',  75.000, 'Third raw material', 1);
GO

-- Step 8: Insert Sample Processing Records
INSERT INTO Items (ItemName, Weight, Description, CreatedBy)
VALUES
    ('Output Item X', 45.000, 'First output of A', 1),
    ('Output Item Y', 30.000, 'Second output of A', 1),
    ('Sub Output D',  20.000, 'Output of X', 1);
GO

INSERT INTO ProcessedItems
    (ParentItemId, ChildItemId, OutputWeight, ProcessedBy, Notes, IsProcessed)
VALUES
    (1, 4, 45.000, 1, 'First processing run', 1),
    (1, 5, 30.000, 1, 'First processing run', 1),
    (4, 6, 20.000, 1, 'Second level processing', 1);
GO

-- Step 9: Verify setup
SELECT 'Users'          AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Items'          AS TableName, COUNT(*) AS RecordCount FROM Items
UNION ALL
SELECT 'ProcessedItems' AS TableName, COUNT(*) AS RecordCount FROM ProcessedItems;
GO

PRINT 'Database setup complete!';
GO