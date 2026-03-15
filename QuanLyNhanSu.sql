CREATE DATABASE FaceID_HRMS; ---HỆ THỐNG QUẢN LÝ NHÂN SỰ VÀ CHẤM CÔNG AI BẰNG FACEID
GO
USE FaceID_HRMS;
GO

--Bảng vai trò 
CREATE TABLE Roles (
    Id INT IDENTITY PRIMARY KEY,        -- Mã vai trò
    RoleName NVARCHAR(50) NOT NULL,     -- Tên vai trò (Admin, HR, Nhân viên)
    Description NVARCHAR(255)           -- Mô tả vai trò
);

-- DỮ LIỆU MẪU
INSERT INTO Roles (RoleName, Description)
VALUES
(N'Admin', N'Quản trị hệ thống'),
(N'HR', N'Nhân sự'),
(N'Employee', N'Nhân viên');



-- BẢNG PHÒNG BAN
CREATE TABLE Departments (
    Id INT IDENTITY PRIMARY KEY,
    DepartmentCode NVARCHAR(10) UNIQUE, -- IT, HR
    DepartmentName NVARCHAR(100),
    Description NVARCHAR(255)
);

--DỮ LIỆU MẪU 
INSERT INTO Departments (DepartmentCode, DepartmentName, Description)
VALUES
('IT', N'Phòng Công nghệ thông tin', N'Quản lý hệ thống'),
('HR', N'Phòng Nhân sự', N'Quản lý nhân sự');

INSERT INTO Departments (DepartmentCode, DepartmentName, Description)
VALUES
('FIN', N'Phòng Tài chính', N'Quản lý lương và tài chính'),
('MKT', N'Phòng Marketing', N'Truyền thông & quảng cáo'),
('OPS', N'Phòng Vận hành', N'Vận hành hệ thống');


-- BẢNG CHỨC VỤ
CREATE TABLE Positions (
    Id INT IDENTITY PRIMARY KEY,      -- Mã chức vụ
    PositionCode NVARCHAR(10) UNIQUE, -- Mã (NV, TP)
    PositionName NVARCHAR(100),       -- Tên đầy đủ
    BaseSalary DECIMAL(18,2)
);

--DỮ LIỆU MẪU
INSERT INTO Positions (PositionCode, PositionName, BaseSalary)
VALUES
('NV', N'Nhân viên', 8000000),
('TP', N'Trưởng phòng', 15000000);

INSERT INTO Positions (PositionCode, PositionName, BaseSalary)
VALUES
('PGD', N'Phó giám đốc', 25000000),
('KT', N'Kế toán', 12000000),
('MK', N'Nhân viên Marketing', 9000000);

-- BẢNG KINH NGHIỆM
CREATE TABLE ExperienceLevels (
    Id INT IDENTITY PRIMARY KEY,
    LevelName NVARCHAR(50),
    Description NVARCHAR(255)
);

INSERT INTO ExperienceLevels (LevelName, Description)
VALUES
(N'Thực tập', N'Sinh viên thực tập'),
(N'Thiếu kinh nghiệm', N'Mới đi làm dưới 1 năm'),
(N'Có kinh nghiệm', N'Trên 1 năm kinh nghiệm'),
(N'Senior', N'Trên 5 năm kinh nghiệm');

-- BẢNG NHÂN SỰ
CREATE TABLE Employees (
    Id INT IDENTITY PRIMARY KEY,              -- Mã nhân viên
    EmployeeCode NVARCHAR(20) UNIQUE NOT NULL,-- Mã nhân sự
    FullName NVARCHAR(100) NOT NULL,          -- Họ và tên
    Gender NVARCHAR(10),                      -- Giới tính
    DateOfBirth DATETIME,                         -- Ngày sinh
    Phone NVARCHAR(20),                       -- Số điện thoại
    Email NVARCHAR(100),                      -- Email
    DepartmentId INT,                         -- Phòng ban
    PositionId INT,                           -- Chức vụ
    HireDate DATETIME,                
	Address NVARCHAR(255) NULL,-- Ngày vào làm
    Status NVARCHAR(50) DEFAULT N'Đang làm'   -- Trạng thái làm việc

);


ALTER TABLE Employees
ADD Avatar NVARCHAR(255);
ALTER TABLE Employees
ADD ExperienceLevelId INT;


