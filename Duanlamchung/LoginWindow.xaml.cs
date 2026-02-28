using System;
using System.Linq;
using System.Windows;

namespace Duanlamchung
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLoginStaff_Click(object sender, RoutedEventArgs e)
        {
            txtStaffMsg.Text = "";

            var username = (txtStaffUsername.Text ?? "").Trim();
            var password = (txtStaffPassword.Password ?? "").Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                txtStaffMsg.Text = "Nhập username và password.";
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                // Demo: password đang plaintext
                var u = db.users.FirstOrDefault(x => x.username == username && x.password == password);
                if (u == null)
                {
                    txtStaffMsg.Text = "Sai tài khoản hoặc mật khẩu.";
                    return;
                }

                Session.UserId = u.id;
                Session.UserFullName = u.full_name;
                Session.UserRole = u.role;
                Session.CustomerId = null;
                Session.CustomerName = null;
            }

            // Điều hướng màn nhân viên (bạn đổi sang màn bạn muốn)
            var dash = new DashboardLeTan();
            dash.Show();
            Close();
        }

        private void btnLoginCustomer_Click(object sender, RoutedEventArgs e)
        {
            txtCusMsg.Text = "";

            var key = (txtCusKey.Text ?? "").Trim();
            if (string.IsNullOrEmpty(key))
            {
                txtCusMsg.Text = "Nhập SĐT hoặc CCCD.";
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                var c = db.customers.FirstOrDefault(x => x.phone == key || x.identity_card == key);
                if (c == null)
                {
                    txtCusMsg.Text = "Không tìm thấy khách. Hãy đăng ký trước (hoặc bấm nút demo).";
                    return;
                }

                Session.CustomerId = c.id;
                Session.CustomerName = c.full_name;
                Session.UserId = null;
                Session.UserFullName = null;
                Session.UserRole = null;
            }

            // Điều hướng màn khách (bạn đổi sang màn khách bạn muốn)
            var w = new DatPhong(); // hoặc MainWindow
            w.Show();
            Close();
        }

        // Nút demo: tạo 1 khách test nhanh để login
        private void btnCreateCustomerDemo_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new HotelManagerEntities())
            {
                var demoPhone = "0900000000";
                var exist = db.customers.FirstOrDefault(x => x.phone == demoPhone);
                if (exist != null)
                {
                    MessageBox.Show("Đã có khách demo. Login bằng SĐT: 0900000000");
                    txtCusKey.Text = demoPhone;
                    return;
                }

                var c = new customer
                {
                    full_name = "Khách Demo",
                    phone = demoPhone,
                    identity_card = "123456789",
                    email = "demo@gmail.com",
                    address = "Demo"
                };
                db.customers.Add(c);
                db.SaveChanges();

                MessageBox.Show("Tạo khách demo thành công! Login bằng SĐT: 0900000000");
                txtCusKey.Text = demoPhone;
            }
        }
    }
}