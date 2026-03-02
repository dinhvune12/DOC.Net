using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



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
            LoadRooms();

            dpCheckIn.SelectedDateChanged += (s, e) => CalculateTotalPrice();
            dpCheckOut.SelectedDateChanged += (s, e) => CalculateTotalPrice();
        }

        private int selectedRoomId;

        public DatphongLeTan(int roomId)
        {
            InitializeComponent();
            selectedRoomId = roomId;
        }

        // ================= LOAD ROOMS =================
        private void LoadRooms()
        {
            var rooms = db.rooms.ToList();

            cbRoomNumber.ItemsSource = rooms;
            cbRoomNumber.DisplayMemberPath = "room_number";
            cbRoomNumber.SelectedValuePath = "id";
        }

        // ================= CREATE =================
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

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

            booking newBooking = new booking()
            {
                customer_id = customer.id,
                room_id = (int)cbRoomNumber.SelectedValue,
                check_in_date = dpCheckIn.SelectedDate.Value,
                check_out_date = dpCheckOut.SelectedDate.Value,
                status = cbStatus.SelectedItem != null
                         ? ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString()
                         : "Pending",
                created_at = DateTime.Now
            };

            db.bookings.Add(newBooking);

            var room = db.rooms.Find(newBooking.room_id);
            if (room != null)
                room.status = "Occupied";

            db.SaveChanges();

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

            selectedBooking.check_in_date = dpCheckIn.SelectedDate.Value;
            selectedBooking.check_out_date = dpCheckOut.SelectedDate.Value;
            selectedBooking.status =
                ((ComboBoxItem)cbStatus.SelectedItem)?.Content.ToString();

            db.SaveChanges();

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

            selectedBooking.status = "Cancelled";

            var room = db.rooms.Find(selectedBooking.room_id);
            if (room != null)
                room.status = "Available";

            db.SaveChanges();

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

            selectedBooking.status = "Checked In";

            var room = db.rooms.Find(selectedBooking.room_id);
            if (room != null)
                room.status = "Occupied";

            db.SaveChanges();

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
    }
}