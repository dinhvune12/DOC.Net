using System;
using System.Collections.Generic;
using System.Windows;

namespace Duanlamchung
{
    public static class Nav
    {
        private static readonly object _lock = new object();
        private static readonly Stack<Window> _history = new Stack<Window>();

        // Current root navigation (no-op if empty)
        public static bool CanGoBack
        {
            get
            {
                lock (_lock) { return _history.Count > 0; }
            }
        }

        // Navigate: push current, show next, hide current (do NOT Close here)
        public static void Go(Window current, Window next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            lock (_lock)
            {
                if (current != null)
                {
                    // push current so we can return to it later
                    _history.Push(current);
                    try { current.Hide(); } catch { /* safe-fail */ }
                }

                next.Show();
            }
        }

        // Show dialog without affecting history
        public static void Dialog(Window current, Window dialog)
        {
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));
            dialog.Owner = current;
            dialog.ShowDialog();
        }

        // Go back to previous window in history.
        // current will be closed; previous will be shown.
        public static void Back(Window current)
        {
            Window previous = null;
            lock (_lock)
            {
                if (_history.Count == 0)
                {
                    // nothing to go back to
                    previous = null;
                }
                else
                {
                    previous = _history.Pop();
                }
            }

            if (previous != null)
            {
                // show previous and close current
                try { previous.Show(); } catch { /* best-effort */ }
                try { current?.Close(); } catch { /* best-effort */ }
            }
            else
            {
                // no previous: close current (or keep it)
                try { current?.Close(); } catch { }
            }
        }

        // Clears navigation history (useful when going to root)
        public static void ClearHistory()
        {
            lock (_lock)
            {
                _history.Clear();
            }
        }
    }
}