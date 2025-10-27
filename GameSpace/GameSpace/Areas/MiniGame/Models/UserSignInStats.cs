using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 簽到記錄 ViewModel（用於 MiniGame Area）
    /// 注意：這是 ViewModel，不是實體類型，不映射到資料庫
    /// </summary>
    public class UserSignInStats
    {
        public int LogID { get; set; }

        // Alias for compatibility
        [NotMapped]
        public int StatsID
        {
            get => LogID;
            set => LogID = value;
        }

        public int UserID { get; set; }

        public DateTime SignTime { get; set; } = DateTime.Now;

        public int PointsGained { get; set; } = 0;

        // Alias for compatibility
        [NotMapped]
        public int PointsEarned
        {
            get => PointsGained;
            set => PointsGained = value;
        }

        public DateTime PointsGainedTime { get; set; }

        public int ExpGained { get; set; } = 0;

        // Alias for compatibility
        [NotMapped]
        public int PetExpEarned
        {
            get => ExpGained;
            set => ExpGained = value;
        }

        public DateTime ExpGainedTime { get; set; }

        public string? CouponGained { get; set; }

        // Alias for compatibility (int? instead of string?)
        [NotMapped]
        public int? CouponEarned
        {
            get => string.IsNullOrEmpty(CouponGained) ? null : int.TryParse(CouponGained, out int val) ? val : null;
            set => CouponGained = value?.ToString();
        }

        public DateTime? CouponGainedTime { get; set; }

        public int ConsecutiveDays { get; set; } = 1;
    }
}

