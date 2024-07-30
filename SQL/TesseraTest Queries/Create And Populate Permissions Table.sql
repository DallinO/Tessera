use TesseraTest;

CREATE TABLE Permissions (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO Permissions (Id, Name) VALUES
(0, 'Dev')
