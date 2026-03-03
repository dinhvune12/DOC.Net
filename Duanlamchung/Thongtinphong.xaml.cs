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
using System.Collections.ObjectModel;

namespace Duanlamchung
{
    public partial class Thongtinphong : Window
    {
        private ObservableCollection<Room> roomList;

        public Thongtinphong()
        {
            InitializeComponent();

            roomList = new ObservableCollection<Room>();
            dgRooms.ItemsSource = roomList;

            cbRoomType.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoomNumber.Text))
            {
                MessageBox.Show("Vui lòng nhập số phòng!");
                return;
            }

            if (roomList.Any(r => r.RoomNumber == txtRoomNumber.Text))
            {
                MessageBox.Show("Số phòng đã tồn tại!");
                return;
            }

            Room room = new Room()
            {
                RoomNumber = txtRoomNumber.Text,
                Type = cbRoomType.Text,
                Status = cbStatus.Text
            };

            roomList.Add(room);
            MessageBox.Show("Thêm phòng thành công!");
            ClearForm();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgRooms.SelectedItem is Room selected)
            {
                selected.Type = cbRoomType.Text;
                selected.Status = cbStatus.Text;

                dgRooms.Items.Refresh();
                MessageBox.Show("Cập nhật thành công!");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgRooms.SelectedItem is Room selected)
            {
                roomList.Remove(selected);
                MessageBox.Show("Xóa phòng thành công!");
            }
        }

        private void dgRooms_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgRooms.SelectedItem is Room selected)
            {
                txtRoomNumber.Text = selected.RoomNumber;
                cbRoomType.Text = selected.Type;
                cbStatus.Text = selected.Status;
            }
        }

        private void ClearForm()
        {
            txtRoomNumber.Clear();
            cbRoomType.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
        }
    }

    public class Room
    {
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
}