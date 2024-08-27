CREATE TABLE Books (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(MAX) NOT NULL,
    ScribeId NVARCHAR(450) NULL -- Not a foriegn key, but references the scribe id in tessera-master
);

CREATE TABLE Chapters (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Title NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    BookId UNIQUEIDENTIFIER NOT NULL,
);

CREATE TABLE Roles (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL
);

CREATE TABLE Scholar (
    ScribeId NVARCHAR(450) NOT NULL, -- Not a foriegn key, but references the scribe id in tessera-master
    RoleId INT NOT NULL
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

CREATE TABLE Permissions (
	Id INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
	Permission NVARCHAR(MAX) NOT NULL
);

CREATE TABLE RoleCollection (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL
);

CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE
);

CREATE TABLE RoleCollectionRoles (
    RoleCollectionId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (RoleCollectionId, RoleId),
    FOREIGN KEY (RoleCollectionId) REFERENCES RoleCollection(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

CREATE TABLE StatusCollection (
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(50) NOT NULL
);

CREATE TABLE Statuses (
	Id INT PRIMARY KEY IDENTITY,	
	Name NVARCHAR(50) NOT NULL
);

CREATE TABLE StatusCollectionStatuses (
    StatusCollectionId INT,
    StatusId INT,
    PRIMARY KEY (StatusCollectionId, StatusId),
    FOREIGN KEY (StatusCollectionId) REFERENCES StatusCollection(Id),
    FOREIGN KEY (StatusId) REFERENCES Statuses(Id)
);

CREATE TABLE Lists (
    Id INT PRIMARY KEY IDENTITY,
	ChapterId int NOT NULL,
    Name NVARCHAR(255) NOT NULL
	FOREIGN KEY (ChapterId) REFERENCES Chapters(Id) ON DELETE CASCADE
);

CREATE TABLE Columns (
    Id INT PRIMARY KEY IDENTITY,
    ListId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    DataType NVARCHAR(50) NOT NULL,
	FOREIGN KEY (ListId) REFERENCES Lists(Id) ON DELETE CASCADE
    
);

CREATE TABLE Data (
    Id INT PRIMARY KEY IDENTITY,
    ListId INT FOREIGN KEY REFERENCES Lists(Id),
    RowId INT NOT NULL,
    ColumnId INT FOREIGN KEY REFERENCES Columns(Id),
    Value NVARCHAR(MAX) -- You may need to handle data type conversions
);

