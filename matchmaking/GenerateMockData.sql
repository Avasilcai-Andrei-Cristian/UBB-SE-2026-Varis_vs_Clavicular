

SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;


DELETE FROM dbo.Interaction;
DELETE FROM dbo.Post;
DELETE FROM dbo.Message;
DELETE FROM dbo.Chat;
DELETE FROM dbo.Recommendation;
DELETE FROM dbo.[Matches];
DELETE FROM dbo.Developer;

-- Reset identity seeds so IDs are predictable
DBCC CHECKIDENT ('dbo.Developer', RESEED, 0);
DBCC CHECKIDENT ('dbo.Post', RESEED, 0);
DBCC CHECKIDENT ('dbo.Interaction', RESEED, 0);
DBCC CHECKIDENT ('dbo.Chat', RESEED, 0);
DBCC CHECKIDENT ('dbo.Message', RESEED, 0);
DBCC CHECKIDENT ('dbo.Matches', RESEED, 0);
DBCC CHECKIDENT ('dbo.Recommendation', RESEED, 0);


INSERT INTO dbo.Developer (Name, Password) VALUES
('Andrei Mihai', 'pass123'),
('Bianca Neagu', 'pass123'),
('Cristian Popa', 'pass123'),
('Daniela Lazar', 'pass123'),
('Emil Tanase', 'pass123'),
('Felicia Munteanu', 'pass123'),
('George Dumitrescu', 'pass123'),
('Helena Constantinescu', 'pass123'),
('Ion Alexandrescu', 'pass123'),
('Jana Rotaru', 'pass123'),
('Konstantin Ciobanu', 'pass123'),
('Larisa Ungureanu', 'pass123'),
('Mihai Florescu', 'pass123'),
('Natalia Bogdan', 'pass123'),
('Octavian Serban', 'pass123'),
('Patricia Aldea', 'pass123'),
('Radu Mocanu', 'pass123'),
('Simona Nistor', 'pass123'),
('Tiberiu Costache', 'pass123'),
('Viorica Zamfir', 'pass123');


INSERT INTO dbo.Post (DeveloperId, Parameter, Value) VALUES
-- Mitigation factor proposals
(1, 'mitigation factor', '0.3'),
(3, 'mitigation factor', '0.5'),
(7, 'mitigation factor', '0.1'),
(15, 'mitigation factor', '0.8'),
-- Weighted distance score weight proposals
(2, 'weighted distance score weight', '30'),
(5, 'weighted distance score weight', '25'),
(10, 'weighted distance score weight', '35'),
(18, 'weighted distance score weight', '20'),
-- Job-resume similarity score weight proposals
(4, 'job-resume similarity score weight', '35'),
(8, 'job-resume similarity score weight', '40'),
(12, 'job-resume similarity score weight', '25'),
(16, 'job-resume similarity score weight', '30'),
-- Preference score weight proposals
(6, 'preference score weight', '20'),
(9, 'preference score weight', '15'),
(13, 'preference score weight', '25'),
(19, 'preference score weight', '10'),
-- Promotion score weight proposals
(11, 'promotion score weight', '15'),
(14, 'promotion score weight', '10'),
(17, 'promotion score weight', '20'),
(20, 'promotion score weight', '5'),
-- Relevant keyword proposals
(1, 'relevant keyword', 'machine learning'),
(2, 'relevant keyword', 'cloud native'),
(4, 'relevant keyword', 'microservices'),
(6, 'relevant keyword', 'agile'),
(8, 'relevant keyword', 'distributed systems'),
(10, 'relevant keyword', 'data pipeline'),
(12, 'relevant keyword', 'devops'),
(14, 'relevant keyword', 'security'),
(16, 'relevant keyword', 'full stack'),
(18, 'relevant keyword', 'automation'),
(3, 'relevant keyword', 'kubernetes'),
(5, 'relevant keyword', 'react'),
(7, 'relevant keyword', 'python'),
(9, 'relevant keyword', 'sql'),
(11, 'relevant keyword', 'testing'),
(13, 'relevant keyword', 'api design'),
(15, 'relevant keyword', 'ci/cd'),
(17, 'relevant keyword', 'typescript'),
(19, 'relevant keyword', 'containerization'),
(20, 'relevant keyword', 'deep learning'),
-- Extra posts for Developer 1 (Andrei Mihai) — PostIds 41-45
(1, 'weighted distance score weight', '28'),
(1, 'preference score weight', '18'),
(1, 'promotion score weight', '12'),
(1, 'job-resume similarity score weight', '32'),
(1, 'relevant keyword', 'react');

