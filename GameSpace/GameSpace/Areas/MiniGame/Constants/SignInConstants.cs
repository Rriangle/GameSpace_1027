namespace GameSpace.Areas.MiniGame.Constants
{
    public static class SignInConstants
    {
        // 簽到獎勵（與 CLAUDE.md 對應）
        public const int WeekdayPoints = 20;
        public const int WeekdayExperience = 0;
        public const int WeekendPoints = 30;
        public const int WeekendExperience = 200;
        public const int StreakBonusPoints = 40;
        public const int StreakBonusExperience = 300;
        public const int PerfectMonthPoints = 200;
        public const int PerfectMonthExperience = 2000;

        // 連續簽到天數閾值
        public const int StreakThresholdDays = 7;

        // 驗證限制
        public const int MaxPointsPerSignIn = 1000;
        public const int MaxExperiencePerSignIn = 500;
    }
}
