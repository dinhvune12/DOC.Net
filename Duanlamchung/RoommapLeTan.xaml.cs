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
    public partial class RoommapLeTan : Window
    {
        public RoommapLeTan()
        {
            InitializeComponent();
        }

        // ================= BOOK ROOM =================
        private void BookRoom_Click(object sender, RoutedEventArgs e)
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

                if (room.status != "Available")
                {
                    MessageBox.Show("Phòng không khả dụng!");
                    return;
                }

                // Mở form đặt phòng
                DatphongLeTan booking = new DatphongLeTan(roomId);
                booking.Show();
                this.Close();
            }
        }

        // ================= VIEW ROOM =================
        private void ViewRoom_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            int roomId = Convert.ToInt32(btn.Tag);

            using (var db = new HotelManagerEntities())
            {
                var room = db.rooms.FirstOrDefault(r => r.id == roomId);

                if (room != null)
                {
                    MessageBox.Show(
                        "Room: " + room.room_number +
                        "\nStatus: " + room.status +
                        "\nPrice: " + room.room_types.price_per_night + " VND",
                        "Room Information",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        // ================= UPDATE ROOM STATUS =================
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

                // Toggle trạng thái ví dụ đơn giản
                if (room.status == "Cleaning")
                    room.status = "Available";
                else if (room.status == "Maintenance")
                    room.status = "Available";
                else
                    room.status = "Cleaning";

                db.SaveChanges();

                MessageBox.Show("Cập nhật trạng thái thành công!");

                // Refresh lại roommap
                RoommapLeTan newMap = new RoommapLeTan();
                newMap.Show();
                this.Close();
            }
        }
    }
}
