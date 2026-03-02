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
                var list = (from b in db.bookings
                            join c in db.customers on b.customer_id equals c.id
                            join r in db.rooms on b.room_id equals r.id
                            orderby b.id descending
                            select new BookingListItem
                            {
                                BookingIdRaw = b.id,
                                MaBooking = b.id.ToString(),
                                TenKhach = c.full_name,
                                CCCD = c.identity_card,
                                SoPhong = r.room_number,
                                NgayCheckIn = b.check_in_date.ToString("dd/MM/yyyy"),
                                NgayCheckOut = b.check_out_date.ToString("dd/MM/yyyy"),
                                TrangThai = b.status,
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
            string q = (TxtSearch.Text ?? "").Trim().ToLower();
            string by = (CbSearchBy.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tên khách";

            using (var db = new HotelManagerEntities())
            {
                var query = from b in db.bookings
                            join c in db.customers on b.customer_id equals c.id
                            join r in db.rooms on b.room_id equals r.id
                            select new { b, c, r };

                if (!string.IsNullOrEmpty(q))
                {
                    if (by.Contains("Tên"))
                        query = query.Where(x => (x.c.full_name ?? "").ToLower().Contains(q));
                    else if (by.Contains("CCCD"))
                        query = query.Where(x => (x.c.identity_card ?? "").ToLower().Contains(q));
                    else if (by.Contains("Số phòng"))
                        query = query.Where(x => (x.r.room_number ?? "").ToLower().Contains(q));
                    else
                        query = query.Where(x => x.b.id.ToString().Contains(q));
                }

                var list = query.OrderByDescending(x => x.b.id)
                    .Select(x => new BookingListItem
                    {
                        BookingIdRaw = x.b.id,
                        MaBooking = x.b.id.ToString(),
                        TenKhach = x.c.full_name,
                        CCCD = x.c.identity_card,
                        SoPhong = x.r.room_number,
                        NgayCheckIn = x.b.check_in_date.ToString("dd/MM/yyyy"),
                        NgayCheckOut = x.b.check_out_date.ToString("dd/MM/yyyy"),
                        TrangThai = x.b.status,
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

                var list = query.OrderByDescending(x => x.b.id)
                    .Select(x => new BookingListItem
                    {
                        BookingIdRaw = x.b.id,
                        MaBooking = x.b.id.ToString(),
                        TenKhach = x.c.full_name,
                        CCCD = x.c.identity_card,
                        SoPhong = x.r.room_number,
                        NgayCheckIn = x.b.check_in_date.ToString("dd/MM/yyyy"),
                        NgayCheckOut = x.b.check_out_date.ToString("dd/MM/yyyy"),
                        TrangThai = x.b.status,
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
                db.SaveChanges();
            }

            LoadBookings();
            MessageBox.Show("Đã hủy booking!");
        }

        private void BtnOpenCheckIn_Click(object sender, RoutedEventArgs e)
        {
          
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