INSERT INTO dbo.Interaction (DeveloperId, PostId, Type) VALUES
-- Votes on mitigation factor posts (PostId 1-4)
(2, 1, 1), (4, 1, 1), (5, 1, 0), (8, 1, 1), (10, 1, 0),
(1, 2, 1), (6, 2, 1), (9, 2, 0), (12, 2, 1), (14, 2, 1),
(3, 3, 0), (5, 3, 0), (11, 3, 1), (13, 3, 0), (16, 3, 0),
(2, 4, 0), (7, 4, 1), (8, 4, 0), (17, 4, 0), (19, 4, 1),
-- Votes on weighted distance posts (PostId 5-8)
(1, 5, 1), (3, 5, 1), (6, 5, 0), (9, 5, 1), (11, 5, 1),
(2, 6, 1), (4, 6, 1), (7, 6, 0), (13, 6, 1), (15, 6, 1),
(1, 7, 0), (3, 7, 0), (8, 7, 1), (14, 7, 0), (16, 7, 1),
(5, 8, 1), (9, 8, 0), (12, 8, 1), (17, 8, 1), (20, 8, 0),
-- Votes on job-resume similarity posts (PostId 9-12)
(1, 9, 1), (2, 9, 1), (5, 9, 1), (10, 9, 0), (15, 9, 1),
(3, 10, 1), (6, 10, 0), (9, 10, 1), (11, 10, 1), (18, 10, 1),
(4, 11, 0), (7, 11, 0), (13, 11, 1), (16, 11, 0), (20, 11, 0),
(1, 12, 1), (5, 12, 0), (8, 12, 1), (14, 12, 1), (19, 12, 0),
-- Votes on preference score posts (PostId 13-16)
(2, 13, 1), (7, 13, 1), (10, 13, 0), (14, 13, 1), (17, 13, 1),
(1, 14, 0), (4, 14, 1), (8, 14, 0), (11, 14, 0), (15, 14, 1),
(3, 15, 1), (5, 15, 1), (12, 15, 0), (16, 15, 1), (18, 15, 0),
(6, 16, 0), (9, 16, 0), (10, 16, 1), (13, 16, 0), (20, 16, 0),
-- Votes on promotion score posts (PostId 17-20)
(1, 17, 1), (3, 17, 0), (7, 17, 1), (12, 17, 1), (15, 17, 0),
(2, 18, 1), (5, 18, 1), (8, 18, 0), (16, 18, 1), (19, 18, 1),
(4, 19, 0), (6, 19, 1), (9, 19, 0), (13, 19, 1), (18, 19, 0),
(1, 20, 1), (7, 20, 1), (10, 20, 0), (14, 20, 1), (20, 20, 0),
-- Votes on relevant keyword posts (PostId 21-40)
(3, 21, 1), (8, 21, 1), (10, 21, 1), (15, 21, 0),
(1, 22, 1), (5, 22, 1), (9, 22, 0), (14, 22, 1),
(2, 23, 1), (6, 23, 0), (11, 23, 1), (17, 23, 1),
(3, 24, 1), (7, 24, 1), (13, 24, 1), (18, 24, 0),
(1, 25, 1), (4, 25, 1), (9, 25, 0), (16, 25, 1),
(2, 26, 1), (5, 26, 0), (11, 26, 1), (19, 26, 1),
(3, 27, 0), (6, 27, 1), (8, 27, 1), (15, 27, 1),
(4, 28, 1), (7, 28, 0), (10, 28, 1), (17, 28, 1),
(1, 29, 1), (5, 29, 1), (9, 29, 1), (13, 29, 0),
(2, 30, 1), (6, 30, 0), (12, 30, 1), (18, 30, 1),
(4, 31, 1), (8, 31, 0), (11, 31, 1), (16, 31, 1),
(3, 32, 1), (7, 32, 1), (14, 32, 0), (19, 32, 1),
(1, 33, 0), (5, 33, 1), (10, 33, 1), (17, 33, 0),
(2, 34, 1), (6, 34, 1), (9, 34, 0), (15, 34, 1),
(4, 35, 1), (8, 35, 1), (13, 35, 0), (20, 35, 1),
(3, 36, 1), (7, 36, 0), (11, 36, 1), (16, 36, 0),
(1, 37, 1), (5, 37, 0), (10, 37, 1), (18, 37, 1),
(2, 38, 1), (6, 38, 1), (12, 38, 0), (19, 38, 1),
(4, 39, 0), (8, 39, 1), (14, 39, 1), (17, 39, 0),
(3, 40, 1), (9, 40, 1), (13, 40, 0), (20, 40, 1),
-- Votes on Developer 1's extra posts (PostId 41-45)
(2, 41, 1), (5, 41, 0), (8, 41, 1), (12, 41, 1), (17, 41, 0),
(3, 42, 1), (6, 42, 1), (10, 42, 0), (14, 42, 1), (19, 42, 1),
(4, 43, 0), (7, 43, 1), (11, 43, 1), (15, 43, 0), (20, 43, 1),
(2, 44, 1), (9, 44, 1), (13, 44, 0), (16, 44, 1), (18, 44, 0),
(5, 45, 1), (8, 45, 0), (10, 45, 1), (14, 45, 1), (17, 45, 1);


