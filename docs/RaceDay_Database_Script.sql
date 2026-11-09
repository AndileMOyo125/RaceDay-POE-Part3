/* ==========================================================
   RaceDay Database Script
   Module: Programming 2B (PROG6212)
   Part 1 - Section C
   Creates the RaceDay schema and loads sample data.
   ========================================================== */

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

CREATE TABLE Roles (
    RoleID      INT             IDENTITY(1,1)   PRIMARY KEY,
    RoleName    VARCHAR(20)     NOT NULL UNIQUE
);
GO

CREATE TABLE Users (
    UserID          INT             IDENTITY(1,1)   PRIMARY KEY,
    RoleID          INT             NOT NULL,
    FullName        VARCHAR(100)    NOT NULL,
    Email           VARCHAR(100)    NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255)    NOT NULL,
    CreatedDate     DATETIME        NOT NULL DEFAULT GETDATE(),
    ProfileImageUrl VARCHAR(1000)   NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

CREATE TABLE Events (
    EventID         INT             IDENTITY(1,1)   PRIMARY KEY,
    OrganiserID     INT             NOT NULL,
    EventName       VARCHAR(100)    NOT NULL,
    Description     VARCHAR(500)    NULL,
    EventDate       DATETIME        NOT NULL,
    Location        VARCHAR(150)    NOT NULL,
    DistanceKm      DECIMAL(5,2)    NOT NULL,
    EventType       VARCHAR(20)     NOT NULL DEFAULT 'Run',
    CreatedDate     DATETIME        NOT NULL DEFAULT GETDATE(),
    BannerImageUrl  VARCHAR(1000)   NULL,
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES Users(UserID),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Run','Walk','Cycle'))
);
GO

CREATE TABLE Categories (
    CategoryID      INT             IDENTITY(1,1)   PRIMARY KEY,
    EventID         INT             NOT NULL,
    CategoryName    VARCHAR(50)     NOT NULL,
    MinAge          INT             NULL,
    MaxAge          INT             NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO

CREATE TABLE EventEnrolments (
    EnrolmentID     INT             IDENTITY(1,1)   PRIMARY KEY,
    ParticipantID   INT             NOT NULL,
    EventID         INT             NOT NULL,
    CategoryID      INT             NOT NULL,
    EnrolmentDate   DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT UQ_Enrolment UNIQUE (ParticipantID, EventID)
);
GO

CREATE TABLE Results (
    ResultID        INT             IDENTITY(1,1)   PRIMARY KEY,
    EnrolmentID     INT             NOT NULL UNIQUE,
    FinishTime      TIME            NULL,
    FinishPosition  INT             NULL,
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES EventEnrolments(EnrolmentID)
);
GO

INSERT INTO Roles (RoleName) VALUES ('Organiser'), ('Participant');
GO

INSERT INTO Users (RoleID, FullName, Email, PasswordHash)
VALUES
(1, 'Thabo Nkosi', 'thabo.nkosi@raceday.co.za', 'hashed_password_1'),
(1, 'Lindiwe Dube', 'lindiwe.dube@raceday.co.za', 'hashed_password_2'),
(2, 'Johan van der Merwe', 'johan.vdm@gmail.com', 'hashed_password_3'),
(2, 'Naledi Molefe', 'naledi.molefe@gmail.com', 'hashed_password_4');
GO

INSERT INTO Events (OrganiserID, EventName, Description, EventDate, Location, DistanceKm, EventType)
VALUES
(1, 'Joburg City Fun Run', 'A family-friendly run through the Joburg CBD.', '2026-10-10 07:00:00', 'Johannesburg, Gauteng', 10.00, 'Run'),
(1, 'Soweto Charity Walk', 'A community walk raising funds for local schools.', '2026-11-01 08:00:00', 'Soweto, Gauteng', 5.00, 'Walk'),
(2, 'Cape Winelands Cycle Tour', 'A scenic cycling event through the Cape Winelands.', '2026-09-20 06:30:00', 'Stellenbosch, Western Cape', 60.00, 'Cycle');
GO

INSERT INTO Categories (EventID, CategoryName, MinAge, MaxAge)
VALUES
(1, 'Under 20', 0, 19),
(1, 'Senior', 20, 99),
(2, 'Under 20', 0, 19),
(2, 'Senior', 20, 99),
(3, '21km', NULL, NULL),
(3, '60km', NULL, NULL);
GO

INSERT INTO EventEnrolments (ParticipantID, EventID, CategoryID)
VALUES
(3, 1, 2),
(4, 1, 2),
(3, 3, 6),
(4, 2, 4);
GO

INSERT INTO Results (EnrolmentID, FinishTime, FinishPosition)
VALUES
(1, '00:48:32', 1),
(2, '00:52:10', 2);
GO
