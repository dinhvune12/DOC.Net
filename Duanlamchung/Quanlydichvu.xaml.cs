using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Duanlamchung
{
    public partial class Quanlydichvu : Window
    {
        private ObservableCollection<Service> serviceList;

        public Quanlydichvu()
        {
            InitializeComponent();

            serviceList = new ObservableCollection<Service>();

            dgServices.ItemsSource = serviceList;
        }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServiceName.Text) ||
                string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Đơn giá không hợp lệ!");
                return;
            }

            Service newService = new Service()
            {
                ServiceName = txtServiceName.Text,
                Price = price,
                IsActive = chkActive.IsChecked == true
            };

            serviceList.Add(newService);

            MessageBox.Show("Thêm dịch vụ thành công!");

            txtServiceName.Clear();
            txtPrice.Clear();
            chkActive.IsChecked = true;
        }

        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Mật khẩu không khớp!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password) ||
                string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            MessageBox.Show("Tạo tài khoản thành công!");

            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            txtFullName.Clear();
        }
        private void btnService_Click(object sender, RoutedEventArgs e)
        {
            Quanlydichvu frm = new Quanlydichvu();
            frm.Show();
        }
    }

    public class Service
    {
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}