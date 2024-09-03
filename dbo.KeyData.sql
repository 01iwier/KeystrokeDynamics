﻿CREATE TABLE dbo.KeyData
(
    SessionId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES dbo.Users(UserId) NOT NULL,
    Timestamp DATETIME NOT NULL
);