INSERT INTO dbo.[Matches] (UserID, JobID, Status, Timestamp, Feedback) VALUES
-- Alice Pop (User 1) — Frontend dev, skills: C#, React
(1, 1, 'Accepted', '2026-03-01 09:00:00', 'Strong React skills. Good fit for junior frontend.'),
(1, 9, 'Pending', '2026-03-15 10:30:00', NULL),
(1, 102, 'Advanced', '2026-03-10 14:00:00', 'Moving to technical interview for frontend role.'),
(1, 2, 'Pending', '2026-03-18 09:00:00', NULL),
(1, 7, 'Rejected', '2026-02-20 14:00:00', 'UI/UX role requires more Figma experience.'),
(1, 5, 'Pending', '2026-03-22 11:00:00', NULL),
(1, 100, 'Rejected', '2026-03-05 10:00:00', 'Backend role needs deeper API experience.'),
(1, 3, 'Accepted', '2026-03-12 15:00:00', 'QA skills promising with testing background.'),
(1, 6, 'Pending', '2026-03-25 09:00:00', NULL),
-- Bogdan Ionescu (User 2) — Backend .NET, skills: C#, SQL, React, Docker, Python
(2, 2, 'Accepted', '2026-03-02 11:00:00', 'Excellent C# and SQL. Perfect for backend .NET.'),
(2, 100, 'Advanced', '2026-03-08 09:15:00', 'Strong backend profile. Final round scheduled.'),
(2, 9, 'Pending', '2026-03-20 16:00:00', NULL),
(2, 101, 'Pending', '2026-03-22 10:00:00', NULL),
-- Carmen Radu (User 3) — Junior QA, skills: Testing, Selenium
(3, 3, 'Accepted', '2026-03-05 13:00:00', 'Good QA automation background.'),
(3, 104, 'Pending', '2026-03-12 10:00:00', NULL),
-- Dan Tudor (User 4) — DevOps, skills: Docker, Kubernetes
(4, 4, 'Advanced', '2026-02-28 08:30:00', 'Scheduled final round with VP for DevOps role.'),
(4, 103, 'Accepted', '2026-03-08 15:00:00', 'Excellent cloud and pipeline knowledge.'),
(4, 10, 'Pending', '2026-03-18 11:00:00', NULL),
-- Elena Matei (User 5) — Data analyst, skills: Python, Pandas
(5, 5, 'Accepted', '2026-03-03 14:30:00', 'Strong Python and data analysis skills.'),
(5, 101, 'Pending', '2026-03-14 10:00:00', NULL),
(5, 6, 'Rejected', '2026-03-01 09:45:00', 'ML role needs more deep learning experience.'),
-- Florin Pavel (User 6) — ML engineer, skills: Machine Learning, NLP
(6, 6, 'Accepted', '2026-03-06 13:00:00', 'Excellent ML and NLP background.'),
(6, 100, 'Rejected', '2026-02-25 16:00:00', 'Backend role needs more API experience.'),
(6, 101, 'Advanced', '2026-03-19 10:00:00', 'Python skills strong. Moving to final round.'),
-- Gabriela Stan (User 7) — UI/UX, skills: Figma, UI Design
(7, 7, 'Accepted', '2026-02-22 09:00:00', 'Beautiful portfolio. Great design sense.'),
(7, 1, 'Pending', '2026-03-16 14:00:00', NULL),
-- Horia Vasile (User 8) — Tech lead, skills: Architecture, Leadership
(8, 8, 'Advanced', '2026-03-04 11:00:00', 'Strong architecture skills. Offer stage.'),
(8, 100, 'Accepted', '2026-03-07 10:30:00', 'Can mentor junior developers.'),
(8, 103, 'Pending', '2026-03-21 15:00:00', NULL),
-- Ioana Dobre (User 9) — Full-stack, skills: React, C#
(9, 9, 'Accepted', '2026-03-09 09:00:00', 'Solid full-stack profile.'),
(9, 1, 'Accepted', '2026-03-11 13:00:00', 'Good React skills for frontend role.'),
(9, 2, 'Rejected', '2026-03-01 15:00:00', 'C# skills not deep enough for senior .NET role.'),
(9, 102, 'Pending', '2026-03-20 11:00:00', NULL),
-- Julian Muresan (User 10) — Cloud architect, skills: Cloud, Docker
(10, 10, 'Advanced', '2026-03-06 12:00:00', 'Top candidate for Cloud Architect. Offer extended.'),
(10, 4, 'Accepted', '2026-03-13 10:00:00', 'Docker expertise strong for DevOps role.'),
(10, 103, 'Pending', '2026-03-25 09:00:00', NULL),
(10, 8, 'Pending', '2026-03-22 14:00:00', NULL),
-- Katerina Lupu (User 11) — Mobile dev, skills: Flutter, Kotlin
(11, 1, 'Accepted', '2026-03-05 10:00:00', 'Mobile experience translates well to frontend.'),
(11, 7, 'Pending', '2026-03-18 13:00:00', NULL),
(11, 102, 'Rejected', '2026-03-02 09:00:00', 'Looking for web-specific frontend experience.'),
-- Lucian Barbu (User 12) — Security, skills: Pen-testing, SIEM, Docker
(12, 4, 'Accepted', '2026-03-07 11:00:00', 'Security background strong for DevOps.'),
(12, 10, 'Pending', '2026-03-20 10:00:00', NULL),
(12, 103, 'Advanced', '2026-03-14 15:00:00', 'Moving to final round. Infra security knowledge.'),
-- Maria Enescu (User 13) — Junior, skills: Java, Spring Boot
(13, 1, 'Pending', '2026-03-19 09:00:00', NULL),
(13, 100, 'Rejected', '2026-03-03 14:00:00', 'Too junior for senior backend role.'),
(13, 9, 'Pending', '2026-03-22 11:00:00', NULL),
-- Nicolae Grigorescu (User 14) — Engineering manager, skills: Leadership, Architecture, Agile
(14, 8, 'Advanced', '2026-03-04 10:00:00', 'Exceptional leadership. Offer stage.'),
(14, 10, 'Accepted', '2026-03-11 14:00:00', 'Architecture skills excellent for cloud role.'),
(14, 11, 'Pending', '2026-03-23 09:00:00', NULL),
-- Oana Cristea (User 15) — Data engineer, skills: Python, Spark, SQL
(15, 5, 'Accepted', '2026-03-06 09:00:00', 'Strong data pipeline experience.'),
(15, 101, 'Advanced', '2026-03-15 11:00:00', 'Python expertise confirmed. Technical final.'),
(15, 6, 'Pending', '2026-03-21 16:00:00', NULL),
-- Pavel Dragomir (User 16) — Backend Go, skills: Go, PostgreSQL
(16, 100, 'Pending', '2026-03-17 10:00:00', NULL),
(16, 2, 'Rejected', '2026-03-05 13:00:00', 'Role requires C# not Go.'),
(16, 9, 'Accepted', '2026-03-12 09:00:00', 'Good backend skills for full-stack role.'),
-- Raluca Stoica (User 17) — CV engineer, skills: Computer Vision, PyTorch, Python
(17, 6, 'Accepted', '2026-03-08 10:00:00', 'Outstanding CV and ML profile.'),
(17, 101, 'Pending', '2026-03-20 14:00:00', NULL),
(17, 8, 'Rejected', '2026-03-01 16:00:00', 'Tech lead role needs broader management experience.'),
-- Stefan Marinescu (User 18) — Full-stack Angular/.NET, skills: Angular, C#, SQL
(18, 9, 'Advanced', '2026-03-09 11:00:00', 'Strong full-stack profile. Final interview.'),
(18, 2, 'Accepted', '2026-03-14 10:00:00', 'C# and SQL skills match backend .NET role.'),
(18, 102, 'Pending', '2026-03-24 09:00:00', NULL),
-- Teodora Voinea (User 19) — Frontend Vue.js, skills: Vue.js, TypeScript
(19, 1, 'Pending', '2026-03-16 13:00:00', NULL),
(19, 102, 'Accepted', '2026-03-10 09:00:00', 'TypeScript skills excellent for frontend role.'),
(19, 7, 'Rejected', '2026-03-04 11:00:00', 'UI/UX role needs design tool experience.'),
-- Vlad Petrescu (User 20) — Cloud architect, skills: Cloud, AWS, Docker, K8s
(20, 10, 'Accepted', '2026-03-07 10:00:00', 'AWS and Azure certified. Top cloud candidate.'),
(20, 4, 'Advanced', '2026-03-13 15:00:00', 'K8s expertise strong. Final round.'),
(20, 103, 'Accepted', '2026-03-18 09:00:00', 'Excellent DevOps and cloud skills.');

