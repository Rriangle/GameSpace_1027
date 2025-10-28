using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 統計模型集合
    /// </summary>
    public class StatisticsModels
    {
        /// <summary>
        /// 簽到統計讀取模型
        /// </summary>
        public class SignInStatisticsReadModel
        {
            /// <summary>
            /// 總用戶數
            /// </summary>
            public int TotalUsers { get; set; }
            
            /// <summary>
            /// 活躍用戶數
            /// </summary>
            public int ActiveUsers { get; set; }
            
            /// <summary>
            /// 簽到率
            /// </summary>
            public double SignInRate { get; set; }
            
            /// <summary>
            /// 今日簽到數
            /// </summary>
            public int TodaySignIns { get; set; }
        }

        /// <summary>
        /// 寵物統計讀取模型
        /// </summary>
        public class PetStatisticsReadModel
        {
            /// <summary>
            /// 總寵物數
            /// </summary>
            public int TotalPets { get; set; }
            
            /// <summary>
            /// 平均等級
            /// </summary>
            public double AverageLevel { get; set; }
            
            /// <summary>
            /// 最高等級
            /// </summary>
            public int MaxLevel { get; set; }
        }

        /// <summary>
        /// 遊戲統計讀取模型
        /// </summary>
        public class GameStatisticsReadModel
        {
            /// <summary>
            /// 總遊戲次數
            /// </summary>
            public int TotalGames { get; set; }
            
            /// <summary>
            /// 平均分數
            /// </summary>
            public double AverageScore { get; set; }
            
            /// <summary>
            /// 最高分數
            /// </summary>
            public int MaxScore { get; set; }
        }
    }
}

