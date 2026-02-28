using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Duanlamchung
{
    public partial class CheckInOut : Window
    {
        // Demo model (để chạy được ngay). Sau bạn muốn nối DB thì thay list này.
        public class BookingRow
        {
            public string BookingId { get; set; }
            public string CustomerName { get; set; }
            public string RoomNo { get; set; }
            public string RoomType { get; set; }
            public string CheckInDate { get; set; }
            public string CheckOutDate { get; set; }
            public string Status { get; set; } // Pending/Confirmed/CheckedIn/CheckedOut
        }

        private ObservableCollection<BookingRow> _all = new ObservableCollection<BookingRow>();
        private ObservableCollection<BookingRow> _view = new ObservableCollection<BookingRow>();

        public CheckInOut()
        {
            InitializeComponent();

            // gán datasource
            dgBookings.ItemsSource = _view;

            // data demo
            _all.Add(new BookingRow { BookingId = "BK001", CustomerName = "Nguyễn Văn A", RoomNo = "101", RoomType = "Deluxe", CheckInDate = "28/02/2026", CheckOutDate = "01/03/2026", Status = "Pending" });
            _all.Add(new BookingRow { BookingId = "BK002", CustomerName = "Trần Thị B", RoomNo = "202", RoomType = "VIP", CheckInDate = "28/02/2026", CheckOutDate = "02/03/2026", Status = "CheckedIn" });
            _all.Add(new BookingRow { BookingId = "BK003", CustomerName = "Lê Văn C", RoomNo = "303", RoomType = "Standard", CheckInDate = "27/02/2026", CheckOutDate = "28/02/2026", Status = "CheckedOut" });

            ApplyFilter();
            ClearDetail();
        }

        // ====== EVENTS (đúng y chang tên trong XAML) ======

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

                // enable/disable nút theo trạng thái
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

            b.Status = "CheckedIn";
            ApplyFilter();
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

            // demo: cho check-out luôn (thực tế bạn sẽ check hóa đơn đã paid chưa)
            b.Status = "CheckedOut";
            ApplyFilter();
            SelectById(b.BookingId);
            MessageBox.Show("Check-out thành công!");
        }

        private void btnInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgBookings.SelectedItem is BookingRow b))
            {
                MessageBox.Show("Chọn 1 booking trước.");
                return;
            }

            // Mở form BillPay
            var w = new BillPay();
            w.ShowDialog();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ====== HELPERS ======

        private void ApplyFilter()
        {
            string q = (txtSearch.Text ?? "").Trim().ToLower();
            int filterIndex = cbFilter.SelectedIndex; // 0 all, 1 pending/confirmed, 2 checkedin, 3 checkedout

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
    }
}