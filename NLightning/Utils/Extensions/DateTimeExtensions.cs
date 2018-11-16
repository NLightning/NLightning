using System;

namespace NLightning.Utils.Extensions
{
    public static class DateTimeExtensions
    {
        public static uint ToUnixSeconds(this DateTime value)
        {
            return (uint) Math.Truncate(value.ToUniversalTime()
                                            .Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }
        
        public static DateTime CreateFromUnixSeconds(uint unixSeconds)
        {
            DateTime dateTime = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixSeconds).ToLocalTime();
            return dateTime;
        }
    }
}