-- ============================================================
-- STEP 6: Chats (between users and companies about jobs)
-- UserId: in-memory user IDs (1-20)
-- CompanyId: in-memory company IDs (1-10)
-- SecondUserId: for dev-to-dev chats (uses in-memory user IDs)
-- CHECK constraint: exactly one of CompanyId/SecondUserId must be set
-- ============================================================
INSERT INTO dbo.Chat (UserId, CompanyId, SecondUserId, JobId, IsBlocked, BlockedByUserId, DeletedAtByUser, DeletedAtBySecondParty) VALUES
-- Chat 1: Alice (1) <-> PixelSoft (4) about Junior Frontend (Job 1)
(1, 4, NULL, 1, 0, NULL, NULL, NULL),
-- Chat 2: Katerina (11) <-> TechNova (1) about Backend .NET (Job 2)
(11, 1, NULL, 2, 0, NULL, NULL, NULL),
-- Chat 3: Alice (1) <-> DataForge (3) about Data Analyst (Job 5)
(1, 3, NULL, 5, 0, NULL, NULL, NULL),
-- Chat 4: Alice (1) <-> BrightSystems (8) about QA Automation (Job 3)
(1, 8, NULL, 3, 0, NULL, NULL, NULL),
-- Chat 5: Alice (1) <-> AI Valley (9) about ML Engineer (Job 6)
(1, 9, NULL, 6, 0, NULL, NULL, NULL),
-- Chat 6: Bogdan (2) <-> TechNova (1) about Backend .NET (Job 2)
(2, 1, NULL, 2, 0, NULL, NULL, NULL),
-- Chat 7: Carmen (3) <-> BrightSystems (8) about QA Automation (Job 3)
(3, 8, NULL, 3, 0, NULL, NULL, NULL),
-- Chat 8: Dan (4) <-> CloudWorks (2) about DevOps Engineer (Job 4)
(4, 2, NULL, 4, 0, NULL, NULL, NULL),
-- Chat 9: Elena (5) <-> DataForge (3) about Data Analyst (Job 5)
(5, 3, NULL, 5, 0, NULL, NULL, NULL),
-- Chat 10: Florin (6) <-> AI Valley (9) about ML Engineer (Job 6)
(6, 9, NULL, 6, 0, NULL, NULL, NULL),
-- Chat 11: Teodora (19) <-> RocketApps (7) about UI/UX Designer (Job 7)
(19, 7, NULL, 7, 0, NULL, NULL, NULL),
-- Chat 12: Horia (8) <-> CodeBridge (10) about Technical Lead (Job 8)
(8, 10, NULL, 8, 0, NULL, NULL, NULL),
-- Chat 13: Ioana (9) <-> GreenCode (6) about Full-Stack Developer (Job 9)
(9, 6, NULL, 9, 0, NULL, NULL, NULL),
-- Chat 14: Julian (10) <-> SecureOps (5) about Cloud Architect (Job 10)
(10, 5, NULL, 10, 0, NULL, NULL, NULL),
-- Chat 15: Bogdan (2) <-> TechNova (1) about Backend Engineer (Job 100)
(2, 1, NULL, 100, 0, NULL, NULL, NULL),
-- Chat 16: Dan (4) <-> TechNova (1) about DevOps Engineer (Job 103)
(4, 1, NULL, 103, 0, NULL, NULL, NULL),
-- Chat 17: User-to-user chat: Alice (1) <-> Bogdan (2)
(1, NULL, 2, NULL, 0, NULL, NULL, NULL),
-- Chat 18: Katerina (11) <-> PixelSoft (4) about Junior Frontend (Job 1)
(11, 4, NULL, 1, 0, NULL, NULL, NULL),
-- Chat 19: Lucian (12) <-> CloudWorks (2) about DevOps (Job 4)
(12, 2, NULL, 4, 0, NULL, NULL, NULL),
-- Chat 20: Nicolae (14) <-> CodeBridge (10) about Technical Lead (Job 8)
(14, 10, NULL, 8, 0, NULL, NULL, NULL),
-- Chat 21: Oana (15) <-> DataForge (3) about Data Analyst (Job 5)
(15, 3, NULL, 5, 0, NULL, NULL, NULL),
-- Chat 22: Stefan (18) <-> TechNova (1) about Backend .NET (Job 2)
(18, 1, NULL, 2, 0, NULL, NULL, NULL),
-- Chat 23: Vlad (20) <-> SecureOps (5) about Cloud Architect (Job 10)
(20, 5, NULL, 10, 0, NULL, NULL, NULL),
-- Chat 24: Blocked: Maria (13) <-> TechNova (1) about Backend Engineer (Job 100)
(13, 1, NULL, 100, 1, 13, NULL, NULL),
-- Chat 25: Deleted: Maria (13) <-> DataForge (3) about Data Analyst (Job 5)
(13, 3, NULL, 5, 0, NULL, '2026-03-18 08:00:00', NULL);

