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
    /// <summary>
    /// Interaction logic for DangKyDangnhapKhach.xaml
    /// </summary>
    public partial class DangKyDangnhapKhach : Window
    {
        public DangKyDangnhapKhach()
        {
            InitializeComponent();
        }
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnLogin_Click(object sender, RoutedEventArgs e) { }
        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e) { }
        private void BtnGuestContinue_Click(object sender, RoutedEventArgs e) { }
        private void BtnRegister_Click(object sender, RoutedEventArgs e) { }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