-- KHÓA NGOẠI NHÂN SỰ VỚI PHÒNG BAN
	ALTER TABLE Employees
	ADD CONSTRAINT FK_Employees_Departments
	FOREIGN KEY (DepartmentId) REFERENCES Departments(Id);

-- KHÓA NGOẠI NHÂN SỰ VỚI VỊ TRÍ
	ALTER TABLE Employees
	ADD CONSTRAINT FK_Employees_Positions
	FOREIGN KEY (PositionId) REFERENCES Positions(Id);

	-- KHÓA NGOẠI VỚI KINH NGHIỆM
	ALTER TABLE Employees
ADD CONSTRAINT FK_Employees_ExperienceLevels
FOREIGN KEY (ExperienceLevelId)
REFERENCES ExperienceLevels(Id);


-- DỮ LIỆU MẪU
-- Chèn dữ liệu mẫu đầy đủ với địa chỉ
INSERT INTO Employees
(EmployeeCode, FullName, Gender, DateOfBirth, Phone, Email,DepartmentId, PositionId, HireDate, Address, Avatar,ExperienceLevelId)
VALUES
('IT-NV-001', N'Nguyễn Văn A', N'Nam', '2000-01-01', '0901111111', 'a@company.com', 1, 1, '2024-01-01', N'123 Đường IT, Quận 1', '/images/default.jpg','2'),
('HR-TP-001', N'Trần Thị B', N'Nữ', '1995-02-02', '0902222222', 'b@company.com', 2, 2, '2023-01-01', N'456 Đường HR, Quận 2', '/images/default.jpg','2'),
('IT-NV-002', N'Lê Hoàng C', N'Nam', '1999-03-15', '0903333333', 'c@company.com', 1, 1, '2024-03-01', N'789 Đường IT, Quận 1', '/images/default.jpg','3'),
('FIN-KT-001', N'Phạm Thị D', N'Nữ', '1998-07-20', '0904444444', 'd@company.com', 3, 4, '2023-05-10', N'321 Đường FIN, Quận 3', '/images/default.jpg','3'),
('MKT-MK-001', N'Võ Minh E', N'Nam', '2001-11-05', '0905555555', 'e@company.com', 4, 5, '2024-02-01', N'654 Đường MKT, Quận 4', '/images/default.jpg','4'),
('OPS-PGD-001', N'Nguyễn Hồng F', N'Nữ', '1990-06-01', '0906666666', 'f@company.com', 5, 3, '2020-01-01', N'987 Đường OPS, Quận 5', '/images/default.jpg','4');







-- BẢNG NGƯỜI DÙNG
CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,         -- Mã người dùng
    EmployeeId INT UNIQUE NOT NULL,           -- Liên kết nhân sự
    Username NVARCHAR(50) UNIQUE NOT NULL,    -- Mã nhân sự (VD: IT-NV-001)
    PasswordHash NVARCHAR(255) NOT NULL, -- Mật khẩu đã mã hóa
    Email NVARCHAR(100),                 -- Email
    IsActive BIT DEFAULT 1,              -- Trạng thái hoạt động
    CreatedAt DATETIME DEFAULT GETDATE() -- Ngày tạo
);

ALTER TABLE Users
ADD IsFirstLogin BIT NOT NULL DEFAULT 1;

--KHÓA NGOẠI CỦA NGƯỜI DÙNG VỚI NHÂN SỰ
	ALTER TABLE Users
	ADD CONSTRAINT FK_Users_Employees
	FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

-- DỮ LIỆU MẪU
INSERT INTO Users (EmployeeId, Username, PasswordHash, Email)
VALUES
(1, 'IT-NV-001', 'hash123', 'it-nv-001@company.com'),
(2, 'HR-TP-001', 'hash456', 'hr-tp-001@company.com');

INSERT INTO Users (EmployeeId, Username, PasswordHash, Email)
VALUES
(3, 'IT-NV-002', 'hash789', 'it-nv-002@company.com'),
(4, 'FIN-KT-001', 'hash101', 'fin-kt-001@company.com'),
(5, 'MKT-MK-001', 'hash102', 'mkt-mk-001@company.com'),
(6, 'OPS-PGD-001', 'hash103', 'ops-pgd-001@company.com');



