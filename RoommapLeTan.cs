csharp Duanlamchung\RoommapLeTan.xaml.cs
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Duanlamchung
{
    public partial class RoommapLeTan : Window
    {
        public RoommapLeTan()
        {
            InitializeComponent();
            Loaded += RoommapLeTan_Loaded;
        }

        private void RoommapLeTan_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshRoomMap();
        }

        private void RefreshRoomMap()
        {
            try
            {
                using (var db = new HotelManagerEntities())
                {
                    // l?y t?t c? rooms, key theo room_number
                    var rooms = db.rooms.ToList().ToDictionary(r => r.room_number, r => (r.status ?? "").Trim());

                    // helper ?? apply tr?ng thái lên UI element t??ng ?ng
                    void Apply(string roomNumber, TextBlock statusText, Border bd)
                    {
                        if (!rooms.TryGetValue(roomNumber, out var statusRaw))
                            return;

                        var s = (statusRaw ?? "").Trim().ToLowerInvariant();

                        switch (s)
                        {
                            case "available":
                                statusText.Text = "Available";
                                statusText.Foreground = Brushes.Green;
                                bd.Background = new SolidColorBrush(Color.FromRgb(200, 230, 201)); // greenish
                                break;
                            case "occupied":
                                statusText.Text = "Occupied";
                                statusText.Foreground = Brushes.Red;
                                bd.Background = new SolidColorBrush(Color.FromRgb(255, 205, 210)); // reddish
                                break;
                            case "cleaning":
                                statusText.Text = "Cleaning";
                                statusText.Foreground = Brushes.Orange;
                                bd.Background = new SolidColorBrush(Color.FromRgb(255, 249, 196)); // yellowish
                                break;
                            case "maintenance":
                                statusText.Text = "Maintenance";
                                statusText.Foreground = Brushes.Gray;
                                bd.Background = new SolidColorBrush(Color.FromRgb(224, 224, 224)); // gray
                                break;
                            default:
                                statusText.Text = statusRaw;
                                statusText.Foreground = Brushes.Gray;
                                bd.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                                break;
                        }
                    }

                    // Áp d?ng cho t?ng phòng hi?n có trong XAML
                    Apply("101", txtStatus101, bd101);
                    Apply("102", txtStatus102, bd102);
                    Apply("103", txtStatus103, bd103);
                    Apply("104", txtStatus104, bd104);
                    Apply("105", txtStatus105, bd105);
                    Apply("106", txtStatus106, bd106);
                    Apply("107", txtStatus107, bd107);
                    Apply("108", txtStatus108, bd108);
                    Apply("109", txtStatus109, bd109);
                    Apply("110", txtStatus110, bd110);
                    Apply("111", txtStatus111, bd111);
                    Apply("112", txtStatus112, bd112);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi t?i tr?ng thái phòng: " + ex.Message, "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // BookRoom_Click / UpdateRoom_Click / ViewRoom_Click gi? nguyên,
        // nh?ng sau khi thay ??i tr?ng thái (UpdateRoom_Click) b?n có th? g?i RefreshRoomMap()
        // thay vì t?o window m?i. Ví d? s?a ph?n cu?i UpdateRoom_Click nh? sau:

        private void UpdateRoom_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            int roomId = Convert.ToInt32(btn.Tag);

            using (var db = new HotelManagerEntities())
            {
                var room = db.rooms.FirstOrDefault(r => r.id == roomId);

                if (room == null)
                {
                    MessageBox.Show("Room not found!");
                    return;
                }

                var status = (room.status ?? "").Trim().ToLowerInvariant();
                if (status == "cleaning")
                    room.status = "Available";
                else if (status == "maintenance")
                    room.status = "Available";
                else
                    room.status = "Cleaning";

                db.SaveChanges();

                MessageBox.Show("C?p nh?t tr?ng thái thành công!");

                // Refresh UI (không c?n m? l?i window)
                RefreshRoomMap();
            }
        }
    }
}