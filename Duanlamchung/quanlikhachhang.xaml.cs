using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Duanlamchung
{
    public partial class quanlikhachhang : Window
    {
        private class CustomerRow
        {
            public int CustomerId { get; set; }
            public string MaKH { get; set; }
            public string HoTen { get; set; }
            public string CCCD { get; set; }
            public string SDT { get; set; }
            public string SoPhong { get; set; }
            public string GhiChu { get; set; }
        }

        public quanlikhachhang()
        {
            InitializeComponent();
            SeedData.EnsureSeed();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            using (var db = new HotelManagerEntities())
            {
                var raw = (from c in db.customers
                           join b in db.bookings on c.id equals b.customer_id into bookingGroup
                           from b in bookingGroup.OrderByDescending(x => x.id).Take(1).DefaultIfEmpty()
                           join r in db.rooms on b.room_id equals r.id into roomGroup
                           from r in roomGroup.DefaultIfEmpty()
                           orderby c.id descending
                           select new
                           {
                               c.id,
                               c.full_name,
                               c.identity_card,
                               c.phone,
                               SoPhong = r.room_number,
                               c.address
                           }).ToList();

                var list = raw.Select(x => new CustomerRow
                {
                    CustomerId = x.id,
                    MaKH = x.id.ToString(),
                    HoTen = x.full_name,
                    CCCD = x.identity_card,
                    SDT = x.phone,
                    SoPhong = x.SoPhong,
                    GhiChu = x.address
                }).ToList();

                dgCustomers.ItemsSource = list;
                txtCount.Text = $"({list.Count} dòng)";
            }
        }

        private CustomerRow GetSelectedRow()
        {
            return dgCustomers.SelectedItem as CustomerRow;
        }

        private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = GetSelectedRow();
            if (row == null)
            {
                txtHoTen.Text = "";
                txtSDT.Text = "";
                txtCCCD.Text = "";
                txtSoPhong.Text = "";
                txtGhiChu.Text = "";
                return;
            }

            txtHoTen.Text = row.HoTen ?? "";
            txtSDT.Text = row.SDT ?? "";
            txtCCCD.Text = row.CCCD ?? "";
            txtSoPhong.Text = row.SoPhong ?? "";
            txtGhiChu.Text = row.GhiChu ?? "";
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var name = (txtHoTen.Text ?? "").Trim();
            var phone = (txtSDT.Text ?? "").Trim();
            var cccd = (txtCCCD.Text ?? "").Trim();
            var note = (txtGhiChu.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
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

                if (!string.IsNullOrEmpty(cccd) && db.customers.Any(x => x.identity_card == cccd))
                {
                    MessageBox.Show("CCCD đã tồn tại!");
                    return;
                }

                db.customers.Add(new customer
                {
                    full_name = name,
                    phone = phone,
                    identity_card = string.IsNullOrEmpty(cccd) ? null : cccd,
                    address = string.IsNullOrEmpty(note) ? null : note
                });

                db.SaveChanges();
            }

            LoadCustomers();
            MessageBox.Show("Thêm khách thành công!");
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedRow();
            if (selected == null)
            {
                MessageBox.Show("Chọn khách để sửa!");
                return;
            }

            var name = (txtHoTen.Text ?? "").Trim();
            var phone = (txtSDT.Text ?? "").Trim();
            var cccd = (txtCCCD.Text ?? "").Trim();
            var note = (txtGhiChu.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Họ tên và SĐT là bắt buộc!");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var c = db.customers.FirstOrDefault(x => x.id == selected.CustomerId);
                if (c == null)
                {
                    MessageBox.Show("Không tìm thấy khách hàng trong CSDL.");
                    return;
                }

                if (db.customers.Any(x => x.id != c.id && x.phone == phone))
                {
                    MessageBox.Show("SĐT đã tồn tại ở khách khác!");
                    return;
                }

                if (!string.IsNullOrEmpty(cccd) && db.customers.Any(x => x.id != c.id && x.identity_card == cccd))
                {
                    MessageBox.Show("CCCD đã tồn tại ở khách khác!");
                    return;
                }

                c.full_name = name;
                c.phone = phone;
                c.identity_card = string.IsNullOrEmpty(cccd) ? null : cccd;
                c.address = string.IsNullOrEmpty(note) ? null : note;

                db.SaveChanges();
            }

            LoadCustomers();
            MessageBox.Show("Cập nhật thành công!");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedRow();
            if (selected == null)
            {
                MessageBox.Show("Chọn khách để xóa!");
                return;
            }

            if (MessageBox.Show("Bạn có chắc muốn xóa khách này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using (var db = new HotelManagerEntities())
            {
                var c = db.customers.FirstOrDefault(x => x.id == selected.CustomerId);
                if (c == null) return;

                bool hasBooking = db.bookings.Any(b => b.customer_id == c.id);
                if (hasBooking)
                {
                    MessageBox.Show("Khách hàng đã có booking, không thể xóa.");
                    return;
                }

                db.customers.Remove(c);
                db.SaveChanges();
            }

            LoadCustomers();
            MessageBox.Show("Đã xóa!");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string q = (txtSearch.Text ?? "").Trim();
            int byIndex = cbSearchBy.SelectedIndex;

            var all = dgCustomers.ItemsSource as System.Collections.Generic.List<CustomerRow>;
            if (all == null)
            {
                LoadCustomers();
                all = dgCustomers.ItemsSource as System.Collections.Generic.List<CustomerRow>;
            }

            var source = all ?? new System.Collections.Generic.List<CustomerRow>();

            var filtered = string.IsNullOrWhiteSpace(q)
                ? source
                : source.Where(x =>
                {
                    var key = byIndex == 1 ? (x.CCCD ?? "")
                            : byIndex == 2 ? (x.SDT ?? "")
                            : byIndex == 3 ? (x.SoPhong ?? "")
                            : (x.HoTen ?? "");

                    return key.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                }).ToList();

            dgCustomers.ItemsSource = filtered;
            txtCount.Text = $"({filtered.Count} dòng)";
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cbSearchBy.SelectedIndex = 0;
            LoadCustomers();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCustomers();
        }
    }
}
