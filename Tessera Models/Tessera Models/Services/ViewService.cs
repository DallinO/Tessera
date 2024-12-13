using Tessera.Models.Chapter;

namespace Tessera.Web.Services
{
    public interface IViewService
    {
        event Action<string> OnNotificationScheduled;
        string ButtonIdGen();
        void ScheduleNotification(string message, DateTime scheduledTime);
        void ResetNotifications();
    }

    public class ViewService : IViewService
    {
        private int _buttonId = 1;
        private List<NotificationDto> _notifications = new List<NotificationDto>();
        private readonly Timer _timer; // To periodically check for notifications
        public event Action<string> OnNotificationScheduled;


        public ViewService()
        {
            // Set the timer to check every 60 seconds
            _timer = new Timer(async _ => await CheckForNotifications(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
        }

        // Schedule a notification
        public void ScheduleNotification(string message, DateTime schedule)
        {
            var notification = new NotificationDto
            {
                Message = message,
                Schedule = schedule
            };

            _notifications.Add(notification);

            // Notify listeners about the scheduled notification
            OnNotificationScheduled?.Invoke(notification.Message);
        }

        // Check if any notification is due to be shown
        private async Task CheckForNotifications()
        {
            var now = DateTime.UtcNow;

            // Loop through the notifications and show any that are due
            List<NotificationDto> not = _notifications.Where(n => !n.IsShown && n.Schedule <= now).ToList();
            foreach (var notification in not)
            {
                // Mark as shown and raise the event to show the notification
                notification.IsShown = true;
                OnNotificationScheduled?.Invoke(notification.Message);
                await Task.Delay(6500);
            }
        }

        // Reset all notifications (e.g., when the app is refreshed or restarted)
        public void ResetNotifications()
        {
            _notifications.Clear();
        }

        public string ButtonIdGen()
        {
            return _buttonId++.ToString();
        }
    }

}
