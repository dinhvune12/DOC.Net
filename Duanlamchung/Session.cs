using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duanlamchung
{
    
        public static class Session
        {
            public static int? UserId { get; set; }
            public static string UserFullName { get; set; }
            public static string UserRole { get; set; }

            public static int? CustomerId { get; set; }
            public static string CustomerName { get; set; }

            public static bool IsStaff => UserId.HasValue;
            public static bool IsCustomer => CustomerId.HasValue;

            public static void Logout()
            {
                UserId = null; UserFullName = null; UserRole = null;
                CustomerId = null; CustomerName = null;
            }
        }
    }

