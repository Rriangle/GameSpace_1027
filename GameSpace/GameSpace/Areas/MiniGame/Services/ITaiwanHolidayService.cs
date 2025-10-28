using System;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// Taiwan holiday detection service
    /// Determines if a given date is a weekend or public holiday in Taiwan
    /// </summary>
    public interface ITaiwanHolidayService
    {
        /// <summary>
        /// Check if a date is a holiday (weekend or public holiday) in Taiwan timezone
        /// </summary>
        /// <param name="date">Date in Taiwan timezone (Asia/Taipei)</param>
        /// <returns>True if the date is a holiday, false otherwise</returns>
        bool IsHoliday(DateTime date);

        /// <summary>
        /// Check if a date is a weekend (Saturday or Sunday)
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if weekend, false otherwise</returns>
        bool IsWeekend(DateTime date);

        /// <summary>
        /// Check if a date is a Taiwan public holiday
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if public holiday, false otherwise</returns>
        bool IsPublicHoliday(DateTime date);
    }
}
