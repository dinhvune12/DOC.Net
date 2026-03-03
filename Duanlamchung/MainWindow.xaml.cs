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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Duanlamchung
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnRoom_Click(object sender, RoutedEventArgs e)
        {
            Thongtinphong frm = new Thongtinphong();
            frm.Show();
            this.Hide();
        }
    }
}

//using System.Windows;

//namespace Duanlamchung
//{
//    public partial class MainWindow : Window
//    {
//        public MainWindow()
//        {
//            InitializeComponent();

//            // Load mặc định trang phòng
//            MainFrame.Navigate(new Thongtinphong());
//        }

//        private void btnRoom_Click(object sender, RoutedEventArgs e)
//        {
//            MainFrame.Navigate(new Thongtinphong());
//        }

//        private void btnService_Click(object sender, RoutedEventArgs e)
//        {
//            MainFrame.Navigate(new Quanlydichvu());
//        }

//        private void btnAccount_Click(object sender, RoutedEventArgs e)
//        {
//            MainFrame.Navigate(new Quanlytaikhoan());
//        }
//    }
//}