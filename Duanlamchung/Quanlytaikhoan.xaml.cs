using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Duanlamchung
{
    public partial class Quanlytaikhoan : Window
    {
        private ObservableCollection<Account> accountList;

        public Quanlytaikhoan()
        {
            InitializeComponent();

            accountList = new ObservableCollection<Account>();
            dgAccounts.ItemsSource = accountList;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (accountList.Any(a => a.Username == txtUsername.Text))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại!");
                return;
            }

            Account acc = new Account()
            {
                Username = txtUsername.Text,
                Password = txtPassword.Password,
                FullName = txtFullName.Text,
                Role = ((ComboBoxItem)cbRole.SelectedItem).Content.ToString(),
                IsActive = chkIsActive.IsChecked == true
            };

            accountList.Add(acc);
            MessageBox.Show("Thêm tài khoản thành công!");
            ClearForm();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgAccounts.SelectedItem is Account selected)
            {
                selected.FullName = txtFullName.Text;
                selected.Role = ((ComboBoxItem)cbRole.SelectedItem).Content.ToString();
                selected.IsActive = chkIsActive.IsChecked == true;

                dgAccounts.Items.Refresh();
                MessageBox.Show("Cập nhật thành công!");
            }
        }

        private void btnDeactivate_Click(object sender, RoutedEventArgs e)
        {
            if (dgAccounts.SelectedItem is Account selected)
            {
                selected.IsActive = false;
                dgAccounts.Items.Refresh();
                MessageBox.Show("Tài khoản đã bị vô hiệu hóa!");
            }
        }

        private void dgAccounts_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgAccounts.SelectedItem is Account selected)
            {
                txtUsername.Text = selected.Username;
                txtFullName.Text = selected.FullName;
                txtPassword.Password = selected.Password;
                chkIsActive.IsChecked = selected.IsActive;
            }
        }

        private void txtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.ToLower();

            dgAccounts.ItemsSource = accountList
                .Where(a => a.Username.ToLower().Contains(keyword))
                .ToList();
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtFullName.Clear();
            chkIsActive.IsChecked = true;
            cbRole.SelectedIndex = 0;
        }

        private void btnAccount_Click(object sender, RoutedEventArgs e)
        {
            Quanlytaikhoan frm = new Quanlytaikhoan();
            frm.Show();
        }
    }

    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        public string Status => IsActive ? "Hoạt động" : "Vô hiệu";

        public Brush StatusColor => IsActive ? Brushes.Green : Brushes.Red;
    }
}