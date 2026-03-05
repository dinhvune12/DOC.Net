using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace Duanlamchung
{
    public partial class danhsachdatphong : Window
    {
        private ObservableCollection<BookingRow> _all = new ObservableCollection<BookingRow>();
        private ObservableCollection<BookingRow> _view = new ObservableCollection<BookingRow>();

        public danhsachdatphong()
        {
            InitializeComponent();
            DgBookings.ItemsSource = _view;
            Loaded += Danhsachdatphong_Loaded;
        }

        private class BookingRow
        {
            public int Id { get; set; }
            public string MaBooking => Id.ToString();
            public string TenKhach { get; set; }
            public string CCCD { get; set; }
            public string SoPhong { get; set; }
            public DateTime NgayCheckInRaw { get; set; }
            public DateTime NgayCheckOutRaw { get; set; }
            public string NgayCheckIn => NgayCheckInRaw.ToString("dd/MM/yyyy");
            public string NgayCheckOut => NgayCheckOutRaw.ToString("dd/MM/yyyy");
            public string TrangThai { get; set; }
            public string TienCoc { get; set; }
        }

        private void Danhsachdatphong_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllBookings();
        }

        private void LoadAllBookings()
        {
            try
            {
                _all.Clear();
                _view.Clear();

                using (var db = new HotelManagerEntities())
                {
                    var bookings = db.bookings
                        .Include(b => b.customer)
                        .Include(b => b.room)
                        .Include("room.room_types")
                        .OrderByDescending(b => b.id)
                        .ToList();

                    foreach (var b in bookings)
                    {
                        int nights = (b.check_out_date - b.check_in_date).Days;
                        if (nights < 1) nights = 1;

                        decimal price = 0;
                        if (b.room?.room_types != null)
                            price = b.room.room_types.price_per_night * nights;

                        var row = new BookingRow
                        {
                            Id = b.id,
                            TenKhach = b.customer?.full_name ?? "",
                            CCCD = b.customer?.identity_card ?? "",
                            SoPhong = b.room?.room_number ?? "",
                            NgayCheckInRaw = b.check_in_date,
                            NgayCheckOutRaw = b.check_out_date,
                            TrangThai = b.status ?? "N/A",
                            TienCoc = price.ToString("N0") + " VND"
                        };

                        _all.Add(row);
                        _view.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi load: " + ex.Message, "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterData();
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;
            CbStatus.SelectedIndex = 0;
            LoadAllBookings();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            CbSearchBy.SelectedIndex = 0;
            LoadAllBookings();
        }

        private void FilterData()
        {
            try
            {
                var filtered = _all.AsEnumerable();

                if (DpFrom.SelectedDate.HasValue)
                {
                    filtered = filtered.Where(x => x.NgayCheckInRaw >= DpFrom.SelectedDate.Value);
                }

                if (DpTo.SelectedDate.HasValue)
                {
                    filtered = filtered.Where(x => x.NgayCheckOutRaw <= DpTo.SelectedDate.Value);
                }

                if (CbStatus.SelectedIndex > 0)
                {
                    string status = (CbStatus.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
                    if (!string.IsNullOrEmpty(status))
                    {
                        filtered = filtered.Where(x => x.TrangThai.Equals(status, StringComparison.OrdinalIgnoreCase));
                    }
                }

                _view.Clear();
                foreach (var row in filtered)
                {
                    _view.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi filter: " + ex.Message, "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchData()
        {
            try
            {
                string searchText = TxtSearch.Text.Trim();
                if (string.IsNullOrEmpty(searchText))
                {
                    LoadAllBookings();
                    return;
                }

                int searchBy = CbSearchBy.SelectedIndex;
                var filtered = _all.AsEnumerable();

                switch (searchBy)
                {
                    case 0: // Ten khach
                        filtered = filtered.Where(x => x.TenKhach.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                        break;
                    case 1: // CCCD
                        filtered = filtered.Where(x => x.CCCD.Contains(searchText));
                        break;
                    case 2: // So phong
                        filtered = filtered.Where(x => x.SoPhong.Contains(searchText));
                        break;
                    case 3: // Ma booking
                        if (int.TryParse(searchText, out int id))
                        {
                            filtered = filtered.Where(x => x.Id == id);
                        }
                        break;
                }

                _view.Clear();
                foreach (var row in filtered)
                {
                    _view.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi search: " + ex.Message, "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgBookings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Nav.Back(this);
            }
            catch
            {
                try
                {
                    var dash = new DashboardLeTan();
                    dash.Show();
                    this.Close();
                }
                catch { try { Close(); } catch { } }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAllBookings();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (DgBookings.SelectedItem is BookingRow item)
            {
                try
                {
                    using (var db = new HotelManagerEntities())
                    {
                        var booking = db.bookings.Find(item.Id);
                        if (booking != null && booking.status == "Pending")
                        {
                            booking.status = "Confirmed";
                            var room = db.rooms.Find(booking.room_id);
                            if (room != null) room.status = "Occupied";
                            db.SaveChanges();
                            LoadAllBookings();
                            MessageBox.Show("Da xac nhan!", "Thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Loi: " + ex.Message, "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Chon booking!", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (DgBookings.SelectedItem is BookingRow item)
            {
                var result = MessageBox.Show("Xac nhan huy?", "Xac nhan", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new HotelManagerEntities())
                        {
                            var booking = db.bookings.Find(item.Id);
                            if (booking != null)
                            {
                                booking.status = "Cancelled";
                                var room = db.rooms.Find(booking.room_id);
                                if (room != null) room.status = "Available";
                                db.SaveChanges();
                                LoadAllBookings();
                                MessageBox.Show("Da huy!", "Thanh cong", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Loi: " + ex.Message, "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Chon booking!", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnOpenCheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (DgBookings.SelectedItem is BookingRow item)
            {
                try
                {
                    var checkInOut = new CheckInOut();
                    checkInOut.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Loi: " + ex.Message, "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Chon booking!", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
