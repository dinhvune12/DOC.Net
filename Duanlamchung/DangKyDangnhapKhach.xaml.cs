using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using System.Windows.Controls;
using Duanlamchung.Utils;

namespace Duanlamchung
{
    public partial class DangKyDangnhapKhach : Window
    {
        // secret admin code để tạo tài khoản lễ tân
        private const string AdminCreationCode = "vudeptrai";

        public DangKyDangnhapKhach()
        {
            InitializeComponent();
        }
        private void OpenNextWindow(Window nextWindow)
        {
            if (nextWindow == null) return;

            nextWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // đặt cửa sổ mới thành MainWindow trước khi đóng cửa sổ hiện tại
            Application.Current.MainWindow = nextWindow;

            nextWindow.Show();

            // đóng cửa sổ hiện tại sau khi cửa sổ mới đã mở
            this.Close();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng quên mật khẩu chưa được cài đặt.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGuestContinue_Click(object sender, RoutedEventArgs e)
        {
            OpenNextWindow(new TimPhong());
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fullName = ReadTextControl(TbRegFullName).Trim();
                var phone = ReadTextControl(TbRegPhone).Trim();
                var email = ReadTextControl(TbRegEmail).Trim();
                var cccd = ReadTextControl(TbRegCccd).Trim();

                var password = ReadPasswordValue(PbRegPassword, "PbRegPassword", "TbRegPassword", "TxtRegPassword", "RegPasswordTextBox");
                var confirm = ReadPasswordValue(PbRegConfirm, "PbRegConfirm", "TbRegConfirm", "TxtRegConfirm", "RegConfirmTextBox");

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

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                var role = (CbRegRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "customer";

                if (string.Equals(role, "receptionist", StringComparison.OrdinalIgnoreCase))
                {
                    var code = ReadTextControl(TbAdminCode).Trim();
                    if (code != AdminCreationCode)
                    {
                        MessageBox.Show("Mã quản trị không hợp lệ. Không thể tạo tài khoản lễ tân.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                user newUser;

                using (var db = new HotelManagerEntities())
                {
                    var dupCustomer = db.customers.Any(x =>
                        x.phone == phone ||
                        (!string.IsNullOrEmpty(cccd) && x.identity_card == cccd) ||
                        (!string.IsNullOrEmpty(email) && x.email == email));

                    if (dupCustomer)
                    {
                        MessageBox.Show("Số điện thoại, CCCD hoặc email đã tồn tại.", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var username = !string.IsNullOrWhiteSpace(email) ? email : phone;

                    var userExists = db.Set<user>().Any(u => u.username == username || u.phone == phone);
                    if (userExists)
                    {
                        MessageBox.Show("Tài khoản người dùng đã tồn tại.", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                    var hashed = PasswordHasher.HashPassword(password);
                    newUser = new user
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

                MessageBox.Show("Tạo tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // auto login luôn sau khi tạo tài khoản
                Session.Login(newUser);

                if (string.Equals(role, "receptionist", StringComparison.OrdinalIgnoreCase))
                {
                    OpenNextWindow(new DashboardLeTan());
                }
                else
                {
                    OpenNextWindow(new TimPhong());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đăng ký: " + ex.ToString(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool PerformLogin()
        {
            var identity = ReadTextControl(TbLoginIdentity).Trim();
            var password = ReadPasswordValue(PbLoginPassword, "PbLoginPassword", "TbLoginPassword", "TxtLoginPassword", "LoginPasswordTextBox");
            var requireStaff = CbLoginAsStaff.IsChecked == true;

            if (string.IsNullOrWhiteSpace(identity) || string.IsNullOrWhiteSpace(password))
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

                    bool isPasswordCorrect = false;

                    if (usr.password == password)
                    {
                        isPasswordCorrect = true;
                    }
                    else
                    {
                        try
                        {
                            isPasswordCorrect = PasswordHasher.VerifyPassword(password, usr.password);
                        }
                        catch
                        {
                            isPasswordCorrect = false;
                        }
                    }

                    if (!isPasswordCorrect)
                    {
                        MessageBox.Show("Mật khẩu không đúng.", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    if (requireStaff && !string.Equals(usr.role, "receptionist", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Tài khoản này không có quyền lễ tân.", "Quyền truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    MessageBox.Show("Đăng nhập thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    Session.Login(usr);

                    if (string.Equals(usr.role, "receptionist", StringComparison.OrdinalIgnoreCase))
                    {
                        OpenNextWindow(new DashboardLeTan());
                    }
                    else
                    {
                        OpenNextWindow(new TimPhong());
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đăng nhập: " + ex.ToString(), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private string ReadTextControl(object control)
        {
            if (control is TextBox tb)
                return tb.Text ?? "";

            return "";
        }

        private string ReadPasswordValue(object mainControl, params string[] fallbackNames)
        {
            // ưu tiên control chính
            if (mainControl is PasswordBox pb && !string.IsNullOrEmpty(pb.Password))
                return pb.Password;

            if (mainControl is TextBox tb && !string.IsNullOrEmpty(tb.Text))
                return tb.Text;

            // fallback theo tên control trong XAML
            foreach (var name in fallbackNames)
            {
                var found = this.FindName(name);

                if (found is PasswordBox foundPb && !string.IsNullOrEmpty(foundPb.Password))
                    return foundPb.Password;

                if (found is TextBox foundTb && !string.IsNullOrEmpty(foundTb.Text))
                    return foundTb.Text;
            }

            return "";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}