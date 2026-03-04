using System;
using System.Linq;

namespace Duanlamchung
{
    public static class SeedData
    {
        public static void EnsureSeed()
        {
            using (var db = new HotelManagerEntities())
            {
                // 1) room_types
                if (!db.room_types.Any())
                {
                    db.room_types.Add(new room_types { name = "Standard", price_per_night = 300000, capacity = 2 });
                    db.room_types.Add(new room_types { name = "Deluxe", price_per_night = 500000, capacity = 2 });
                    db.room_types.Add(new room_types { name = "VIP", price_per_night = 900000, capacity = 4 });
                    db.SaveChanges();
                }

                var standard = db.room_types.First(x => x.name == "Standard");
                var deluxe = db.room_types.First(x => x.name == "Deluxe");
                var vip = db.room_types.First(x => x.name == "VIP");

                // 2) rooms (đảm bảo tối thiểu 12 phòng)
                if (!db.rooms.Any())
                {
                    db.rooms.Add(new room { room_number = "101", room_type_id = deluxe.id, status = "available", description = "Demo" });
                    db.rooms.Add(new room { room_number = "102", room_type_id = standard.id, status = "available", description = "Demo" });
                    db.SaveChanges();
                }

                for (int i = 101; i <= 112; i++)
                {
                    string roomNo = i.ToString();
                    if (!db.rooms.Any(x => x.room_number == roomNo))
                    {
                        int typeId = (i % 3 == 0) ? vip.id : (i % 2 == 0 ? deluxe.id : standard.id);
                        db.rooms.Add(new room
                        {
                            room_number = roomNo,
                            room_type_id = typeId,
                            status = "available",
                            description = "Phòng mẫu"
                        });
                    }
                }
                db.SaveChanges();

                // 3) customers (đảm bảo tối thiểu 10 khách)
                if (!db.customers.Any())
                {
                    db.customers.Add(new customer
                    {
                        full_name = "Nguyễn Văn A",
                        identity_card = "0123456789",
                        phone = "0911111111",
                        email = "a@gmail.com",
                        address = "Tam Kỳ"
                    });
                    db.SaveChanges();
                }

                for (int i = 1; i <= 10; i++)
                {
                    string cccd = "09999999" + i.ToString("00");
                    if (!db.customers.Any(x => x.identity_card == cccd))
                    {
                        db.customers.Add(new customer
                        {
                            full_name = "Khách mẫu " + i,
                            identity_card = cccd,
                            phone = "0900000" + i.ToString("000"),
                            email = "khach" + i + "@mail.com",
                            address = "Địa chỉ mẫu " + i
                        });
                    }
                }
                db.SaveChanges();

                // 4) users (nhân viên)
                if (!db.users.Any(u => u.username == "letan"))
                {
                    db.users.Add(new user
                    {
                        username = "letan",
                        password = "123456",
                        full_name = "Lễ tân",
                        role = "staff",
                        phone = "0900000000",
                        created_at = DateTime.Now
                    });
                    db.SaveChanges();
                }

                // 5) bookings mẫu (đảm bảo tối thiểu 10 booking)
                int existingBookings = db.bookings.Count();
                if (existingBookings < 10)
                {
                    var u = db.users.First(x => x.username == "letan");
                    var customers = db.customers.OrderBy(x => x.id).Take(20).ToList();
                    var rooms = db.rooms.OrderBy(x => x.id).Take(20).ToList();

                    string[] statuses = { "Pending", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };
                    int need = 10 - existingBookings;

                    for (int i = 0; i < need; i++)
                    {
                        var customer = customers[(existingBookings + i) % customers.Count];
                        var room = rooms[(existingBookings + i) % rooms.Count];
                        string status = statuses[(existingBookings + i) % statuses.Length];

                        var checkIn = DateTime.Today.AddDays(-((existingBookings + i) % 3));
                        var checkOut = checkIn.AddDays(1 + ((existingBookings + i) % 3));

                        db.bookings.Add(new booking
                        {
                            customer_id = customer.id,
                            room_id = room.id,
                            user_id = u.id,
                            check_in_date = checkIn,
                            check_out_date = checkOut,
                            status = status,
                            created_at = DateTime.Now.AddMinutes(-(existingBookings + i))
                        });

                        if (status == "CheckedIn") room.status = "occupied";
                        else if (status == "Pending" || status == "Confirmed") room.status = "reserved";
                        else room.status = "available";
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}