-- BẢNG PHÂN QUYỀN
CREATE TABLE UserRoles
(
    Id INT IDENTITY(1,1) PRIMARY KEY,   -- ⭐ thêm surrogate key (QUAN TRỌNG)

    UserId INT NOT NULL,
    RoleId INT NOT NULL,

    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_UserRoles_Users
        FOREIGN KEY (UserId)
        REFERENCES Users(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_UserRoles_Roles
        FOREIGN KEY (RoleId)
        REFERENCES Roles(Id)
        ON DELETE CASCADE
);
GO



-- DỮ LIỆU MẪU
INSERT INTO UserRoles (UserId, RoleId)
VALUES
(1, 3), -- Nhân viên
(2, 2); -- HR

INSERT INTO UserRoles (UserId, RoleId)
VALUES
(3, 1), -- Nhân viên
(4, 3), -- FIN-KT-001 → Employee (Kế toán KHÔNG phải HR)
(5, 3), -- Marketing → Employee
(6, 2); -- Phó giám đốc → HR

-- BẢNG CA LÀM
-- ==============================
-- BẢNG CA LÀM VIỆC (SHIFTS)
-- ==============================
CREATE TABLE Shifts (
    Id INT IDENTITY(1,1) PRIMARY KEY,      -- Mã ca
    ShiftName NVARCHAR(50) NOT NULL,       -- Tên ca (Ca sáng, Ca chiều)
    StartTime TIME NOT NULL,               -- Giờ bắt đầu ca
    EndTime TIME NOT NULL,                 -- Giờ kết thúc ca
    LateThreshold TIME NOT NULL,           -- Mốc tính đi trễ
    IsActive BIT NOT NULL DEFAULT 1,       -- Ca còn sử dụng hay không
    IsAttendanceOpen BIT NOT NULL DEFAULT 1, -- Cho phép chấm công
    CreatedAt DATETIME2 DEFAULT GETDATE()  -- Ngày tạo
);
ALTER TABLE Shifts
ADD AllowAttendance BIT NOT NULL DEFAULT 1;
INSERT INTO Shifts 
(ShiftName, StartTime, EndTime, LateThreshold)
VALUES
(N'Ca sáng',  '07:30:00', '11:30:00', '07:35:00'),
(N'Ca chiều', '13:30:00', '17:30:00', '13:35:00');


-- BẢNG CA TRỰC
CREATE TABLE DutyShifts (
    Id INT IDENTITY PRIMARY KEY,
    DutyName NVARCHAR(50),      -- Ca trực ngày / Ca trực đêm
    StartTime TIME,
    EndTime TIME,
    IsOvernight BIT            -- 1 = qua ngày hôm sau
);
ALTER TABLE DutyShifts
ADD IsActive BIT NOT NULL DEFAULT 1,
    AllowAttendance BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE();
INSERT INTO DutyShifts (DutyName, StartTime, EndTime, IsOvernight)
VALUES
(N'Ca trực ngày', '07:00', '17:00', 0),
(N'Ca trực đêm', '17:00', '07:00', 1);

-- BẢNG LỊCH TRỰC
CREATE TABLE DutySchedules (
    Id INT IDENTITY PRIMARY KEY,
    EmployeeId INT NOT NULL,
    DutyShiftId INT NOT NULL,
    DutyDate DATETIME NOT NULL   -- ngày BẮT ĐẦU trực
);

ALTER TABLE DutySchedules
ADD CONSTRAINT FK_DutySchedules_Employees
FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

ALTER TABLE DutySchedules
ADD CONSTRAINT FK_DutySchedules_DutyShifts
FOREIGN KEY (DutyShiftId) REFERENCES DutyShifts(Id);


-- BẢNG CHẤM CÔNG TRỰC
CREATE TABLE DutyAttendance (
    Id INT IDENTITY PRIMARY KEY,
    EmployeeId INT NOT NULL,
    DutyShiftId INT NOT NULL,
    CheckTime DATETIME NOT NULL,
    DutyDate DATETIME NOT NULL,
    Status NVARCHAR(50), -- Đúng giờ / Trễ
    CreatedAt DATETIME DEFAULT GETDATE()
);

ALTER TABLE DutyAttendance
ADD CONSTRAINT FK_DutyAttendance_Employees
FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

ALTER TABLE DutyAttendance
ADD CONSTRAINT FK_DutyAttendance_DutyShifts
FOREIGN KEY (DutyShiftId) REFERENCES DutyShifts(Id);

-- Mỗi ca trực chỉ chấm 1 lần
CREATE UNIQUE INDEX UX_DutyAttendance
ON DutyAttendance (EmployeeId, DutyShiftId, DutyDate);


-- BẢNG LỊCH LÀM VIỆC
CREATE TABLE WorkSchedules (
    Id INT IDENTITY PRIMARY KEY, -- Mã lịch
    EmployeeId INT,              -- Nhân viên
    ShiftId INT,                 -- Ca làm
    WorkDate DATETIME                -- Ngày làm việc
);



-- KHÓA NGOẠI CỦA LỊCH LÀM VIỆC VỚI NHÂN SỰ
	ALTER TABLE WorkSchedules
	ADD CONSTRAINT FK_WorkSchedules_Employees
	FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

	-- KHÓA NGOẠI CỦA LỊCH LÀM VIỆC VỚI CA LÀM
	ALTER TABLE WorkSchedules
	ADD CONSTRAINT FK_WorkSchedules_Shifts
	FOREIGN KEY (ShiftId) REFERENCES Shifts(Id);


-- DỮ LIỆU MẪU
INSERT INTO WorkSchedules (EmployeeId, ShiftId, WorkDate)
VALUES
-- Thứ 2
(1, 1, '2026-01-05'), (1, 2, '2026-01-05'),
-- Thứ 3
(1, 1, '2026-01-06'), (1, 2, '2026-01-06'),
-- Thứ 4
(1, 1, '2026-01-07'), (1, 2, '2026-01-07'),
-- Thứ 5
(1, 1, '2026-01-08'), (1, 2, '2026-01-08'),
-- Thứ 6
(1, 1, '2026-01-09'), (1, 2, '2026-01-09');

INSERT INTO WorkSchedules (EmployeeId, ShiftId, WorkDate)
VALUES
-- Thứ 2
(2, 1, '2026-01-05'), (2, 2, '2026-01-05'),
-- Thứ 3
(2, 1, '2026-01-06'), (2, 2, '2026-01-06'),
-- Thứ 4
(2, 1, '2026-01-07'), (2, 2, '2026-01-07'),
-- Thứ 5
(2, 1, '2026-01-08'), (2, 2, '2026-01-08'),
-- Thứ 6
(2, 1, '2026-01-09'), (2, 2, '2026-01-09');


-- BẢNG ĐƠN NGHỈ VIỆC (PHÉP) ---
CREATE TABLE LeaveRequests (
    Id INT IDENTITY PRIMARY KEY,
    EmployeeId INT NOT NULL,
    FromDate DATETIME NOT NULL,
    ToDate DATETIME NOT NULL,
    LeaveType NVARCHAR(50), -- Phép năm, Không lương, Ốm
    Reason NVARCHAR(255),
    Status NVARCHAR(50) DEFAULT N'Chờ duyệt', -- Chờ duyệt / Duyệt / Từ chối
    CreatedAt DATETIME DEFAULT GETDATE()
);

ALTER TABLE LeaveRequests
ADD CONSTRAINT FK_LeaveRequests_Employees
FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);



