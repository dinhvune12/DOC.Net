using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using System.Windows.Controls;
using System.Windows.Threading;
using Duanlamchung.Utils;

namespace Duanlamchung
{
    public partial class DangKyDangnhapKhach : Window       
    {
        // secret admin code để tạo tài khoản lễ tân (thay đổi theo môi trường)
        private const string AdminCreationCode = "vudeptrai";

        public DangKyDangnhapKhach()
        {
            InitializeComponent();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e) { /* implement forgot password if needed */ }

        private void BtnGuestContinue_Click(object sender, RoutedEventArgs e)
        {
            var win = new TimPhong { Owner = this };
            win.Show();
            Dispatcher.BeginInvoke(new Action(() => Close()), DispatcherPriority.Background);
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fullName = (TbRegFullName.Text ?? "").Trim();
                var phone = (TbRegPhone.Text ?? "").Trim();
                var email = (TbRegEmail.Text ?? "").Trim();
                var cccd = (TbRegCccd.Text ?? "").Trim();
                var password = (PbRegPassword?.Password ?? "");
                var confirm = (PbRegConfirm?.Password ?? "");

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    MessageBox.Show("Vui lòng nhập họ và tên.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(phone))
                {
                    MessageBox.Show("Vui lòng nhập số điện thoại.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CbAgree.IsChecked != true)
                {
                    MessageBox.Show("Bạn phải đồng ý với Điều khoản & Chính sách bảo mật.", "Chưa đồng ý", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (password.Length < 8)
                {
                    MessageBox.Show("Mật khẩu nên có ít nhất 8 ký tự.", "Mật khẩu yếu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (password != confirm)
                {
                    MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp.", "Lỗi mật khẩu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // lấy role từ ComboBox
                var role = (CbRegRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "customer";

                // nếu tạo lễ tân thì kiểm tra mã quản trị
                if (string.Equals(role, "receptionist", StringComparison.OrdinalIgnoreCase))
                {
                    var code = (TbAdminCode.Text ?? "").Trim();
                    if (code != AdminCreationCode)
                    {
                        MessageBox.Show("Mã quản trị không hợp lệ. Không thể tạo tài khoản lễ tân.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                using (var db = new HotelManagerEntities())
                {
                    var dup = db.customers.Any(x =>
                        x.phone == phone ||
                        (!string.IsNullOrEmpty(cccd) && x.identity_card == cccd) ||
                        (!string.IsNullOrEmpty(email) && x.email == email));

                    if (dup)
                    {
                        MessageBox.Show("Số điện thoại, CCCD hoặc email đã tồn tại.", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var customer = new customer
                    {
                        full_name = fullName,
                        phone = phone,
                        identity_card = string.IsNullOrWhiteSpace(cccd) ? null : cccd,
                        email = string.IsNullOrWhiteSpace(email) ? null : email,
                        address = null
                    };

                    db.customers.Add(customer);
                    db.SaveChanges();

                    var username = !string.IsNullOrWhiteSpace(email) ? email : phone;

                    var userExists = db.Set<user>().Any(u => u.username == username || u.phone == phone);
                    if (userExists)
                    {
                        db.customers.Remove(customer);
                        db.SaveChanges();
                        MessageBox.Show("Tài khoản người dùng đã tồn tại.", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var hashed = PasswordHasher.HashPassword(password);
                    var newUser = new user
                    {
                        username = username,
                        password = hashed,
                        full_name = fullName,
                        role = role,
                        phone = phone,
                        created_at = DateTime.Now
                    };

                    db.Set<user>().Add(newUser);
                    db.SaveChanges();
                }

                MessageBox.Show("Đăng ký thành công! Đang chuyển sang đăng nhập...", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                try
                {
                    AuthTab.SelectedIndex = 0;
                    TbLoginIdentity.Text = !string.IsNullOrWhiteSpace(email) ? email : phone;
                    PbLoginPassword.Password = password;
                    PerformLogin();
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đăng ký: " + ex.ToString(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool PerformLogin()
        {
            var identity = (TbLoginIdentity.Text ?? "").Trim();
            var password = (PbLoginPassword?.Password ?? "");
            var requireStaff = CbLoginAsStaff.IsChecked == true;

            if (string.IsNullOrWhiteSpace(identity) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập thông tin đăng nhập.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                using (var db = new HotelManagerEntities())
                {
                    var usr = db.Set<user>().FirstOrDefault(u => u.username == identity || u.phone == identity);
                    if (usr == null)
                    {
                        MessageBox.Show("Tài khoản không tồn tại.", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    if (!PasswordHasher.VerifyPassword(password, usr.password))
                    {
                        MessageBox.Show("Mật khẩu không đúng.", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    if (requireStaff && !string.Equals(usr.role, "receptionist", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Tài khoản này không có quyền lễ tân.", "Quyền truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    // --- Gán Session trước khi mở cửa sổ mới ---
                    Session.UserId = usr.id;
                    Session.UserFullName = usr.full_name;
                    Session.UserRole = usr.role;
                    // clear customer session when staff logs in via user table
                    Session.CustomerId = null;
                    Session.CustomerName = null;
                    // ------------------------------------------------

                    // authenticated -> chuyển hướng theo role
                    if (string.Equals(usr.role, "receptionist", StringComparison.OrdinalIgnoreCase))
                    {
                        var dashboard = new DashboardLeTan();
                        dashboard.Show();
                        Dispatcher.BeginInvoke(new Action(() => Close()), DispatcherPriority.Background);
                        return true;
                    }
                    else
                    {
                        var customerWin = new TimPhong();
                        customerWin.Show();
                        Dispatcher.BeginInvoke(new Action(() => Close()), DispatcherPriority.Background);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đăng nhập: " + ex.ToString(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Nav.Back(this);
        }
    }
}