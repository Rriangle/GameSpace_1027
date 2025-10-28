using System;
using System.Collections.Generic;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// Taiwan holiday detection service implementation
    /// Includes Taiwan public holidays for 2024-2026
    /// </summary>
    public class TaiwanHolidayService : ITaiwanHolidayService
    {
        // Taiwan public holidays (excluding weekends)
        // Format: "MM-dd" for annual holidays, "yyyy-MM-dd" for specific year holidays
        private readonly HashSet<string> _publicHolidays = new HashSet<string>
        {
            // Annual fixed holidays
            "01-01", // New Year's Day (元旦)
            "02-28", // Peace Memorial Day (和平紀念日)
            "04-04", // Children's Day (兒童節)
            "04-05", // Tomb Sweeping Day (清明節)
            "10-10", // National Day (國慶日)

            // 2024 specific holidays
            "2024-02-08", // Lunar New Year Eve (除夕)
            "2024-02-09", // Spring Festival (春節初一)
            "2024-02-10", // Spring Festival (春節初二)
            "2024-02-11", // Spring Festival (春節初三)
            "2024-02-12", // Spring Festival (春節初四)
            "2024-02-13", // Spring Festival (春節初五)
            "2024-02-14", // Spring Festival (春節初六)
            "2024-06-10", // Dragon Boat Festival (端午節)
            "2024-09-17", // Mid-Autumn Festival (中秋節)

            // 2025 specific holidays
            "2025-01-27", // Lunar New Year Eve (除夕)
            "2025-01-28", // Spring Festival (春節初一)
            "2025-01-29", // Spring Festival (春節初二)
            "2025-01-30", // Spring Festival (春節初三)
            "2025-01-31", // Spring Festival (春節初四)
            "2025-05-31", // Dragon Boat Festival (端午節)
            "2025-10-06", // Mid-Autumn Festival (中秋節)

            // 2026 specific holidays
            "2026-02-16", // Lunar New Year Eve (除夕)
            "2026-02-17", // Spring Festival (春節初一)
            "2026-02-18", // Spring Festival (春節初二)
            "2026-02-19", // Spring Festival (春節初三)
            "2026-02-20", // Spring Festival (春節初四)
            "2026-06-19", // Dragon Boat Festival (端午節)
            "2026-09-25", // Mid-Autumn Festival (中秋節)
        };

        /// <summary>
        /// Check if a date is a holiday (weekend or public holiday)
        /// </summary>
        public bool IsHoliday(DateTime date)
        {
            return IsWeekend(date) || IsPublicHoliday(date);
        }

        /// <summary>
        /// Check if a date is a weekend (Saturday or Sunday)
        /// </summary>
        public bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Check if a date is a Taiwan public holiday
        /// </summary>
        public bool IsPublicHoliday(DateTime date)
        {
            // Check for specific year holiday first (e.g., "2024-02-10")
            string specificDate = date.ToString("yyyy-MM-dd");
            if (_publicHolidays.Contains(specificDate))
            {
                return true;
            }

            // Check for annual holiday (e.g., "01-01")
            string annualDate = date.ToString("MM-dd");
            return _publicHolidays.Contains(annualDate);
        }
    }
}
