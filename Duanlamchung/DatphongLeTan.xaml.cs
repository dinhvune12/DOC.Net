using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Duanlamchung
{
    public partial class DatphongLeTan : Window
    {
        HotelManagerEntities db = new HotelManagerEntities();
        booking selectedBooking = null;

        decimal pricePerDay = 500000;

        public DatphongLeTan()
        {
            InitializeComponent();

            try
            {
                // Load and wire events safely; n?u control null, b? qua
                try { LoadRooms(); } catch (Exception ex) { MessageBox.Show("L?i khi load danh sách phòng: " + ex.ToString(), "L?i", MessageBoxButton.OK, MessageBoxImage.Error); }

                if (dpCheckIn != null)
                    dpCheckIn.SelectedDateChanged += (s, e) => CalculateTotalPrice();
                if (dpCheckOut != null)
                    dpCheckOut.SelectedDateChanged += (s, e) => CalculateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi kh?i t?o form d?t phòng: " + ex.ToString(), "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int selectedRoomId;

        public DatphongLeTan(int roomId)
        {
            InitializeComponent();

            selectedRoomId = roomId;

            try
            {
                try { LoadRooms(); } catch (Exception ex) { MessageBox.Show("L?i khi load danh sách phòng: " + ex.ToString(), "L?i", MessageBoxButton.OK, MessageBoxImage.Error); }

                if (dpCheckIn != null)
                    dpCheckIn.SelectedDateChanged += (s, e) => CalculateTotalPrice();
                if (dpCheckOut != null)
                    dpCheckOut.SelectedDateChanged += (s, e) => CalculateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi kh?i t?o form d?t phòng (with roomId): " + ex.ToString(), "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= LOAD ROOMS =================
        private void LoadRooms()
        {
            try
            {
                // L?y danh sách phòng t? DB
                var rooms = db.rooms.ToList();

                if (cbRoomNumber == null)
                    return;

                // N?u Items dã có ph?n t?, clear tru?c khi gán ItemsSource
                // (tránh l?i: "Items collection must be empty before using ItemsSource")
                if (cbRoomNumber.Items != null && cbRoomNumber.Items.Count > 0)
                    cbRoomNumber.Items.Clear();

                // Gán ItemsSource (an toàn)
                cbRoomNumber.ItemsSource = rooms;
                cbRoomNumber.DisplayMemberPath = "room_number";
                cbRoomNumber.SelectedValuePath = "id";

                // N?u form du?c kh?i t?o v?i roomId, ch?n giá tr? tuong ?ng
                if (selectedRoomId > 0)
                {
                    try
                    {
                        cbRoomNumber.SelectedValue = selectedRoomId;
                    }
                    catch
                    {
                        // ignore if selection fails
                    }
                }
            }
            catch (InvalidOperationException iex)
            {
                // Thêm thông báo rõ ràng cho developer debug
                MessageBox.Show("L?i khi gán ItemsSource cho cbRoomNumber: " + iex.Message,
                                "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                // fallback: clear items and retry once
                try
                {
                    cbRoomNumber.Items.Clear();
                    cbRoomNumber.ItemsSource = db.rooms.ToList();
                    cbRoomNumber.DisplayMemberPath = "room_number";
                    cbRoomNumber.SelectedValuePath = "id";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không th? load danh sách phòng: " + ex.ToString(),
                                    "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi load danh sách phòng: " + ex.ToString(),
                                "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================= CREATE =================
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            // Check for double-booking: phòng dã du?c d?t trong kho?ng th?i gian này?
            var roomId = (int)cbRoomNumber.SelectedValue;
            var checkInDate = dpCheckIn.SelectedDate.Value;
            var checkOutDate = dpCheckOut.SelectedDate.Value;

            var conflictingBookings = db.bookings
                .Where(b => b.room_id == roomId &&
                            b.status != "Cancelled" &&
                            b.check_in_date < checkOutDate &&
                            b.check_out_date > checkInDate)
                .ToList();

            if (conflictingBookings.Count > 0)
            {
                MessageBox.Show("Phòng này dã có booking trong kho?ng th?i gian này. Vui lòng ch?n phòng ho?c ngày khác.",
                                "L?i Double-Booking", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var customer = db.customers
                .FirstOrDefault(c => c.phone == txtPhone.Text);

            if (customer == null)
            {
                customer = new customer()
                {
                    full_name = txtFullName.Text,
                    phone = txtPhone.Text,
                    identity_card = txtIDPassport.Text
                };

                db.customers.Add(customer);
                db.SaveChanges();
            }

            string statusValue = cbStatus.SelectedItem != null
                         ? ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString()
                         : "Pending";

            booking newBooking = new booking()
            {
                customer_id = customer.id,
                room_id = roomId,
                check_in_date = checkInDate,
                check_out_date = checkOutDate,
                status = statusValue,
                created_at = DateTime.Now
            };

            db.bookings.Add(newBooking);

            var room = db.rooms.Find(newBooking.room_id);
            if (room != null)
                room.status = "Occupied";

            db.SaveChanges();

            // Notify other windows (danhsachdatphong, RoommapLeTan, Dashboard) to refresh
            try { AppEvents.RaiseBookingOrRoomChanged(); } catch { }

            MessageBox.Show("Booking created successfully!");
            ClearForm();
        }

        // ================= UPDATE =================
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Select booking first!");
                return;
            }

            // Re-fetch t? DB d? ensure nó du?c tracked
            var booking = db.bookings.Find(selectedBooking.id);
            if (booking == null)
            {
                MessageBox.Show("Booking not found in database!");
                return;
            }

            booking.check_in_date = dpCheckIn.SelectedDate.Value;
            booking.check_out_date = dpCheckOut.SelectedDate.Value;
            booking.status = ((ComboBoxItem)cbStatus.SelectedItem)?.Content.ToString();

            // c?p nh?t tr?ng thái phòng tuong ?ng khi thay d?i d?t phòng
            var room = db.rooms.Find(booking.room_id);
            if (room != null)
            {
                var statusValue = booking.status ?? "";
                if (statusValue.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                    room.status = "Available";
                else if (statusValue.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) || statusValue.Equals("Checked In", StringComparison.OrdinalIgnoreCase) || statusValue.Equals("CheckedIn", StringComparison.OrdinalIgnoreCase))
                    room.status = "Occupied";
            }

            db.SaveChanges();

            // Clear form and reload
            ClearForm();
            LoadRooms();

            // Notify other windows
            try { AppEvents.RaiseBookingOrRoomChanged(); } catch { }

            MessageBox.Show("Booking updated!");
        }

        // ================= CANCEL =================
        private void btnCancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Select booking first!");
                return;
            }

            // Re-fetch t? DB d? ensure nó du?c tracked
            var booking = db.bookings.Find(selectedBooking.id);
            if (booking == null)
            {
                MessageBox.Show("Booking not found in database!");
                return;
            }

            booking.status = "Cancelled";

            var room = db.rooms.Find(booking.room_id);
            if (room != null)
                room.status = "Available";

            db.SaveChanges();

            // Clear form and reload
            ClearForm();
            LoadRooms();

            // Notify other windows
            try { AppEvents.RaiseBookingOrRoomChanged(); } catch { }

            MessageBox.Show("Booking cancelled!");
        }

        // ================= CHECK-IN =================
        private void btnConfirmCheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Select booking first!");
                return;
            }

            // Re-fetch t? DB d? ensure nó du?c tracked
            var booking = db.bookings.Find(selectedBooking.id);
            if (booking == null)
            {
                MessageBox.Show("Booking not found in database!");
                return;
            }

            booking.status = "Checked In";

            var room = db.rooms.Find(booking.room_id);
            if (room != null)
                room.status = "Occupied";

            db.SaveChanges();

            // Clear form and reload
            ClearForm();
            LoadRooms();

            // Notify other windows
            try { AppEvents.RaiseBookingOrRoomChanged(); } catch { }

            MessageBox.Show("Check-in confirmed!");
        }

        // ================= AUTO CALCULATE =================
        private void CalculateTotalPrice()
        {
            if (dpCheckIn.SelectedDate != null &&
                dpCheckOut.SelectedDate != null)
            {
                int days = (dpCheckOut.SelectedDate.Value -
                            dpCheckIn.SelectedDate.Value).Days;

                if (days > 0)
                {
                    decimal total = days * pricePerDay;
                    txtTotalPrice.Text = total.ToString("N0");
                }
                else
                {
                    txtTotalPrice.Text = "0";
                }
            }
        }
        // ===== HANDLE XAML DataChanged EVENT =====
        private void DataChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateTotalPrice();
        }

        // ================= VALIDATE =================
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                cbRoomNumber.SelectedValue == null ||
                dpCheckIn.SelectedDate == null ||
                dpCheckOut.SelectedDate == null)
            {
                MessageBox.Show("Please fill all required fields!");
                return false;
            }

            if (dpCheckOut.SelectedDate <= dpCheckIn.SelectedDate)
            {
                MessageBox.Show("Check-out must be after Check-in!");
                return false;
            }

            return true;
        }

        // ================= CLEAR =================
        private void ClearForm()
        {
            txtFullName.Clear();
            txtPhone.Clear();
            txtIDPassport.Clear();
            txtNote.Clear();
            txtTotalPrice.Text = "0";

            dpCheckIn.SelectedDate = null;
            dpCheckOut.SelectedDate = null;

            cbRoomNumber.SelectedIndex = -1;
            cbRoomType.SelectedIndex = -1;
            cbStatus.SelectedIndex = -1;

            selectedBooking = null;
        }

        // ================= EMPTY EVENTS =================
        private void TxtFullName_TextChanged(object sender, TextChangedEventArgs e) { }
        private void TxtPhone_TextChanged(object sender, TextChangedEventArgs e) { }
        private void TxtIDPassport_TextChanged(object sender, TextChangedEventArgs e) { }
        private void TxtNote_TextChanged(object sender, TextChangedEventArgs e) { }
        private void CbRoomNumber_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void CbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void TxtTotalPrice_TextChanged(object sender, TextChangedEventArgs e) { }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // S? d?ng Nav.Back n?u dã cài Nav helper (gi? l?ch s?)
                Nav.Back(this);
            }
            catch
            {
                // Fallback: m? Dashboard m?i n?u không có l?ch s?
                try
                {
                    var dash = new DashboardLeTan();
                    dash.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Khong the quay lai: " + ex.Message, "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ================= WINDOW CONTROLS =================
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}