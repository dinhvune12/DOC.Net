USE HotelManager;
GO

-- Nếu đã lỡ tạo bảng trước đó thì drop theo thứ tự FK
IF OBJECT_ID('dbo.service_orders','U') IS NOT NULL DROP TABLE dbo.service_orders;
IF OBJECT_ID('dbo.invoices','U') IS NOT NULL DROP TABLE dbo.invoices;
IF OBJECT_ID('dbo.bookings','U') IS NOT NULL DROP TABLE dbo.bookings;
IF OBJECT_ID('dbo.rooms','U') IS NOT NULL DROP TABLE dbo.rooms;
IF OBJECT_ID('dbo.services','U') IS NOT NULL DROP TABLE dbo.services;
IF OBJECT_ID('dbo.customers','U') IS NOT NULL DROP TABLE dbo.customers;
IF OBJECT_ID('dbo.[users]','U') IS NOT NULL DROP TABLE dbo.[users];
IF OBJECT_ID('dbo.room_types','U') IS NOT NULL DROP TABLE dbo.room_types;
GO

CREATE TABLE dbo.room_types (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    price_per_night DECIMAL(12,2) NOT NULL DEFAULT(0),
    capacity INT NOT NULL DEFAULT(1)
);
GO

CREATE TABLE dbo.rooms (
    id INT IDENTITY(1,1) PRIMARY KEY,
    room_number NVARCHAR(20) NOT NULL,
    room_type_id INT NOT NULL,
    status NVARCHAR(30) NOT NULL DEFAULT(N'available'),
    description NVARCHAR(MAX) NULL,
    CONSTRAINT UQ_rooms_room_number UNIQUE (room_number),
    CONSTRAINT FK_rooms_room_types FOREIGN KEY(room_type_id) REFERENCES dbo.room_types(id)
);
GO

CREATE TABLE dbo.services (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(150) NOT NULL,
    price DECIMAL(12,2) NOT NULL DEFAULT(0),
    is_active BIT NOT NULL DEFAULT(1)
);
GO

CREATE TABLE dbo.[users] (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(80) NOT NULL,
    [password] NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(150) NULL,
    [role] NVARCHAR(50) NOT NULL DEFAULT(N'staff'),
    phone NVARCHAR(20) NULL,
    created_at DATETIME NOT NULL DEFAULT(GETDATE()),
    CONSTRAINT UQ_users_username UNIQUE (username)
);
GO

CREATE TABLE dbo.customers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    full_name NVARCHAR(150) NOT NULL,
    identity_card NVARCHAR(30) NULL,
    phone NVARCHAR(20) NULL,
    email NVARCHAR(150) NULL,
    address NVARCHAR(MAX) NULL
);
GO

CREATE TABLE dbo.bookings (
    id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    room_id INT NOT NULL,
    user_id INT NULL,
    check_in_date DATETIME NOT NULL,
    check_out_date DATETIME NOT NULL,
    status NVARCHAR(30) NOT NULL DEFAULT(N'pending'),
    created_at DATETIME NOT NULL DEFAULT(GETDATE()),

    CONSTRAINT FK_bookings_customers FOREIGN KEY(customer_id) REFERENCES dbo.customers(id),
    CONSTRAINT FK_bookings_rooms FOREIGN KEY(room_id) REFERENCES dbo.rooms(id),
    CONSTRAINT FK_bookings_users FOREIGN KEY(user_id) REFERENCES dbo.[users](id)
);
GO

CREATE TABLE dbo.service_orders (
    id INT IDENTITY(1,1) PRIMARY KEY,
    booking_id INT NOT NULL,
    service_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT(1),
    order_time DATETIME NOT NULL DEFAULT(GETDATE()),

    CONSTRAINT FK_service_orders_bookings FOREIGN KEY(booking_id) REFERENCES dbo.bookings(id) ON DELETE CASCADE,
    CONSTRAINT FK_service_orders_services FOREIGN KEY(service_id) REFERENCES dbo.services(id)
);
GO

CREATE TABLE dbo.invoices (
    id INT IDENTITY(1,1) PRIMARY KEY,
    booking_id INT NOT NULL,
    total_room_price DECIMAL(12,2) NOT NULL DEFAULT(0),
    total_service_price DECIMAL(12,2) NOT NULL DEFAULT(0),
    grand_total DECIMAL(12,2) NOT NULL DEFAULT(0),
    payment_status NVARCHAR(30) NOT NULL DEFAULT(N'unpaid'),
    created_at DATETIME NOT NULL DEFAULT(GETDATE()),

    CONSTRAINT UQ_invoices_booking UNIQUE (booking_id),
    CONSTRAINT FK_invoices_bookings FOREIGN KEY(booking_id) REFERENCES dbo.bookings(id) ON DELETE CASCADE
);
GO