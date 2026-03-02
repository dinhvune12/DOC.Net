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

                // 2) rooms
                if (!db.rooms.Any())
                {
                    var deluxe = db.room_types.First(x => x.name == "Deluxe");
                    var standard = db.room_types.First(x => x.name == "Standard");

                    db.rooms.Add(new room { room_number = "101", room_type_id = deluxe.id, status = "available", description = "Demo" });
                    db.rooms.Add(new room { room_number = "102", room_type_id = standard.id, status = "available", description = "Demo" });
                    db.SaveChanges();
                }

                // 3) customers
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

                // 5) bookings (tạo 1 booking demo)
                if (!db.bookings.Any())
                {
                    var c = db.customers.First();
                    var r = db.rooms.First();
                    var u = db.users.First(x => x.username == "letan");

                    db.bookings.Add(new booking
                    {
                        customer_id = c.id,
                        room_id = r.id,
                        user_id = u.id,
                        check_in_date = DateTime.Today,
                        check_out_date = DateTime.Today.AddDays(1),
                        status = "Pending",
                        created_at = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }
        }
    }
}