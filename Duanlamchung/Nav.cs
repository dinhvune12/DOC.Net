using System.Windows;

namespace Duanlamchung
{
    public static class Nav
    {
        public static void Go(Window current, Window next)
        {
            next.Show();
            current.Close();
        }

        public static void Dialog(Window current, Window dialog)
        {
            dialog.Owner = current;
            dialog.ShowDialog();
        }
    }
}