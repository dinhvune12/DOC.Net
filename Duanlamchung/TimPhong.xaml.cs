using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Duanlamchung
{
    /// <summary>
    /// Interaction logic for TimPhong.xaml
    /// </summary>
    public partial class TimPhong : Window
    {
        private DateTime? _checkIn;
        private DateTime? _checkOut;

        public TimPhong()
        {
            InitializeComponent();
            Loaded += TimPhong_Loaded;
        }

        private class RoomTypeComboItem
        {
            public int? Id { get; set; }
            public string Name { get; set; }
        }

        private class RoomCardItem
        {
            public int RoomId { get; set; }
            public string RoomName { get; set; }
            public string RoomDescription { get; set; }
            public string PriceText { get; set; }
        }

        private void TimPhong_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DpCheckIn.SelectedDate = DateTime.Today;
                DpCheckOut.SelectedDate = DateTime.Today.AddDays(1);

                using (var db = new HotelManagerEntities())
                {
                    var roomTypes = db.room_types
                        .Select(t => new RoomTypeComboItem
                        {
                            Id = t.id,
                            Name = t.name
                        })
                        .ToList();

                    roomTypes.Insert(0, new RoomTypeComboItem
                    {
                        Id = null,
                        Name = "Tất cả các loại"
                    });

                    CbRoomType.ItemsSource = roomTypes;
                    CbRoomType.DisplayMemberPath = "Name";
                    CbRoomType.SelectedValuePath = "Id";
                    CbRoomType.SelectedIndex = 0;
                }

                LoadAvailableRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu tìm phòng: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAvailableRooms()
        {
            try
            {
                _checkIn = DpCheckIn.SelectedDate;
                _checkOut = DpCheckOut.SelectedDate;

                if (_checkIn == null || _checkOut == null)
                {
                    RoomList.ItemsSource = null;
                    return;
                }

                if (_checkOut <= _checkIn)
                {
                    MessageBox.Show("Ngày trả phòng phải lớn hơn ngày nhận phòng.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new HotelManagerEntities())
                {
                    var inDate = _checkIn.Value;
                    var outDate = _checkOut.Value;

                    int? roomTypeId = null;
                    var selectedType = CbRoomType.SelectedItem as RoomTypeComboItem;
                    if (selectedType != null)
                        roomTypeId = selectedType.Id;

                    var bookedRoomIds = db.bookings
                        .Where(b => b.status != "cancelled"
                                    && b.check_in_date < outDate
                                    && b.check_out_date > inDate)
                        .Select(b => b.room_id)
                        .Distinct()
                        .ToList();

                    var roomsQuery = db.rooms.Where(r => !bookedRoomIds.Contains(r.id));

                    if (roomTypeId.HasValue)
                        roomsQuery = roomsQuery.Where(r => r.room_type_id == roomTypeId.Value);

                    var rooms = roomsQuery
                        .ToList()
                        .Select(r => new RoomCardItem
                        {
                            RoomId = r.id,
                            RoomName = "Phòng " + r.room_number,
                            RoomDescription = string.IsNullOrWhiteSpace(r.description)
                                ? (r.room_types != null
                                    ? "Loại phòng: " + r.room_types.name
                                    : "Không có mô tả")
                                : r.description,
                            PriceText = ((r.room_types != null ? r.room_types.price_per_night : 0)).ToString("N0") + "đ"
                        })
                        .ToList();

                    RoomList.ItemsSource = rooms;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm phòng: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadAvailableRooms();
        }

        private void BtnBookNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_checkIn == null || _checkOut == null)
                {
                    MessageBox.Show("Vui lòng chọn ngày nhận/trả phòng trước.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var btn = sender as Button;
                if (btn == null || btn.Tag == null)
                    return;

                int roomId = Convert.ToInt32(btn.Tag);

                var datPhong = new DatPhong(roomId, _checkIn.Value, _checkOut.Value);
                datPhong.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở form đặt phòng: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}