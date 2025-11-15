-- Create AutoGrader Database
USE master;
GO

-- Drop database if exists (uncomment if you want to recreate)
-- IF EXISTS (SELECT name FROM sys.databases WHERE name = 'AutoGraderDB')
-- DROP DATABASE AutoGraderDB;
-- GO

-- Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AutoGraderDB')
CREATE DATABASE AutoGraderDB;
GO

USE AutoGraderDB;
GO

--Roles Table

CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL
);

-- Insert Roles
INSERT INTO Roles (RoleName)
VALUES 
('Admin'),
('Manager'),
('Moderator'),
('Examiner');

--Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId INT NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId)
);



--Classes Table
CREATE TABLE Classes (
    ClassId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ClassName NVARCHAR(100) NOT NULL,
    Semester NVARCHAR(50) NOT NULL,
    Lecturer INT NULL,
    Examiner INT NULL,
    CONSTRAINT FK_Classes_Lecturer FOREIGN KEY (Lecturer)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Classes_Examiner FOREIGN KEY (Examiner)
        REFERENCES Users(UserId)
);
GO

-- Create Students table
CREATE TABLE Students (
    StudentId int IDENTITY(1,1) NOT NULL,
    StudentCode nvarchar(20) NOT NULL,
    FullName nvarchar(255) NULL,
    Email nvarchar(255) NULL,
    CONSTRAINT PK_Students PRIMARY KEY (StudentId)
);

CREATE TABLE ClassStudents (
    ClassId INT NOT NULL,
    StudentId INT NOT NULL,
    CONSTRAINT PK_ClassStudents PRIMARY KEY (ClassId, StudentId),
    CONSTRAINT FK_ClassStudents_Classes FOREIGN KEY (ClassId)
        REFERENCES Classes(ClassId),
    CONSTRAINT FK_ClassStudents_Students FOREIGN KEY (StudentId)
        REFERENCES Students(StudentId)
);

-- Create unique index on StudentCode
CREATE UNIQUE NONCLUSTERED INDEX IX_Students_StudentCode 
ON Students (StudentCode);

-- Create Rules table
CREATE TABLE Rules (
    RuleId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name nvarchar(255) NOT NULL,
    Pattern nvarchar(255) NOT NULL,
    Severity nvarchar(50) NULL,
    Description nvarchar(500) NULL,
);

-- Create Submissions table
CREATE TABLE Submissions (
    SubmissionId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ZipFileName nvarchar(255) NOT NULL,
    UploadedAt datetime NULL DEFAULT (getdate()),
    CheckedAt datetime NULL,
    StudentId int NULL,
    CONSTRAINT FK_Submissions_Students FOREIGN KEY (StudentId) REFERENCES Students(StudentId)
);

-- Create Violations table
CREATE TABLE Violations (
    ViolationId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SubmissionId INT FOREIGN KEY REFERENCES Submissions(SubmissionId),  
    RuleId INT FOREIGN KEY REFERENCES Rules(RuleId),  
    FilePath nvarchar(500) NULL,
    Message nvarchar(1000) NULL,
);

