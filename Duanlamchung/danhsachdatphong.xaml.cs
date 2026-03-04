using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Duanlamchung
{
    public partial class danhsachdatphong : Window
    {
        public danhsachdatphong()
        {
            InitializeComponent();
            SeedData.EnsureSeed();
            LoadBookings();
        }

        private void LoadBookings()
        {
            using (var db = new HotelManagerEntities())
            {
                var raw = (from b in db.bookings
                           join c in db.customers on b.customer_id equals c.id
                           join r in db.rooms on b.room_id equals r.id
                           orderby b.id descending
                           select new
                           {
                               b.id,
                               c.full_name,
                               c.identity_card,
                               r.room_number,
                               b.check_in_date,
                               b.check_out_date,
                               b.status
                           }).ToList();

                var list = raw.Select(x => new BookingListItem
                {
                    BookingIdRaw = x.id,
                    MaBooking = x.id.ToString(),
                    TenKhach = x.full_name,
                    CCCD = x.identity_card,
                    SoPhong = x.room_number,
                    NgayCheckIn = x.check_in_date.ToString("dd/MM/yyyy"),
                    NgayCheckOut = x.check_out_date.ToString("dd/MM/yyyy"),
                    TrangThai = x.status,
                    TienCoc = "0"
                }).ToList();

                DgBookings.ItemsSource = list;
                TxtCount.Text = $"({list.Count} dòng)";
            }
        }

   
        private BookingListItem GetSelected()
        {
            return DgBookings.SelectedItem as BookingListItem;
        }

       

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadBookings();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string q = (TxtSearch.Text ?? "").Trim();
            int byIndex = CbSearchBy.SelectedIndex;

            using (var db = new HotelManagerEntities())
            {
                var raw = (from b in db.bookings
                           join c in db.customers on b.customer_id equals c.id
                           join r in db.rooms on b.room_id equals r.id
                           orderby b.id descending
                           select new
                           {
                               b.id,
                               c.full_name,
                               c.identity_card,
                               r.room_number,
                               b.check_in_date,
                               b.check_out_date,
                               b.status
                           }).ToList();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    raw = raw.Where(x =>
                    {
                        var key = byIndex == 1 ? (x.identity_card ?? "")
                                : byIndex == 2 ? (x.room_number ?? "")
                                : byIndex == 3 ? x.id.ToString()
                                : (x.full_name ?? "");

                        return key.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                    }).ToList();
                }

                var list = raw.Select(x => new BookingListItem
                {
                    BookingIdRaw = x.id,
                    MaBooking = x.id.ToString(),
                    TenKhach = x.full_name,
                    CCCD = x.identity_card,
                    SoPhong = x.room_number,
                    NgayCheckIn = x.check_in_date.ToString("dd/MM/yyyy"),
                    NgayCheckOut = x.check_out_date.ToString("dd/MM/yyyy"),
                    TrangThai = x.status,
                    TienCoc = "0"
                }).ToList();

                DgBookings.ItemsSource = list;
                TxtCount.Text = $"({list.Count} dòng)";
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = "";
            LoadBookings();
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            DateTime? from = DpFrom.SelectedDate;
            DateTime? to = DpTo.SelectedDate;
            string status = (CbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tất cả";

            using (var db = new HotelManagerEntities())
            {
                var query = from b in db.bookings
                            join c in db.customers on b.customer_id equals c.id
                            join r in db.rooms on b.room_id equals r.id
                            select new { b, c, r };

                if (from.HasValue)
                    query = query.Where(x => x.b.check_in_date >= from.Value);

                if (to.HasValue)
                    query = query.Where(x => x.b.check_out_date <= to.Value.AddDays(1).AddSeconds(-1));

                if (status != "Tất cả")
                    query = query.Where(x => x.b.status == status);

                var raw = query.OrderByDescending(x => x.b.id)
                    .Select(x => new
                    {
                        x.b.id,
                        x.c.full_name,
                        x.c.identity_card,
                        x.r.room_number,
                        x.b.check_in_date,
                        x.b.check_out_date,
                        x.b.status
                    }).ToList();

                var list = raw.Select(x => new BookingListItem
                {
                    BookingIdRaw = x.id,
                    MaBooking = x.id.ToString(),
                    TenKhach = x.full_name,
                    CCCD = x.identity_card,
                    SoPhong = x.room_number,
                    NgayCheckIn = x.check_in_date.ToString("dd/MM/yyyy"),
                    NgayCheckOut = x.check_out_date.ToString("dd/MM/yyyy"),
                    TrangThai = x.status,
                    TienCoc = "0"
                }).ToList();

                DgBookings.ItemsSource = list;
                TxtCount.Text = $"({list.Count} dòng)";
            }
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;
            CbStatus.SelectedIndex = 0;
            LoadBookings();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var sel = GetSelected();
            if (sel == null)
            {
                MessageBox.Show("Chọn 1 booking trước.");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var b = db.bookings.FirstOrDefault(x => x.id == sel.BookingIdRaw);
                if (b == null) return;

               
                if (b.status != "Pending" && b.status != "pending")
                {
                    MessageBox.Show("Chỉ xác nhận khi booking đang Pending.");
                    return;
                }

                b.status = "Confirmed";

                var room = db.rooms.FirstOrDefault(x => x.id == b.room_id);
                if (room != null && (room.status == null || room.status == "available"))
                    room.status = "reserved";

                db.SaveChanges();
            }

            LoadBookings();
            MessageBox.Show("Đã xác nhận booking!");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var sel = GetSelected();
            if (sel == null)
            {
                MessageBox.Show("Chọn 1 booking trước.");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var b = db.bookings.FirstOrDefault(x => x.id == sel.BookingIdRaw);
                if (b == null) return;

           
                if (b.status == "CheckedIn" || b.status == "checked_in")
                {
                    MessageBox.Show("Booking đã check-in thì không được hủy.");
                    return;
                }

                b.status = "Cancelled";

                var room = db.rooms.FirstOrDefault(x => x.id == b.room_id);
                if (room != null)
                    room.status = "available";

                db.SaveChanges();
            }

            LoadBookings();
            MessageBox.Show("Đã hủy booking!");
        }

        private void BtnOpenCheckIn_Click(object sender, RoutedEventArgs e)
        {
            var sel = GetSelected();
            if (sel != null)
            {
                using (var db = new HotelManagerEntities())
                {
                    var b = db.bookings.FirstOrDefault(x => x.id == sel.BookingIdRaw);
                    if (b != null && b.status != "Confirmed" && b.status != "Pending")
                    {
                        MessageBox.Show("Chỉ mở check-in cho booking Pending/Confirmed.");
                        return;
                    }
                }
            }

            Nav.Go(this, new CheckInOut());
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export: demo (nếu bạn muốn xuất Excel, mình sẽ làm tiếp).");
        }
        private void DgBookings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Nếu chưa cần xử lý gì thì để trống cũng được
        }
    }
}