-- ============================================================
-- STEP 7: Messages (conversation threads in chats)
-- Type: 0 = Text, 1 = Image, 2 = File
-- Columns: Content, SenderId, Timestamp, ChatId, Type, IsRead
-- ============================================================
INSERT INTO dbo.Message (Content, SenderId, Timestamp, ChatId, Type, IsRead) VALUES
-- Chat 1: Alice <-> PixelSoft (Junior Frontend)
('Hi Alice! Thanks for applying to the Junior Frontend Developer position.', 4, '2026-03-15 09:00:00', 1, 0, 1),
('Thank you! I am very excited about this opportunity.', 1, '2026-03-15 09:30:00', 1, 0, 1),
('Your React skills look great. Can we schedule a technical interview?', 4, '2026-03-16 10:00:00', 1, 0, 1),
('Absolutely! I am available next Tuesday or Wednesday.', 1, '2026-03-16 10:30:00', 1, 0, 1),
('Great, let us do Tuesday at 2 PM. Here is the meeting link.', 4, '2026-03-16 11:00:00', 1, 0, 1),
('The interview went well! We would like to extend an offer.', 4, '2026-03-19 14:00:00', 1, 0, 1),
('That is wonderful news! When can I start?', 1, '2026-03-20 14:00:00', 1, 0, 0),

-- Chat 2: Katerina <-> TechNova (Backend .NET Job 2)
('Hi Katerina, we noticed your C# skills. Interested in our backend role?', 1, '2026-03-19 09:00:00', 2, 0, 1),
('Yes! I have been working on .NET projects recently.', 11, '2026-03-19 10:00:00', 2, 0, 1),
('Great. We will send you a take-home coding challenge.', 1, '2026-03-20 11:00:00', 2, 0, 0),

-- Chat 3: Alice <-> DataForge (Data Analyst Job 5)
('Alice, your analytical background caught our eye.', 3, '2026-03-23 09:00:00', 3, 0, 1),
('Thank you! I enjoy working with data and Python.', 1, '2026-03-23 10:00:00', 3, 0, 1),
('We have an open data analyst position. Can we chat more?', 3, '2026-03-24 11:00:00', 3, 0, 0),

-- Chat 4: Alice <-> BrightSystems (QA Automation Job 3)
('Alice, we saw you applied for our QA role.', 8, '2026-03-13 09:00:00', 4, 0, 1),
('Yes! I have some testing experience from my React projects.', 1, '2026-03-13 10:00:00', 4, 0, 1),
('We accepted your application. Welcome aboard!', 8, '2026-03-14 11:00:00', 4, 0, 1),

-- Chat 5: Alice <-> AI Valley (ML Engineer Job 6)
('Hi Alice, interested in exploring ML engineering?', 9, '2026-03-26 09:00:00', 5, 0, 0),

-- Chat 6: Bogdan <-> TechNova (Backend .NET Job 2)
('Hello Bogdan, we reviewed your backend profile and were impressed!', 1, '2026-03-14 09:00:00', 6, 0, 1),
('Thank you so much! I have been working with C# and SQL for years.', 2, '2026-03-14 09:45:00', 6, 0, 1),
('Can you do a live coding session with our team?', 1, '2026-03-15 11:00:00', 6, 0, 1),
('Sure, I would love that. When works for you?', 2, '2026-03-15 11:30:00', 6, 0, 1),
('How about Friday at 10 AM?', 1, '2026-03-15 14:00:00', 6, 0, 1),
('Perfect. Looking forward to it!', 2, '2026-03-15 14:15:00', 6, 0, 1),
('The session went great! We loved your API design approach.', 1, '2026-03-18 10:30:00', 6, 0, 1),

-- Chat 7: Carmen <-> BrightSystems (QA Automation)
('Carmen, your QA automation experience is exactly what we need.', 8, '2026-03-12 09:00:00', 7, 0, 1),
('Thank you! I love building reliable test suites.', 3, '2026-03-12 10:00:00', 7, 0, 1),
('We would like to do a practical testing exercise. Available next week?', 8, '2026-03-14 09:00:00', 7, 0, 1),
('Yes, any day works for me.', 3, '2026-03-14 10:00:00', 7, 0, 1),
('Excellent work on the exercise! Moving to final round.', 8, '2026-03-19 15:00:00', 7, 0, 1),
('Final interview next week.', 8, '2026-03-21 09:00:00', 7, 0, 0),

-- Chat 8: Dan <-> CloudWorks (DevOps)
('Dan, your DevOps and Kubernetes experience is impressive.', 2, '2026-03-10 09:00:00', 8, 0, 1),
('Thank you! I am passionate about CI/CD and infrastructure.', 4, '2026-03-10 10:00:00', 8, 0, 1),
('We are building a new cloud-native platform. Interested?', 2, '2026-03-11 11:00:00', 8, 0, 1),
('Definitely! Can you share more details?', 4, '2026-03-11 12:00:00', 8, 0, 1),
('Here is our infrastructure roadmap overview.', 2, '2026-03-12 09:00:00', 8, 2, 1),
('This looks amazing. I would love to contribute.', 4, '2026-03-12 10:00:00', 8, 0, 1),
('Can you present your Kubernetes migration plan?', 2, '2026-03-19 15:00:00', 8, 0, 0),

