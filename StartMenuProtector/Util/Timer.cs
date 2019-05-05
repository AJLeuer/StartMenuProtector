using System;
using NodaTime;

namespace StartMenuProtector.Util
{
    public class Timer
    {
        public Duration ElapsedTime
        {
            get
            {
                if (StartTime != null)
                {
                    return SystemClock.Instance.GetCurrentInstant() - StartTime.Value;
                }
                else
                {
                    return Duration.FromSeconds(0);
                }
            }
        }

        public Instant? StartTime { get; private set; } = null;
        public Duration Duration { get; set; }

        public bool Started { get; private set; } = false;

        public bool Finished
        {
            get
            {
                if (Started == false)
                {
                    return true;
                }
                else if (ElapsedTime >= Duration)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Timer(Duration duration)
        {
            this.Duration = duration;
        }
        
        public void Start()
        {
            if (this.Started)
            {
                throw new InvalidOperationException("Timer must finish or stop before starting again");
            }
            StartTime = SystemClock.Instance.GetCurrentInstant();
            Started = true;
        }

        public void Start(Duration duration)
        {
            this.Duration = duration;
            Start();
        }

        public Duration Stop()
        {
            Duration elapsed = ElapsedTime;
            Started = false;
            StartTime = null;
            return elapsed;
        }


    }
}