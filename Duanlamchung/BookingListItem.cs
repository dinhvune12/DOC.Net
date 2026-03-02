using System;

namespace Duanlamchung
{
    public class BookingListItem
    {
        public int BookingIdRaw { get; set; }        
        public string MaBooking { get; set; }
        public string TenKhach { get; set; }
        public string CCCD { get; set; }
        public string SoPhong { get; set; }
        public string NgayCheckIn { get; set; }
        public string NgayCheckOut { get; set; }
        public string TrangThai { get; set; }
        public string TienCoc { get; set; }           
    }
}