-- Chat 9: Elena <-> DataForge (Data Analyst)
('Elena, your Python and data analysis background is outstanding.', 3, '2026-03-08 09:00:00', 9, 0, 1),
('Thanks! I have done extensive work with pandas and dashboarding.', 5, '2026-03-08 10:00:00', 9, 0, 1),
('We need someone to lead our analytics team.', 3, '2026-03-09 11:00:00', 9, 0, 1),
('I would be honored. Data storytelling is my passion.', 5, '2026-03-09 12:00:00', 9, 0, 1),
('Offer extended! Background check in progress.', 3, '2026-03-22 11:00:00', 9, 0, 1),

-- Chat 10: Florin <-> AI Valley (ML Engineer)
('Florin, your ML and NLP expertise is exactly what we need.', 9, '2026-03-15 09:00:00', 10, 0, 1),
('I have been building NLP pipelines for production use cases.', 6, '2026-03-15 10:00:00', 10, 0, 1),
('Can you walk us through a model you deployed?', 9, '2026-03-16 11:00:00', 10, 0, 1),
('Of course! I deployed a sentiment analysis service last quarter.', 6, '2026-03-16 12:00:00', 10, 0, 1),
('We are ready to make an offer. Details attached.', 9, '2026-03-23 16:00:00', 10, 2, 0),

-- Chat 11: Teodora <-> RocketApps (UI/UX Designer)
('Teodora, your design portfolio is well-rounded.', 7, '2026-03-12 13:00:00', 11, 0, 1),
('Thanks! I enjoy creating intuitive user experiences.', 19, '2026-03-12 14:00:00', 11, 0, 1),
('Design challenge results look great.', 7, '2026-03-20 13:00:00', 11, 0, 1),

-- Chat 12: Horia <-> CodeBridge (Technical Lead)
('Horia, your architecture and leadership experience is exceptional.', 10, '2026-03-05 09:00:00', 12, 0, 1),
('Thank you! I have led teams of 10+ engineers.', 8, '2026-03-05 10:00:00', 12, 0, 1),
('We would like you to lead our platform team. Interested?', 10, '2026-03-06 11:00:00', 12, 0, 1),
('Absolutely! This aligns perfectly with my career goals.', 8, '2026-03-06 12:00:00', 12, 0, 1),
('Offer accepted! Welcome aboard! Start date?', 10, '2026-03-24 10:00:00', 12, 0, 0),

-- Chat 13: Ioana <-> GreenCode (Full-Stack)
('Ioana, your full-stack skills caught our attention.', 6, '2026-03-15 09:00:00', 13, 0, 1),
('Thanks! I have been building React + C# apps for 3 years.', 9, '2026-03-15 10:00:00', 13, 0, 1),
('Can you walk us through your latest project?', 6, '2026-03-21 14:00:00', 13, 0, 0),

-- Chat 14: Julian <-> SecureOps (Cloud Architect)
('Julian, your cloud architecture expertise is rare.', 5, '2026-03-14 09:00:00', 14, 0, 1),
('I love designing scalable cloud infrastructure.', 10, '2026-03-14 10:00:00', 14, 0, 1),
('We have a system design assessment. Can you do it this week?', 5, '2026-03-18 11:00:00', 14, 0, 1),
('Of course! I will prepare my architecture diagrams.', 10, '2026-03-18 12:00:00', 14, 0, 1),
('Assessment scheduled for Thursday.', 5, '2026-03-22 09:30:00', 14, 0, 1),

-- Chat 15: Bogdan <-> TechNova (Backend Engineer Job 100)
('Bogdan, we also have a Backend Engineer position open.', 1, '2026-03-12 09:00:00', 15, 0, 1),
('Sounds interesting! How does it differ from the .NET role?', 2, '2026-03-12 10:00:00', 15, 0, 1),
('This one focuses more on microservices and API design.', 1, '2026-03-19 11:00:00', 15, 0, 1),

-- Chat 16: Dan <-> TechNova (DevOps Job 103)
('Dan, your DevOps profile matches our infrastructure role too.', 1, '2026-03-13 09:00:00', 16, 0, 1),
('Thanks! I saw the job description. CI/CD is my specialty.', 4, '2026-03-13 10:00:00', 16, 0, 1),
('We have a pipeline design exercise for you.', 1, '2026-03-18 14:00:00', 16, 0, 1),
('Pipeline design exercise coming up.', 1, '2026-03-20 16:00:00', 16, 0, 0),

-- Chat 17: User-to-user (Alice <-> Bogdan)
('Hey Bogdan, did you see the new weighted distance proposal?', 1, '2026-03-20 15:00:00', 17, 0, 1),
('Yeah, I think 30% is too high. What do you think?', 2, '2026-03-20 15:30:00', 17, 0, 1),
('I agree. Resume similarity should be weighted higher.', 1, '2026-03-20 16:00:00', 17, 0, 1),
('Thoughts on the new weight proposal?', 2, '2026-03-21 17:00:00', 17, 0, 1),

-- Chat 18: Katerina <-> PixelSoft (Junior Frontend)
('Hi Katerina! Your mobile dev background is interesting for our frontend role.', 4, '2026-03-10 09:00:00', 18, 0, 1),
('Thanks! I have been wanting to transition to web frontend.', 11, '2026-03-10 10:00:00', 18, 0, 1),
('Can we do a take-home React exercise?', 4, '2026-03-12 09:00:00', 18, 0, 0),

