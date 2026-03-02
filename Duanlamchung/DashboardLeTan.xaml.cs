using System;
using System.Linq;
using System.Windows;

namespace Duanlamchung
{
    public partial class DashboardLeTan : Window
    {
        public DashboardLeTan()
        {
            InitializeComponent();

            if (!Session.IsStaff)
            {
                MessageBox.Show("Bạn phải đăng nhập nhân viên trước!");
                Nav.Go(this, new DangKyDangnhapKhach());
                return;
            }

            LoadStatistics();
        }

        // ================= LOAD THỐNG KÊ =================
        private void LoadStatistics()
        {
            using (var db = new HotelManagerEntities())
            {
                txtAvailableRooms.Text = db.rooms
                    .Count(r => r.status == "Available")
                    .ToString();

                txtOccupiedRooms.Text = db.rooms
                    .Count(r => r.status == "Occupied")
                    .ToString();

                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                txtTodayCheckIn.Text = db.bookings
                    .Count(b => b.check_in_date >= today &&
                                b.check_in_date < tomorrow &&
                                b.status != "Cancelled")
                    .ToString();

                txtTodayCheckOut.Text = db.bookings
                    .Count(b => b.check_out_date >= today &&
                                b.check_out_date < tomorrow &&
                                b.status != "Cancelled")
                    .ToString();
            }
        }

        // ================= DASHBOARD =================
        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            MessageBox.Show("Dashboard refreshed!");
        }

        // ================= ROOM MAP =================
        private void btnRoomMap_Click(object sender, RoutedEventArgs e)
        {
            Nav.Go(this, new RoommapLeTan());
        }

        // ================= BOOKING LIST =================
        private void btnBooking_Click(object sender, RoutedEventArgs e)
        {
            Nav.Go(this, new DatphongLeTan());
        }

        // ================= CHECK IN / OUT =================
        private void btnCheckInOut_Click(object sender, RoutedEventArgs e)
        {
            Nav.Go(this, new CheckInOut());
        }

        // ================= LOGOUT =================
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Session.Logout();
                Nav.Go(this, new DangKyDangnhapKhach());
            }
        }

        // ================= QUICK BUTTONS =================
        private void btnOpenRoomMap_Click(object sender, RoutedEventArgs e)
        {
            RoommapLeTan roommap = new RoommapLeTan();
            roommap.Show();
            this.Close();   // nếu muốn đóng Dashboard
        }

        private void btnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            DatphongLeTan booking = new DatphongLeTan();
            booking.Show();
            this.Close();
        }

        private void btnQuickCheckInOut_Click(object sender, RoutedEventArgs e)
        {
            CheckInOut check = new CheckInOut();
            check.Show();
            this.Close();
        }

        // ================= AUTO REFRESH KHI QUAY LẠI =================
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            LoadStatistics();
        }
    }
}