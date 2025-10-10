namespace SocialNet.Core.Util.ActionTimerSchedulers;

public interface IActionTimerScheduler
{
    public void ScheduleAction(Func<Task> asyncAction, TimeSpan dueTime, TimeSpan period);
}
