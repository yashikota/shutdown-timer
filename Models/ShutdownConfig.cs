using System;

namespace shutdown_timer.Models
{
    public enum ActionType
    {
        Shutdown,
        Restart
    }

    public enum TimerMode
    {
        Duration,
        SpecificTime
    }

    public class ShutdownConfig
    {
        public DateTime TargetTime { get; init; }
        public ActionType ActionType { get; init; } = ActionType.Shutdown;
        public bool ForceAction { get; init; }
        public TimerMode Mode { get; init; } = TimerMode.Duration;
        public TimeSpan Duration { get; init; }
        public TimeSpan SpecificTime { get; init; }

        public static ShutdownConfig CreateFromDuration(TimeSpan duration, ActionType actionType = ActionType.Shutdown, bool force = false)
        {
            return new ShutdownConfig
            {
                Mode = TimerMode.Duration,
                Duration = duration,
                TargetTime = DateTime.Now.Add(duration),
                ActionType = actionType,
                ForceAction = force
            };
        }

        public static ShutdownConfig CreateFromTime(TimeSpan time, ActionType actionType = ActionType.Shutdown, bool force = false)
        {
            var targetDateTime = DateTime.Today.Add(time);

            // If the time has already passed today, schedule for tomorrow
            if (targetDateTime <= DateTime.Now)
            {
                targetDateTime = targetDateTime.AddDays(1);
            }

            return new ShutdownConfig
            {
                Mode = TimerMode.SpecificTime,
                SpecificTime = time,
                TargetTime = targetDateTime,
                ActionType = actionType,
                ForceAction = force
            };
        }
    }
}