-- Chat 19: Lucian <-> CloudWorks (DevOps)
('Lucian, your security background is valuable for DevOps.', 2, '2026-03-11 09:00:00', 19, 0, 1),
('Thanks! Infrastructure security is my focus area.', 12, '2026-03-11 10:00:00', 19, 0, 1),
('We have a Kubernetes security assessment for you.', 2, '2026-03-15 14:00:00', 19, 0, 0),

-- Chat 20: Nicolae <-> CodeBridge (Technical Lead)
('Nicolae, 10 years of experience and team scaling is impressive.', 10, '2026-03-08 09:00:00', 20, 0, 1),
('Thank you! I enjoy building high-performing teams.', 14, '2026-03-08 10:00:00', 20, 0, 1),
('We are looking for someone to lead our new product division.', 10, '2026-03-09 11:00:00', 20, 0, 1),
('Offer extended! We would love to have you on board.', 10, '2026-03-20 14:00:00', 20, 0, 0),

-- Chat 21: Oana <-> DataForge (Data Analyst)
('Oana, your Spark and data pipeline skills are spot on.', 3, '2026-03-10 09:00:00', 21, 0, 1),
('Thank you! I love working with big data tools.', 15, '2026-03-10 10:00:00', 21, 0, 1),
('When can you start a technical discussion with our data team?', 3, '2026-03-14 11:00:00', 21, 0, 0),

-- Chat 22: Stefan <-> TechNova (Backend .NET)
('Stefan, your Angular and .NET combo is exactly what we need.', 1, '2026-03-16 09:00:00', 22, 0, 1),
('Thanks! I have built several full-stack apps with that stack.', 18, '2026-03-16 10:00:00', 22, 0, 1),
('Can you share a code sample or portfolio?', 1, '2026-03-17 11:00:00', 22, 0, 1),
('Looks solid! Let us schedule a pair programming session.', 1, '2026-03-20 09:00:00', 22, 0, 0),

-- Chat 23: Vlad <-> SecureOps (Cloud Architect)
('Vlad, your AWS and Azure certs are exactly what we need.', 5, '2026-03-12 09:00:00', 23, 0, 1),
('Thank you! Cloud architecture is my specialty.', 20, '2026-03-12 10:00:00', 23, 0, 1),
('We want you to design our multi-cloud landing zone.', 5, '2026-03-15 11:00:00', 23, 0, 1),
('Great work on the presentation! Offer incoming.', 5, '2026-03-22 14:00:00', 23, 0, 0);

-- ============================================================
-- STEP 8: Recommendations (algorithm-generated job suggestions)
-- UserIDs: 1-20, JobIDs: 1-12, 100-104
-- ============================================================
INSERT INTO dbo.Recommendation (UserId, JobId, Timestamp) VALUES
-- Alice Pop (User 1) — lots of recommendations to test the full flow
(1, 1, '2026-03-01 08:00:00'), (1, 9, '2026-03-01 08:00:00'), (1, 102, '2026-03-01 08:00:00'),
(1, 7, '2026-03-01 08:00:00'), (1, 2, '2026-03-01 08:00:00'), (1, 5, '2026-03-01 08:00:00'),
(1, 3, '2026-03-01 08:00:00'), (1, 6, '2026-03-01 08:00:00'), (1, 100, '2026-03-01 08:00:00'),
(1, 4, '2026-03-01 08:00:00'), (1, 8, '2026-03-01 08:00:00'), (1, 104, '2026-03-01 08:00:00'),
-- Bogdan Ionescu (User 2) — Backend .NET
(2, 2, '2026-03-01 08:00:00'), (2, 100, '2026-03-01 08:00:00'), (2, 9, '2026-03-01 08:00:00'), (2, 101, '2026-03-01 08:00:00'), (2, 8, '2026-03-01 08:00:00'),
-- Carmen Radu (User 3) — QA
(3, 3, '2026-03-01 08:00:00'), (3, 104, '2026-03-01 08:00:00'),
-- Dan Tudor (User 4) — DevOps
(4, 4, '2026-03-01 08:00:00'), (4, 103, '2026-03-01 08:00:00'), (4, 10, '2026-03-01 08:00:00'),
-- Elena Matei (User 5) — Data analyst
(5, 5, '2026-03-01 08:00:00'), (5, 101, '2026-03-01 08:00:00'), (5, 6, '2026-03-01 08:00:00'),
-- Florin Pavel (User 6) — ML engineer
(6, 6, '2026-03-01 08:00:00'), (6, 100, '2026-03-01 08:00:00'), (6, 101, '2026-03-01 08:00:00'), (6, 5, '2026-03-01 08:00:00'),
-- Gabriela Stan (User 7) — UI/UX
(7, 7, '2026-03-01 08:00:00'), (7, 1, '2026-03-01 08:00:00'),
-- Horia Vasile (User 8) — Tech lead
(8, 8, '2026-03-01 08:00:00'), (8, 100, '2026-03-01 08:00:00'), (8, 103, '2026-03-01 08:00:00'), (8, 10, '2026-03-01 08:00:00'),
-- Ioana Dobre (User 9) — Full-stack
(9, 9, '2026-03-01 08:00:00'), (9, 1, '2026-03-01 08:00:00'), (9, 2, '2026-03-01 08:00:00'), (9, 102, '2026-03-01 08:00:00'),
-- Julian Muresan (User 10) — Cloud architect
(10, 10, '2026-03-01 08:00:00'), (10, 4, '2026-03-01 08:00:00'), (10, 103, '2026-03-01 08:00:00'), (10, 8, '2026-03-01 08:00:00'),
-- Katerina Lupu (User 11) — Mobile/Frontend
(11, 1, '2026-03-01 08:00:00'), (11, 7, '2026-03-01 08:00:00'), (11, 102, '2026-03-01 08:00:00'),
-- Lucian Barbu (User 12) — Security/DevOps
(12, 4, '2026-03-01 08:00:00'), (12, 10, '2026-03-01 08:00:00'), (12, 103, '2026-03-01 08:00:00'),
-- Maria Enescu (User 13) — Junior Java
(13, 1, '2026-03-01 08:00:00'), (13, 100, '2026-03-01 08:00:00'), (13, 9, '2026-03-01 08:00:00'),
-- Nicolae Grigorescu (User 14) — Eng manager
(14, 8, '2026-03-01 08:00:00'), (14, 10, '2026-03-01 08:00:00'), (14, 11, '2026-03-01 08:00:00'),
-- Oana Cristea (User 15) — Data engineer
(15, 5, '2026-03-01 08:00:00'), (15, 101, '2026-03-01 08:00:00'), (15, 6, '2026-03-01 08:00:00'),
-- Pavel Dragomir (User 16) — Backend Go
(16, 100, '2026-03-01 08:00:00'), (16, 2, '2026-03-01 08:00:00'), (16, 9, '2026-03-01 08:00:00'),
-- Raluca Stoica (User 17) — CV/ML engineer
(17, 6, '2026-03-01 08:00:00'), (17, 101, '2026-03-01 08:00:00'), (17, 8, '2026-03-01 08:00:00'),
-- Stefan Marinescu (User 18) — Full-stack Angular/.NET
(18, 9, '2026-03-01 08:00:00'), (18, 2, '2026-03-01 08:00:00'), (18, 102, '2026-03-01 08:00:00'),
-- Teodora Voinea (User 19) — Frontend Vue.js
(19, 1, '2026-03-01 08:00:00'), (19, 102, '2026-03-01 08:00:00'), (19, 7, '2026-03-01 08:00:00'),
-- Vlad Petrescu (User 20) — Cloud architect
(20, 10, '2026-03-01 08:00:00'), (20, 4, '2026-03-01 08:00:00'), (20, 103, '2026-03-01 08:00:00');

