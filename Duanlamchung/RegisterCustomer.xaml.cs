using System.Linq;
using System.Windows;

namespace Duanlamchung
{
    public partial class RegisterCustomer : Window
    {
        public string CreatedPhone { get; private set; }

        public RegisterCustomer()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var name = (txtName.Text ?? "").Trim();
            var phone = (txtPhone.Text ?? "").Trim();
            var cccd = (txtCccd.Text ?? "").Trim();
            var email = (txtEmail.Text ?? "").Trim();
            var address = (txtAddress.Text ?? "").Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Họ tên và SĐT là bắt buộc.");
                return;
            }

            using (var db = new HotelManagerEntities())
            {
                // chặn trùng SĐT (hoặc CCCD)
                var exists = db.customers.Any(x => x.phone == phone || (!string.IsNullOrEmpty(cccd) && x.identity_card == cccd));
                if (exists)
                {
                    MessageBox.Show("SĐT hoặc CCCD đã tồn tại.");
                    return;
                }

                var c = new customer
                {
                    full_name = name,
                    phone = phone,
                    identity_card = string.IsNullOrEmpty(cccd) ? null : cccd,
                    email = string.IsNullOrEmpty(email) ? null : email,
                    address = string.IsNullOrEmpty(address) ? null : address
                };

                db.customers.Add(c);
                db.SaveChanges();
            }

            CreatedPhone = phone;
            MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập bằng SĐT/CCCD.");
            DialogResult = true;
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}