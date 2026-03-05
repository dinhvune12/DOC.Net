namespace Duanlamchung
{
    using System;

    // Tiny app-level event bus for cross-window notifications
    public static class AppEvents
    {
        public static event Action BookingOrRoomChanged;
        public static void RaiseBookingOrRoomChanged() => BookingOrRoomChanged?.Invoke();
    }
}