-- ============================================================
-- STEP 9: Integrity assertions
-- ============================================================
IF EXISTS (
    SELECT 1
    FROM dbo.Chat
    WHERE (CompanyId IS NOT NULL AND SecondUserId IS NOT NULL)
       OR (CompanyId IS NULL AND SecondUserId IS NULL)
)
BEGIN
    THROW 51001, 'Integrity check failed: invalid chat shape (CompanyId/SecondUserId).', 1;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.Interaction i
    LEFT JOIN dbo.Post p ON p.PostID = i.PostID
    WHERE p.PostID IS NULL
)
BEGIN
    THROW 51002, 'Integrity check failed: orphan Interaction.PostID detected.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.Interaction i
    LEFT JOIN dbo.Developer d ON d.DeveloperID = i.DeveloperID
    WHERE d.DeveloperID IS NULL
)
BEGIN
    THROW 51003, 'Integrity check failed: orphan Interaction.DeveloperID detected.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.Message m
    LEFT JOIN dbo.Chat c ON c.ChatId = m.ChatId
    WHERE c.ChatId IS NULL
)
BEGIN
    THROW 51004, 'Integrity check failed: orphan Message.ChatId detected.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.[Matches]
    WHERE Status NOT IN ('Pending', 'Accepted', 'Rejected', 'Advanced')
)
BEGIN
    THROW 51005, 'Integrity check failed: invalid Matches.Status value detected.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.Message m
    INNER JOIN dbo.Chat c ON c.ChatId = m.ChatId
    WHERE (c.CompanyId IS NOT NULL AND m.SenderID NOT IN (c.UserId, c.CompanyId))
       OR (c.SecondUserId IS NOT NULL AND m.SenderID NOT IN (c.UserId, c.SecondUserId))
)
BEGIN
    THROW 51006, 'Integrity check failed: Message.SenderId is not a participant of its chat.', 1;
END;

DECLARE @DeveloperCount INT = (SELECT COUNT(*) FROM dbo.Developer);
DECLARE @PostCount INT = (SELECT COUNT(*) FROM dbo.Post);
DECLARE @InteractionCount INT = (SELECT COUNT(*) FROM dbo.Interaction);
DECLARE @MatchesCount INT = (SELECT COUNT(*) FROM dbo.[Matches]);
DECLARE @ChatCount INT = (SELECT COUNT(*) FROM dbo.Chat);
DECLARE @MessageCount INT = (SELECT COUNT(*) FROM dbo.Message);
DECLARE @RecommendationCount INT = (SELECT COUNT(*) FROM dbo.Recommendation);

COMMIT TRAN;

-- ============================================================
-- Summary
-- ============================================================
PRINT '=== Mock Data Generation Complete ===';
PRINT 'Developer:      ' + CAST(@DeveloperCount AS VARCHAR(20));
PRINT 'Post:           ' + CAST(@PostCount AS VARCHAR(20));
PRINT 'Interaction:    ' + CAST(@InteractionCount AS VARCHAR(20));
PRINT 'Matches:        ' + CAST(@MatchesCount AS VARCHAR(20));
PRINT 'Chat:           ' + CAST(@ChatCount AS VARCHAR(20));
PRINT 'Message:        ' + CAST(@MessageCount AS VARCHAR(20));
PRINT 'Recommendation: ' + CAST(@RecommendationCount AS VARCHAR(20));
PRINT 'All SQL integrity checks passed.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRAN;
    END;

    DECLARE @ErrorMessage NVARCHAR(4000) =
        CONCAT('GenerateMockData.sql failed at line ', ERROR_LINE(), ': ', ERROR_MESSAGE());

    THROW 51000, @ErrorMessage, 1;
END CATCH;
