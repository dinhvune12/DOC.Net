using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Duanlamchung
{
    public partial class CheckInOut : Window
    {
        public class BookingRow
        {
            public string BookingId { get; set; }
            public string CustomerName { get; set; }
            public string RoomNo { get; set; }
            public string RoomType { get; set; }
            public string CheckInDate { get; set; }
            public string CheckOutDate { get; set; }
            public string Status { get; set; }
        }

        private ObservableCollection<BookingRow> _all = new ObservableCollection<BookingRow>();
        private ObservableCollection<BookingRow> _view = new ObservableCollection<BookingRow>();

        public CheckInOut()
        {
            InitializeComponent();

            dgBookings.ItemsSource = _view;

            SeedData.EnsureSeed();
            LoadBookingsFromEntity();
            ClearDetail();
        }

        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBookings.SelectedItem is BookingRow b)
            {
                txtBookingId.Text = b.BookingId;
                txtCustomer.Text = b.CustomerName;
                txtRoom.Text = $"{b.RoomNo} - {b.RoomType}";
                txtDateRange.Text = $"{b.CheckInDate} → {b.CheckOutDate}";
                txtStatus.Text = b.Status;

                btnCheckIn.IsEnabled = (b.Status == "Pending" || b.Status == "Confirmed");
                btnCheckOut.IsEnabled = (b.Status == "CheckedIn");
                btnInvoice.IsEnabled = true;
            }
            else
            {
                ClearDetail();
            }
        }

        private void btnCheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgBookings.SelectedItem is BookingRow b))
            {
                MessageBox.Show("Chọn 1 booking trước.");
                return;
            }

            if (b.Status != "Pending" && b.Status != "Confirmed")
            {
                MessageBox.Show("Booking này không ở trạng thái chờ check-in.");
                return;
            }

            if (!int.TryParse(b.BookingId, out int bookingId))
            {
                MessageBox.Show("Mã booking không hợp lệ.");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var booking = db.bookings.FirstOrDefault(x => x.id == bookingId);
                if (booking == null)
                {
                    MessageBox.Show("Không tìm thấy booking trong CSDL.");
                    return;
                }

                if (booking.status != "Pending" && booking.status != "Confirmed")
                {
                    MessageBox.Show("Booking không ở trạng thái chờ check-in.");
                    return;
                }

                booking.status = "CheckedIn";

                var room = db.rooms.FirstOrDefault(x => x.id == booking.room_id);
                if (room != null)
                    room.status = "occupied";

                db.SaveChanges();
            }

            LoadBookingsFromEntity();
            SelectById(b.BookingId);
            MessageBox.Show("Check-in thành công!");
        }

        private void btnCheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgBookings.SelectedItem is BookingRow b))
            {
                MessageBox.Show("Chọn 1 booking trước.");
                return;
            }

            if (b.Status != "CheckedIn")
            {
                MessageBox.Show("Chỉ check-out khi booking đang ở trạng thái CheckedIn.");
                return;
            }

            if (!int.TryParse(b.BookingId, out int bookingId))
            {
                MessageBox.Show("Mã booking không hợp lệ.");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var booking = db.bookings.FirstOrDefault(x => x.id == bookingId);
                if (booking == null)
                {
                    MessageBox.Show("Không tìm thấy booking trong CSDL.");
                    return;
                }

                if (booking.status != "CheckedIn")
                {
                    MessageBox.Show("Booking không ở trạng thái CheckedIn.");
                    return;
                }

                booking.status = "CheckedOut";

                var room = db.rooms.FirstOrDefault(x => x.id == booking.room_id);
                if (room != null)
                    room.status = "available";

                db.SaveChanges();
            }

            LoadBookingsFromEntity();
            SelectById(b.BookingId);
            MessageBox.Show("Check-out thành công!");
        }

        private void btnInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgBookings.SelectedItem is BookingRow))
            {
                MessageBox.Show("Chọn 1 booking trước.");
                return;
            }

            var w = new BillPay();
            w.ShowDialog();
        }

        // Back/Close: nếu có history thì quay lại (Nav.Back), nếu không thì đóng như trước
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Nav.Back(this);
            }
            catch
            {
                try { Close(); } catch { /* best-effort */ }
            }
        }

        private void ApplyFilter()
        {
            if (txtSearch == null || cbFilter == null) return;

            string q = (txtSearch.Text ?? "").Trim().ToLower();
            int filterIndex = cbFilter.SelectedIndex;

            var data = _all.AsEnumerable();

            if (filterIndex == 1) data = data.Where(x => x.Status == "Pending" || x.Status == "Confirmed");
            if (filterIndex == 2) data = data.Where(x => x.Status == "CheckedIn");
            if (filterIndex == 3) data = data.Where(x => x.Status == "CheckedOut");

            if (!string.IsNullOrEmpty(q))
            {
                data = data.Where(x =>
                    (x.BookingId ?? "").ToLower().Contains(q) ||
                    (x.CustomerName ?? "").ToLower().Contains(q) ||
                    (x.RoomNo ?? "").ToLower().Contains(q));
            }

            _view.Clear();
            foreach (var item in data) _view.Add(item);
        }

        private void SelectById(string id)
        {
            var found = _view.FirstOrDefault(x => x.BookingId == id);
            if (found != null)
            {
                dgBookings.SelectedItem = found;
                dgBookings.ScrollIntoView(found);
            }
        }

        private void ClearDetail()
        {
            txtBookingId.Text = "";
            txtCustomer.Text = "";
            txtRoom.Text = "";
            txtDateRange.Text = "";
            txtStatus.Text = "";

            btnCheckIn.IsEnabled = false;
            btnCheckOut.IsEnabled = false;
            btnInvoice.IsEnabled = false;
        }

        private void LoadBookingsFromEntity()
        {
            using (var db = new HotelManagerEntities())
            {
                var raw = (from b in db.bookings
                           join c in db.customers on b.customer_id equals c.id
                           join r in db.rooms on b.room_id equals r.id
                           join rt in db.room_types on r.room_type_id equals rt.id
                           orderby b.id descending
                           select new
                           {
                               b.id,
                               c.full_name,
                               r.room_number,
                               RoomTypeName = rt.name,
                               b.check_in_date,
                               b.check_out_date,
                               b.status
                           }).ToList();

                var list = raw.Select(x => new BookingRow
                {
                    BookingId = x.id.ToString(),
                    CustomerName = x.full_name,
                    RoomNo = x.room_number,
                    RoomType = x.RoomTypeName,
                    CheckInDate = x.check_in_date.ToString("dd/MM/yyyy"),
                    CheckOutDate = x.check_out_date.ToString("dd/MM/yyyy"),
                    Status = x.status
                }).ToList();

                _all.Clear();
                foreach (var x in list) _all.Add(x);
            }

            ApplyFilter();
        }
    }
}
