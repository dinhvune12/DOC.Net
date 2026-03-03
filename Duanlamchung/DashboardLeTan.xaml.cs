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

        private void LoadStatistics()
        {
            using (var db = new HotelManagerEntities())
            {
                txtAvailableRooms.Text = db.rooms.Count(r => r.status == "available").ToString();
                txtOccupiedRooms.Text = db.rooms.Count(r => r.status == "occupied").ToString();

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                txtTodayCheckIn.Text = db.bookings.Count(b => b.check_in_date >= today && b.check_in_date < tomorrow).ToString();
                txtTodayCheckOut.Text = db.bookings.Count(b => b.check_out_date >= today && b.check_out_date < tomorrow).ToString();
            }
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e) => LoadStatistics();

        private void btnRoomMap_Click(object sender, RoutedEventArgs e) => Nav.Go(this, new RoommapLeTan());

        private void btnBooking_Click(object sender, RoutedEventArgs e) => Nav.Go(this, new danhsachdatphong());

        private void btnCheckInOut_Click(object sender, RoutedEventArgs e) => Nav.Go(this, new CheckInOut());

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            Nav.Go(this, new DangKyDangnhapKhach());
        }

        private void btnRoom_Click(object sender, RoutedEventArgs e)
        {
            Thongtinphong frm = new Thongtinphong();
            frm.Show();
        }

        private void btnOpenRoomMap_Click(object sender, RoutedEventArgs e) => btnRoomMap_Click(sender, e);
        private void btnCreateBooking_Click(object sender, RoutedEventArgs e) => Nav.Go(this, new DatphongLeTan());
        private void btnQuickCheckInOut_Click(object sender, RoutedEventArgs e) => btnCheckInOut_Click(sender, e);
    }
}