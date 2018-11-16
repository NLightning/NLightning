using System;

namespace NLightning.Utils
{
    public class ExponentialBackOff
    {
        public static TimeSpan Calculate(int failedAttempts, TimeSpan delayMinimum, TimeSpan delayMaximum)
        {
            if (delayMaximum < delayMinimum)
            {
                throw new ArgumentException("Invalid min and max delays.");
            }
            
            
            var delay = TimeSpan.FromSeconds(1d / 2d * (Math.Pow(2d, failedAttempts) - 1d));

            if (delay > delayMaximum)
            {
                delay = delayMaximum;
            }

            if (delay < delayMinimum)
            {
                delay = delayMinimum;
            }

            return delay;
        }
    }
}