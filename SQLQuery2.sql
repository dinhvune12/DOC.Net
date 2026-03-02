USE HotelManager;
GO

INSERT INTO dbo.room_types(name, price_per_night, capacity)
VALUES (N'Standard', 300000, 2),
       (N'Deluxe',   500000, 2),
       (N'VIP',      900000, 4);

INSERT INTO dbo.rooms(room_number, room_type_id, status)
VALUES (N'101', 2, N'available'),
       (N'102', 1, N'available');

INSERT INTO dbo.services(name, price, is_active)
VALUES (N'Nước suối', 10000, 1),
       (N'Giặt ủi', 50000, 1);

INSERT INTO dbo.[users](username, [password], full_name, [role], phone)
VALUES (N'letan', N'123456', N'Lễ tân', N'staff', N'0900000000');

INSERT INTO dbo.customers(full_name, identity_card, phone, email, address)
VALUES (N'Nguyễn Văn A', N'0123456789', N'0911111111', N'a@gmail.com', N'Tam Kỳ');

INSERT INTO dbo.bookings(customer_id, room_id, user_id, check_in_date, check_out_date, status)
VALUES (1, 1, 1, GETDATE(), DATEADD(DAY, 1, GETDATE()), N'confirmed');
GO