ALTER TABLE LeaveRequests
ADD IsCompanyLeave BIT DEFAULT 0;



-- BẢNG HỒ SƠ KHUÔN MẶT
CREATE TABLE FaceEmbeddings (
    Id INT IDENTITY PRIMARY KEY, -- Mã hồ sơ mặt
    EmployeeId INT NOT NULL,     -- Nhân viên
    Embedding VARBINARY(MAX),    -- Vector khuôn mặt (AI)
    CreatedAt DATETIME DEFAULT GETDATE() -- Ngày tạo
);

--KHÓA NGOẠI CỦA HỒ SƠ KHUÔN MẶT VỚI NHÂN SỰ
	ALTER TABLE FaceEmbeddings
	ADD CONSTRAINT FK_FaceEmbeddings_Employees
	FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);


-- BẢNG KIỂM TRA NGƯỜI THẬT
CREATE TABLE LivenessLogs (
    Id INT IDENTITY PRIMARY KEY, -- Mã kiểm tra
    EmployeeId INT,              -- Nhân viên
    BlinkDetected BIT,           -- Có chớp mắt
    HeadMovement BIT,            -- Có xoay đầu
    Result BIT,                  -- Kết quả (1 = người thật)
    CreatedAt DATETIME DEFAULT GETDATE() -- Thời gian
);

