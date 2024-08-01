use BookTest;

-- ROLE --
CREATE TABLE RoleCollections (
    Id INT PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL,
);

-- BOOK --
CREATE TABLE Book (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Title NVARCHAR(MAX) NOT NULL,
    OwnerId NVARCHAR(MAX) NOT NULL,
    CurrentRoleCollectionId INT NOT NULL,
    FOREIGN KEY (CurrentRoleCollectionId) REFERENCES RoleCollections(Id) 
);

-- Chapters --
CREATE TABLE Chapters (
    Id INT PRIMARY KEY,
    Title NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL
);

-- ROLE --
CREATE TABLE Roles (
    Id INT PRIMARY KEY,
    RoleCollectionId INT NOT NULL,
    Name NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (RoleCollectionId) References RoleCollections(Id)

);

-- ROLE PERMISSIONS --
CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id),
);

-- SCHOLARS --
CREATE TABLE Scholars (
    Id INT PRIMARY KEY,
    ScribeId NVARCHAR(MAX) NOT NULL,
    RoleId INT NOT NULL,
    FOREIGN KEY (RoleID) REFERENCES Roles(Id)
);

------------------
-- DEFAULT ROLE --
------------------
INSERT INTO RoleCollections VALUES
	(1, 'Default');

INSERT INTO Roles (Id, RoleCollectionId, Name) VALUES
	(1, 1, 'Dev');

-- OWNER PERMISSIONS --
INSERT INTO RolePermissions (RoleId, PermissionId) VALUES
    (1, 0);  -- Dev

INSERT INTO Chapters (Id, Title, Description) VALUES
	(1, 'Chapter 1', 'This is a test chapter'),
	(2, 'Chapter 2', 'This is a test chapter'),
	(3, 'Chapter 3', 'This is a test chapter');
