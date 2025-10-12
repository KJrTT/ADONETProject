Ссылка на документацию в формате docx:
https://docs.google.com/document/d/1lL73F7wDFFiZiZ_qjdEVzuxSeNcfA5U4F6B6CNvdzQU/edit?usp=sharing

```
-- 1. LevelAccess
CREATE TABLE LevelAccess (
    AccessId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) unique not NULL
);

-- 2. Wallet
CREATE TABLE Wallets (
    WalletId INT IDENTITY(1,1) PRIMARY KEY,
    balance DECIMAL(10,2) not NULL default 0
);




-- 6. Client
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    first_name NVARCHAR(255) NOT NULL,
    last_name NVARCHAR(255) NOT NULL,
    password NVARCHAR(300) NOT NULL,
    user_email NVARCHAR(255) NOT NULL unique,
    level_access_id INT NOT NULL,
    wallet_id INT NOT NULL,
    FOREIGN KEY (level_access_id) REFERENCES LevelAccess(AccessId),
    FOREIGN KEY (wallet_id) REFERENCES Wallet(WalletId)
);

-- 5. Course
CREATE TABLE Course (
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    name_course NVARCHAR(255) NOT NULL,
    data_start DATETIME NOT NULL,
    data_end DATETIME NOT NULL,
    price DECIMAL(10,2) NULL,
    user_id INT NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(UserId)
);


-- 7. ClientCourses (связь многие-ко-многим)
CREATE TABLE UserCourses (
    Id INT IDENTITY(1,1) PRIMARY KEY,  -- Основной ключ
    CourseId INT NOT NULL,
    UserId INT NOT NULL,
    UNIQUE (CourseId, UserId),  -- Уникальное ограничение
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
```

```
INSERT INTO ClientCourses (CourseId, ClientId)
SELECT c.CourseId, cl.ClientId
FROM (VALUES
    (1, 1), (1, 2), (1, 3),
    (2, 4), (2, 5), (2, 6),
    (3, 7), (3, 8), (3, 9),
    (4, 10), (4, 11), (4, 12),
    (5, 13), (5, 14), (5, 15),
    (6, 1), (6, 3), (6, 5),
    (7, 2), (7, 4), (7, 6),
    (8, 7), (8, 9), (8, 11),
    (9, 8), (9, 10), (9, 12),
    (10, 13), (10, 14), (10, 15)
) AS data(CourseId, ClientId)
JOIN Course c ON data.CourseId = c.CourseId
JOIN Client cl ON data.ClientId = cl.ClientId;
```
<img width="974" height="425" alt="image" src="https://github.com/user-attachments/assets/da1f0531-374e-4f85-8ddb-9a112dd0fd4a" />