-- KHÓA NGOẠI CỦA KIỂM TRA VỚI NHÂN SỰ
	ALTER TABLE LivenessLogs
	ADD CONSTRAINT FK_LivenessLogs_Employees
	FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

-- BẢNG CHẤM CÔNG
CREATE TABLE Attendance (
    Id INT IDENTITY PRIMARY KEY,         -- Mã chấm công
    EmployeeId INT NOT NULL,              -- Nhân viên
    ShiftId INT NOT NULL,                 -- Ca làm (Sáng / Chiều)
    CheckTime DATETIME NOT NULL,           -- Thời gian chấm
    WorkDate AS CAST(CheckTime AS DATETIME) PERSISTED, -- Ngày chấm công
    Status NVARCHAR(50),                  -- Đúng giờ / Trễ
    SimilarityScore FLOAT                 -- Độ giống khuôn mặt
);


-- KHÓA NGOẠI CỦA CHẤM CÔNG VỚI NHÂN SỰ
	ALTER TABLE Attendance
	ADD CONSTRAINT FK_Attendance_Employees
	FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

-- KHÓA NGOẠI CỦA CHẤM CÔNG VỚI CA LÀM
	ALTER TABLE Attendance
	ADD CONSTRAINT FK_Attendance_Shifts
	FOREIGN KEY (ShiftId) REFERENCES Shifts(Id);

	-- Mỗi nhân viên chỉ được chấm 1 lần / ca / ngày
CREATE UNIQUE INDEX UX_Attendance
ON Attendance (EmployeeId, ShiftId, WorkDate);

-- DỮ LIỆU MẪU
INSERT INTO Attendance
(EmployeeId, ShiftId, CheckTime, Status, SimilarityScore)
VALUES
(1, 1, '2026-01-07 07:32:00', N'Đúng giờ', 0.93),
(1, 2, '2026-01-07 13:31:00', N'Đúng giờ', 0.91);





-- BẢNG NHẬT KÝ HỆ THỐNG
CREATE TABLE SystemLogs (
    Id INT IDENTITY PRIMARY KEY, -- Mã nhật ký
    UserId INT,                  -- Người thao tác
    Action NVARCHAR(100),        -- Hành động
    Description NVARCHAR(255),   -- Mô tả
    CreatedAt DATETIME DEFAULT GETDATE() -- Thời gian
);

-- KHÓA NGOẠI CẢU NHẬT KÝ VỚI NGƯỜI DÙNG
ALTER TABLE SystemLogs
ADD CONSTRAINT FK_SystemLogs_Users
FOREIGN KEY (UserId) REFERENCES Users(Id);

-- BẢNG BÁO CÁO
CREATE TABLE Reports (
    Id INT IDENTITY PRIMARY KEY, -- Mã báo cáo
    ReportType NVARCHAR(100),    -- Loại báo cáo
    ReportDate DATETIME,             -- Ngày báo cáo
    CreatedAt DATETIME DEFAULT GETDATE() -- Ngày tạo
);

--13/2/2026
-- BẢNG CAMERA (TRƯỜNG HỢP CHO NHIỀU CAM)
CREATE TABLE Cameras (
    Id INT IDENTITY PRIMARY KEY, 
    -- Khóa chính tự tăng của camera

    CameraCode NVARCHAR(50) UNIQUE NOT NULL, 
    -- Mã định danh duy nhất của camera (VD: CAM-01)

    CameraName NVARCHAR(100), 
    -- Tên hiển thị của camera

    Location NVARCHAR(255), 
    -- Vị trí lắp đặt camera (VD: Sảnh tầng 1)

    IsActive BIT DEFAULT 1, 
    -- Trạng thái hoạt động (1: đang bật, 0: tắt)

    CreatedAt DATETIME DEFAULT GETDATE()
    -- Thời điểm tạo bản ghi
);
INSERT INTO Cameras (CameraCode, CameraName, Location)
VALUES (
    'CAM-LAPTOP',
    N'Laptop Camera',
    N'Máy phát triển'
);


