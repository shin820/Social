using Common.Logging;

namespace Social.Job
{
    public class SchedulerLogger
    {
        public static ILog GetLogger()
        {
            return LogManager.GetLogger("Social.Job");
        }
    }
}
