CREATE TABLE dbo.MouseData
(
    SessionId INT PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES dbo.Users(UserId),
    Timestamp DATETIME NOT NULL,
);