--BẢNG BODY EMBEDDINGS (Vector dáng người)
CREATE TABLE BodyEmbeddings (
    Id INT IDENTITY PRIMARY KEY,
    -- Khóa chính tự tăng

    EmployeeId INT NOT NULL,
    -- Nhân viên sở hữu embedding này

    Embedding VARBINARY(MAX) NOT NULL,
    -- Vector đặc trưng dáng người (lưu dạng nhị phân)

    CreatedAt DATETIME DEFAULT GETDATE()
    -- Thời điểm tạo embedding
);
ALTER TABLE BodyEmbeddings
ADD CONSTRAINT FK_BodyEmbeddings_Employees
FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

--BẢNG MOVEMENT LOGS (Log mọi người đi qua camera)
CREATE TABLE MovementLogs (
    Id INT IDENTITY PRIMARY KEY,
    -- Khóa chính tự tăng

    CameraId INT NOT NULL,
    -- Camera ghi nhận chuyển động

    PersonType NVARCHAR(50),
    -- Loại đối tượng: Known / Unknown / Suspicious

    EmployeeId INT NULL,
    -- Nếu nhận diện được nhân viên thì lưu ID

    FaceSimilarity FLOAT NULL,
    -- Độ tương đồng khuôn mặt (0 → 1)

    BodySimilarity FLOAT NULL,
    -- Độ tương đồng dáng người (0 → 1)

    SnapshotImage VARBINARY(MAX) NULL,
    -- Ảnh chụp tại thời điểm phát hiện (lưu trong DB)

    SnapshotPath NVARCHAR(255) NULL,
    -- Đường dẫn file ảnh nếu lưu ngoài hệ thống file

    TrackingId NVARCHAR(100) NULL,
    -- Mã theo dõi 1 người trong nhiều frame

    CreatedAt DATETIME DEFAULT GETDATE()
    -- Thời điểm ghi nhận sự kiện
);

ALTER TABLE MovementLogs
ADD CONSTRAINT FK_MovementLogs_Cameras
FOREIGN KEY (CameraId) REFERENCES Cameras(Id);

ALTER TABLE MovementLogs
ADD CONSTRAINT FK_MovementLogs_Employees
FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);


-- BẢNG LOG CẢNH BÁO
CREATE TABLE SecurityAlerts (
    Id INT IDENTITY PRIMARY KEY,
    -- Khóa chính tự tăng

    CameraId INT NOT NULL,
    -- Camera phát sinh cảnh báo

    EmployeeId INT NULL,
    -- Nếu cảnh báo liên quan tới nhân viên cụ thể

    AlertType NVARCHAR(100),
    -- Loại cảnh báo:
    -- UnknownPerson
    -- BodyAnomaly
    -- MultiPerson
    -- FakeFace
    -- LivenessFail

    Description NVARCHAR(500),
    -- Mô tả chi tiết cảnh báo

    OccurrenceCount INT DEFAULT 1,
    -- Số lần lặp lại của sự kiện trong khoảng thời gian

    IsSentToDiscord BIT DEFAULT 0,
    -- Đã gửi cảnh báo sang Discord hay chưa (1: rồi)

    CreatedAt DATETIME DEFAULT GETDATE()
    -- Thời điểm tạo cảnh báo
);
ALTER TABLE SecurityAlerts
ADD CONSTRAINT FK_SecurityAlerts_Cameras
FOREIGN KEY (CameraId) REFERENCES Cameras(Id);

ALTER TABLE SecurityAlerts
ADD CONSTRAINT FK_SecurityAlerts_Employees
FOREIGN KEY (EmployeeId) REFERENCES Employees(Id);

CREATE INDEX IX_MovementLogs_CreatedAt
ON MovementLogs (CreatedAt DESC);
-- Tối ưu truy vấn theo thời gian mới nhất

CREATE INDEX IX_MovementLogs_PersonType
ON MovementLogs (PersonType);
-- Tối ưu lọc Known / Unknown

CREATE INDEX IX_SecurityAlerts_CreatedAt
ON SecurityAlerts (CreatedAt DESC);
-- Tối ưu truy vấn lịch sử cảnh báo

CREATE INDEX IX_SecurityAlerts_AlertType
ON SecurityAlerts (AlertType);
-- Tối ưu thống kê theo loại cảnh báo

