CREATE DATABASE MovieDB;
GO

USE MovieDB;
GO

CREATE TABLE Movies (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(255) NOT NULL,    
    [Description] NVARCHAR(MAX),       
    [Letter] CHAR(1)                   
);
GO

INSERT INTO Movies (Title, Description, Letter)
VALUES ('A Movie', 'This is a description of a movie that starts with A.', 'A'),
       ('B Movie', 'This is a description of a movie that starts with B.', 'B');
