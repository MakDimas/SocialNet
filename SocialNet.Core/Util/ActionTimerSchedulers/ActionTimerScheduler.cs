using Microsoft.Extensions.Logging;

namespace SocialNet.Core.Util.ActionTimerSchedulers
{
    public class ActionTimerScheduler : IActionTimerScheduler, IDisposable
    {
        private readonly List<Timer> _timers = [];
        private readonly ILogger<ActionTimerScheduler> _logger;

        public ActionTimerScheduler(ILogger<ActionTimerScheduler> logger)
        {
            _logger = logger;
        }

        public void ScheduleAction(Func<Task> asyncAction, TimeSpan dueTime, TimeSpan period)
        {
            var timer = new Timer(async _ =>
            {
                try
                {
                    await asyncAction();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in scheduled action: {ex.Message}");
                }
            }, null, dueTime, period);

            _timers.Add(timer);
        }

        public void Dispose()
        {
            foreach (var timer in _timers)
            {
                timer?.Dispose();
            }
            _timers.Clear();
        }
    }
}
