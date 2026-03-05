csharp Duanlamchung\AppEvents.cs
using System;

namespace Duanlamchung
{
    // Tiny application-level event bus for cross-window notifications
    public static class AppEvents
    {
        // raised when bookings or room statuses change
        public static event Action BookingOrRoomChanged;

        public static void RaiseBookingOrRoomChanged()
        {
            BookingOrRoomChanged?.Invoke();
        }
    }
}