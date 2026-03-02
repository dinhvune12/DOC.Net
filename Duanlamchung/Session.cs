using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duanlamchung
{
    public static class Session
    {
        public static user CurrentUser { get; private set; }

        public static bool IsStaff
        {
            get
            {
                return CurrentUser != null &&
                       string.Equals(CurrentUser.role, "receptionist", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void Login(user usr)
        {
            CurrentUser = usr;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}

