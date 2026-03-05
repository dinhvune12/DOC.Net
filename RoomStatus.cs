csharp Duanlamchung\RoomStatus.cs
namespace Duanlamchung
{
    public static class RoomStatus
    {
        public const string Available = "available";
        public const string Occupied = "occupied";
        public const string Cleaning = "cleaning";
        public const string Maintenance = "maintenance";

        // booking statuses (use same canonical values across code)
        public const string Pending = "pending";
        public const string Confirmed = "confirmed";
        public const string Cancelled = "cancelled";
        public const string CheckedIn = "checked_in";
    }
}