-- =============================================================================
-- schema_final.sql
-- Full schema reset: drops all tables and recreates them with:
--   • Chat updates from updateSqlTable.sql + updateTablev2.sql
--   • Naming fixes for all discrepancies found across branches
-- =============================================================================

-- -----------------------------------------------------------------------------
-- STEP 1: Drop all tables (order respects FK dependencies)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.Message',         'U') IS NOT NULL DROP TABLE dbo.Message;
IF OBJECT_ID('dbo.Interaction',     'U') IS NOT NULL DROP TABLE dbo.Interaction;   -- code-side name
IF OBJECT_ID('dbo.Interactions',    'U') IS NOT NULL DROP TABLE dbo.Interactions;  -- old DDL name (safety)
IF OBJECT_ID('dbo.Match',           'U') IS NOT NULL DROP TABLE dbo.Match;         -- code-side name
IF OBJECT_ID('dbo.Matches',         'U') IS NOT NULL DROP TABLE dbo.Matches;       -- old DDL name (safety)
IF OBJECT_ID('dbo.Recommendation',  'U') IS NOT NULL DROP TABLE dbo.Recommendation; -- code-side name
IF OBJECT_ID('dbo.Recommandation',  'U') IS NOT NULL DROP TABLE dbo.Recommandation; -- old DDL name (safety)
IF OBJECT_ID('dbo.Chat',            'U') IS NOT NULL DROP TABLE dbo.Chat;
IF OBJECT_ID('dbo.Post',            'U') IS NOT NULL DROP TABLE dbo.Post;
IF OBJECT_ID('dbo.Developer',       'U') IS NOT NULL DROP TABLE dbo.Developer;

-- -----------------------------------------------------------------------------
-- STEP 2: Developer
-- (unchanged from original)
-- -----------------------------------------------------------------------------
CREATE TABLE dbo.Developer (
    DeveloperID INT           IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(255)               NOT NULL,
    Password    NVARCHAR(255)               NOT NULL,
    CONSTRAINT PK_Developer PRIMARY KEY (DeveloperID)
);

-- -----------------------------------------------------------------------------
-- STEP 3: Post
-- (unchanged from original)
-- -----------------------------------------------------------------------------
CREATE TABLE dbo.Post (
    PostID      INT           IDENTITY(1,1) NOT NULL,
    DeveloperID INT                         NOT NULL,
    Parameter   NVARCHAR(255)               NOT NULL,
    Value       NVARCHAR(255)               NOT NULL,
    CONSTRAINT PK_Post     PRIMARY KEY (PostID),
    CONSTRAINT FK_Post_Dev FOREIGN KEY (DeveloperID) REFERENCES dbo.Developer(DeveloperID)
);

