using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Duanlamchung
{
    public partial class quanlikhachhang : Window
    {
        public quanlikhachhang()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            using (var db = new HotelManagerEntities())
            {
                dgCustomers.ItemsSource = db.customers
                    .OrderByDescending(c => c.id)
                    .ToList();
                txtCount.Text = dgCustomers.Items.Count.ToString();
            }
        }

        private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem is customer c)
            {
                txtHoTen.Text = c.full_name;
                txtSDT.Text = c.phone;
                txtCCCD.Text = c.identity_card;
                // txtGhiChu/txtSoPhong nếu DB bạn không có thì để trống
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var name = (txtHoTen.Text ?? "").Trim();
            var phone = (txtSDT.Text ?? "").Trim();
            var cccd = (txtCCCD.Text ?? "").Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Họ tên và SĐT là bắt buộc!");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                if (db.customers.Any(x => x.phone == phone))
                {
                    MessageBox.Show("SĐT đã tồn tại!");
                    return;
                }

                db.customers.Add(new customer
                {
                    full_name = name,
                    phone = phone,
                    identity_card = string.IsNullOrEmpty(cccd) ? null : cccd
                });

                db.SaveChanges();
            }

            LoadCustomers();
            MessageBox.Show("Thêm khách thành công!");
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgCustomers.SelectedItem is customer selected))
            {
                MessageBox.Show("Chọn khách để sửa!");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var c = db.customers.FirstOrDefault(x => x.id == selected.id);
                if (c == null) return;

                c.full_name = (txtHoTen.Text ?? "").Trim();
                c.phone = (txtSDT.Text ?? "").Trim();
                c.identity_card = (txtCCCD.Text ?? "").Trim();

                db.SaveChanges();
            }

            LoadCustomers();
            MessageBox.Show("Cập nhật thành công!");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgCustomers.SelectedItem is customer selected))
            {
                MessageBox.Show("Chọn khách để xóa!");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var c = db.customers.FirstOrDefault(x => x.id == selected.id);
                if (c == null) return;

                db.customers.Remove(c);
                db.SaveChanges();
            }

            LoadCustomers();
            MessageBox.Show("Đã xóa!");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var q = (txtSearch.Text ?? "").Trim().ToLower();

            using (var db = new HotelManagerEntities())
            {
                dgCustomers.ItemsSource = db.customers
                    .Where(c => (c.full_name ?? "").ToLower().Contains(q) || (c.phone ?? "").ToLower().Contains(q))
                    .OrderByDescending(c => c.id)
                    .ToList();
                txtCount.Text = dgCustomers.Items.Count.ToString();
            }
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            LoadCustomers();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => LoadCustomers();
    }
}