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

        private void LoadRooms()
        {
            var rooms = db.rooms.ToList();
            cbRoomNumber.ItemsSource = rooms;
            cbRoomNumber.DisplayMemberPath = "room_number";
            cbRoomNumber.SelectedValuePath = "id";
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var customer = db.customers.FirstOrDefault(c => c.phone == txtPhone.Text);

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

            // canonicalize status
            string statusValue = RoomStatus.Pending;
            if (cbStatus.SelectedItem != null)
            {
                statusValue = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString()
                                  .Trim().ToLowerInvariant();
                if (statusValue == "pending") statusValue = RoomStatus.Pending;
                else if (statusValue == "confirmed") statusValue = RoomStatus.Confirmed;
                else if (statusValue == "cancelled" || statusValue == "cancel") statusValue = RoomStatus.Cancelled;
            }

            booking newBooking = new booking()
            {
                customer_id = customer.id,
                room_id = (int)cbRoomNumber.SelectedValue,
                check_in_date = dpCheckIn.SelectedDate.Value,
                check_out_date = dpCheckOut.SelectedDate.Value,
                status = statusValue,
                created_at = DateTime.Now
            };

            db.bookings.Add(newBooking);

            var room = db.rooms.Find(newBooking.room_id);
            if (room != null)
                room.status = RoomStatus.Occupied; // use canonical value

            db.SaveChanges();

            // notify other windows (RoomMap, Dashboard, ...)
            AppEvents.RaiseBookingOrRoomChanged();

            MessageBox.Show("Booking created successfully!");
            ClearForm();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Select booking first!");
                return;
            }

            selectedBooking.check_in_date = dpCheckIn.SelectedDate.Value;
            selectedBooking.check_out_date = dpCheckOut.SelectedDate.Value;

            string statusValue = null;
            if (cbStatus.SelectedItem != null)
            {
                statusValue = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString()
                                  .Trim().ToLowerInvariant();
                if (statusValue == "pending") statusValue = RoomStatus.Pending;
                else if (statusValue == "confirmed") statusValue = RoomStatus.Confirmed;
                else if (statusValue == "cancelled" || statusValue == "cancel") statusValue = RoomStatus.Cancelled;
            }

            if (!string.IsNullOrEmpty(statusValue))
            {
                selectedBooking.status = statusValue;

                var room = db.rooms.Find(selectedBooking.room_id);
                if (room != null)
                {
                    if (statusValue == RoomStatus.Cancelled)
                        room.status = RoomStatus.Available;
                    else if (statusValue == RoomStatus.Confirmed || statusValue == RoomStatus.CheckedIn || statusValue == RoomStatus.Occupied)
                        room.status = RoomStatus.Occupied;
                }
            }

            db.SaveChanges();

            // notify
            AppEvents.RaiseBookingOrRoomChanged();

            MessageBox.Show("Booking updated!");
        }

        private void btnCancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Select booking first!");
                return;
            }

            selectedBooking.status = RoomStatus.Cancelled;

            var room = db.rooms.Find(selectedBooking.room_id);
            if (room != null)
                room.status = RoomStatus.Available;

            db.SaveChanges();

            AppEvents.RaiseBookingOrRoomChanged();

            MessageBox.Show("Booking cancelled!");
        }

        private void btnConfirmCheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBooking == null)
            {
                MessageBox.Show("Select booking first!");
                return;
            }

            selectedBooking.status = RoomStatus.CheckedIn;

            var room = db.rooms.Find(selectedBooking.room_id);
            if (room != null)
                room.status = RoomStatus.Occupied;

            db.SaveChanges();

            AppEvents.RaiseBookingOrRoomChanged();

            MessageBox.Show("Check-in confirmed!");
        }

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
    }