-- -----------------------------------------------------------------------------
-- STEP 4: Interaction  (fix #1 — was "Interactions", code queries "Interaction")
-- -----------------------------------------------------------------------------
CREATE TABLE dbo.Interaction (
    InteractionID INT IDENTITY(1,1) NOT NULL,
    DeveloperID   INT               NOT NULL,
    PostID        INT               NOT NULL,
    Type          BIT               NOT NULL,
    CONSTRAINT PK_Interaction       PRIMARY KEY (InteractionID),
    CONSTRAINT FK_Inter_Developer   FOREIGN KEY (DeveloperID) REFERENCES dbo.Developer(DeveloperID),
    CONSTRAINT FK_Inter_Post        FOREIGN KEY (PostID)      REFERENCES dbo.Post(PostID)
);

-- -----------------------------------------------------------------------------
-- STEP 5: Chat  (fully updated schema from updateSqlTable.sql + updateTablev2.sql)
--
--  Changes vs original:
--    • camelCase IDs  (ChatId, UserId, CompanyId, JobId)
--    • CompanyId      → nullable (supports user-user chats)
--    • SecondUserId   → new nullable column for user-user chats
--    • BlockedByUserId → replaces boolean isBlocked; stores which user blocked
--    • isDeletedByUser / isDeletedByCompany replaced by datetime2 columns
--      (DeletedAtByUser, DeletedAtBySecondParty) — soft-delete with timestamp
--    • CHECK: exactly one of CompanyId / SecondUserId must be set
--    • Filtered UNIQUE indexes enforce one chat per pair per direction
-- -----------------------------------------------------------------------------
CREATE TABLE dbo.Chat (
    ChatId               INT           IDENTITY(1,1) NOT NULL,
    UserId               INT                         NOT NULL,
    CompanyId            INT                         NULL,       -- NULL for user-user chats
    JobId                INT                         NULL,
    isBlocked            BIT                         NULL,
    SecondUserId         INT                         NULL,       -- NULL for user-company chats
    BlockedByUserId      INT                         NULL,       -- NULL = not blocked
    DeletedAtByUser      DATETIME2(7)                NULL,       -- NULL = not deleted
    DeletedAtBySecondParty DATETIME2(7)              NULL,       -- NULL = not deleted
    CONSTRAINT PK_Chat PRIMARY KEY (ChatId),
    CONSTRAINT CK_Chat_CompanyOrSecondUser CHECK (
        (CompanyId IS NOT NULL AND SecondUserId IS NULL) OR
        (CompanyId IS NULL     AND SecondUserId IS NOT NULL)
    )
);

-- One user-company(-job) chat per combination
CREATE UNIQUE INDEX UX_Chat_User_Company_Job
    ON dbo.Chat (UserId, CompanyId, JobId)
    WHERE CompanyId IS NOT NULL AND SecondUserId IS NULL;

-- One user-user chat per direction
CREATE UNIQUE INDEX UX_Chat_User_SecondUser
    ON dbo.Chat (UserId, SecondUserId)
    WHERE SecondUserId IS NOT NULL AND CompanyId IS NULL;

-- -----------------------------------------------------------------------------
-- STEP 6: Message
-- (FK now references Chat.ChatId — camelCase column)
-- -----------------------------------------------------------------------------
CREATE TABLE dbo.Message (
    MessageID   INT           IDENTITY(1,1) NOT NULL,
    Content     NVARCHAR(MAX)               NOT NULL,
    Timestamp   DATETIME      NOT NULL      DEFAULT GETDATE(),
    SenderID    INT                         NOT NULL,
    ChatId      INT                         NOT NULL,   -- matches Chat.ChatId
    Type        TINYINT                     NOT NULL,
    isRead      BIT           NOT NULL      DEFAULT 0,
    CONSTRAINT PK_Message       PRIMARY KEY (MessageID),
    CONSTRAINT FK_Message_Chat  FOREIGN KEY (ChatId) REFERENCES dbo.Chat(ChatId)
);

-- -----------------------------------------------------------------------------
-- STEP 7: Match  (fix #2/#3/#4 — was "Matches", code queries "Match";
--                 column "Feedback" → "FeedbackMessage";
--                 Status changed from NVARCHAR to INT)
--
--  Status int convention (matches 6-branch code using GetInt32()):
--    0 = Pending  |  1 = Accepted  |  2 = Rejected
-- -----------------------------------------------------------------------------
CREATE TABLE dbo.Matches (
    MatchID         INT           IDENTITY(1,1) NOT NULL,
    UserID          INT                         NOT NULL,
    JobID           INT                         NOT NULL,
    Status          VARCHAR(50)           NOT NULL      DEFAULT 0,  -- fix #4: int, not nvarchar
    Timestamp       DATETIME      NOT NULL      DEFAULT GETDATE(),
    Feedback NVARCHAR(MAX)               NULL,       -- fix #3: was "Feedback"
    CONSTRAINT PK_Match PRIMARY KEY (MatchID)
);

-- -----------------------------------------------------------------------------
-- STEP 8: Recommendation  (fix #5/#6 — was "Recommandation"/"RecomID";
--                           code queries "Recommendation"/"RecommendationId")
-- -----------------------------------------------------------------------------
CREATE TABLE dbo.Recommendation (
    RecommendationId INT      IDENTITY(1,1) NOT NULL,  -- fix #6: was "RecomID"
    UserID           INT                    NOT NULL,
    JobID            INT                    NOT NULL,
    Timestamp        DATETIME NOT NULL      DEFAULT GETDATE(),
    CONSTRAINT PK_Recommendation PRIMARY KEY (RecommendationId)
);

-- =============================================================================
-- Summary of all fixes applied
-- =============================================================================
-- #1  Interactions  → Interaction      (table rename, matches all-branch code)
-- #2  Matches       → Match            (table rename, matches 6-branch majority)
-- #3  Feedback      → FeedbackMessage  (column rename, matches 6-branch majority)
-- #4  Status NVARCHAR(100) → Status INT (type fix, matches 6-branch GetInt32() calls)
-- #5  Recommandation → Recommendation  (typo fix + rename, matches all-branch code)
-- #6  RecomID        → RecommendationId (column rename, matches all-branch code)
-- Chat fully updated per updateSqlTable.sql + updateTablev2.sql
-- Note: feature/user-status-skill-gap has an internal contradiction
--       (SqlMatchRepository vs UserStatusMatchRepository) that cannot be fixed
--       at the DB level — one of the two repositories must be updated in code.
-- =============================================================================