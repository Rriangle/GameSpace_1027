using MiniGameEntity = GameSpace.Models.MiniGame;
using Microsoft.Data.SqlClient;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// ⚠️ 此服務已棄用，請使用 GamePlayService 替代
    /// 原因：使用 ADO.NET 而非 EF Core，且功能與 GamePlayService 重複
    /// </summary>
    [Obsolete("此服務已棄用，請使用 GamePlayService 替代。MiniGameService 使用 ADO.NET 且功能與 GamePlayService 重複。", false)]
    public class MiniGameService : IMiniGameService
    {
        private readonly string _connectionString;

        public MiniGameService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<MiniGameEntity>> GetAllMiniGamesAsync()
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame ORDER BY StartTime DESC", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    PlayId = reader.GetInt32(reader.GetOrdinal("PlayId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    PetId = reader.GetInt32(reader.GetOrdinal("PetId")),
                    Level = reader.GetInt32(reader.GetOrdinal("Level")),
                    MonsterCount = reader.GetInt32(reader.GetOrdinal("MonsterCount")),
                    SpeedMultiplier = reader.GetDecimal(reader.GetOrdinal("SpeedMultiplier")),
                    Result = reader.GetString(reader.GetOrdinal("Result")),
                    ExpGained = reader.GetInt32(reader.GetOrdinal("ExpGained")),
                    ExpGainedTime = reader.GetDateTime(reader.GetOrdinal("ExpGainedTime")),
                    PointsGained = reader.GetInt32(reader.GetOrdinal("PointsGained")),
                    PointsGainedTime = reader.GetDateTime(reader.GetOrdinal("PointsGainedTime")),
                    CouponGained = reader.GetString(reader.GetOrdinal("CouponGained")),
                    CouponGainedTime = reader.GetDateTime(reader.GetOrdinal("CouponGainedTime")),
                    HungerDelta = reader.GetInt32(reader.GetOrdinal("HungerDelta")),
                    MoodDelta = reader.GetInt32(reader.GetOrdinal("MoodDelta")),
                    StaminaDelta = reader.GetInt32(reader.GetOrdinal("StaminaDelta")),
                    CleanlinessDelta = reader.GetInt32(reader.GetOrdinal("CleanlinessDelta")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    Aborted = reader.GetBoolean(reader.GetOrdinal("Aborted"))
                });
            }
            return games;
        }

        public async Task<IEnumerable<MiniGameEntity>> GetMiniGamesByUserIdAsync(int userId)
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame WHERE UserId = @UserId ORDER BY StartTime DESC", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    PlayId = reader.GetInt32(reader.GetOrdinal("PlayId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    PetId = reader.GetInt32(reader.GetOrdinal("PetId")),
                    Level = reader.GetInt32(reader.GetOrdinal("Level")),
                    MonsterCount = reader.GetInt32(reader.GetOrdinal("MonsterCount")),
                    SpeedMultiplier = reader.GetDecimal(reader.GetOrdinal("SpeedMultiplier")),
                    Result = reader.GetString(reader.GetOrdinal("Result")),
                    ExpGained = reader.GetInt32(reader.GetOrdinal("ExpGained")),
                    ExpGainedTime = reader.GetDateTime(reader.GetOrdinal("ExpGainedTime")),
                    PointsGained = reader.GetInt32(reader.GetOrdinal("PointsGained")),
                    PointsGainedTime = reader.GetDateTime(reader.GetOrdinal("PointsGainedTime")),
                    CouponGained = reader.GetString(reader.GetOrdinal("CouponGained")),
                    CouponGainedTime = reader.GetDateTime(reader.GetOrdinal("CouponGainedTime")),
                    HungerDelta = reader.GetInt32(reader.GetOrdinal("HungerDelta")),
                    MoodDelta = reader.GetInt32(reader.GetOrdinal("MoodDelta")),
                    StaminaDelta = reader.GetInt32(reader.GetOrdinal("StaminaDelta")),
                    CleanlinessDelta = reader.GetInt32(reader.GetOrdinal("CleanlinessDelta")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    Aborted = reader.GetBoolean(reader.GetOrdinal("Aborted"))
                });
            }
            return games;
        }

        public async Task<IEnumerable<MiniGameEntity>> GetMiniGamesByPetIdAsync(int petId)
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame WHERE PetId = @PetId ORDER BY StartTime DESC", connection);
            command.Parameters.AddWithValue("@PetId", petId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    PlayId = reader.GetInt32(reader.GetOrdinal("PlayId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    PetId = reader.GetInt32(reader.GetOrdinal("PetId")),
                    Level = reader.GetInt32(reader.GetOrdinal("Level")),
                    MonsterCount = reader.GetInt32(reader.GetOrdinal("MonsterCount")),
                    SpeedMultiplier = reader.GetDecimal(reader.GetOrdinal("SpeedMultiplier")),
                    Result = reader.GetString(reader.GetOrdinal("Result")),
                    ExpGained = reader.GetInt32(reader.GetOrdinal("ExpGained")),
                    ExpGainedTime = reader.GetDateTime(reader.GetOrdinal("ExpGainedTime")),
                    PointsGained = reader.GetInt32(reader.GetOrdinal("PointsGained")),
                    PointsGainedTime = reader.GetDateTime(reader.GetOrdinal("PointsGainedTime")),
                    CouponGained = reader.GetString(reader.GetOrdinal("CouponGained")),
                    CouponGainedTime = reader.GetDateTime(reader.GetOrdinal("CouponGainedTime")),
                    HungerDelta = reader.GetInt32(reader.GetOrdinal("HungerDelta")),
                    MoodDelta = reader.GetInt32(reader.GetOrdinal("MoodDelta")),
                    StaminaDelta = reader.GetInt32(reader.GetOrdinal("StaminaDelta")),
                    CleanlinessDelta = reader.GetInt32(reader.GetOrdinal("CleanlinessDelta")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    Aborted = reader.GetBoolean(reader.GetOrdinal("Aborted"))
                });
            }
            return games;
        }

        public async Task<MiniGameEntity?> GetMiniGameByIdAsync(int gameId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM MiniGame WHERE PlayId = @PlayId", connection);
            command.Parameters.AddWithValue("@PlayId", gameId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MiniGameEntity
                {
                    PlayId = reader.GetInt32(reader.GetOrdinal("PlayId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    PetId = reader.GetInt32(reader.GetOrdinal("PetId")),
                    Level = reader.GetInt32(reader.GetOrdinal("Level")),
                    MonsterCount = reader.GetInt32(reader.GetOrdinal("MonsterCount")),
                    SpeedMultiplier = reader.GetDecimal(reader.GetOrdinal("SpeedMultiplier")),
                    Result = reader.GetString(reader.GetOrdinal("Result")),
                    ExpGained = reader.GetInt32(reader.GetOrdinal("ExpGained")),
                    ExpGainedTime = reader.GetDateTime(reader.GetOrdinal("ExpGainedTime")),
                    PointsGained = reader.GetInt32(reader.GetOrdinal("PointsGained")),
                    PointsGainedTime = reader.GetDateTime(reader.GetOrdinal("PointsGainedTime")),
                    CouponGained = reader.GetString(reader.GetOrdinal("CouponGained")),
                    CouponGainedTime = reader.GetDateTime(reader.GetOrdinal("CouponGainedTime")),
                    HungerDelta = reader.GetInt32(reader.GetOrdinal("HungerDelta")),
                    MoodDelta = reader.GetInt32(reader.GetOrdinal("MoodDelta")),
                    StaminaDelta = reader.GetInt32(reader.GetOrdinal("StaminaDelta")),
                    CleanlinessDelta = reader.GetInt32(reader.GetOrdinal("CleanlinessDelta")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    Aborted = reader.GetBoolean(reader.GetOrdinal("Aborted"))
                };
            }
            return null;
        }

        public async Task<bool> CreateMiniGameAsync(MiniGameEntity miniGame)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                INSERT INTO MiniGame (UserId, PetId, Level, MonsterCount, SpeedMultiplier, Result,
                    ExpGained, ExpGainedTime, PointsGained, PointsGainedTime, CouponGained, CouponGainedTime,
                    HungerDelta, MoodDelta, StaminaDelta, CleanlinessDelta, StartTime, EndTime, Aborted)
                VALUES (@UserId, @PetId, @Level, @MonsterCount, @SpeedMultiplier, @Result,
                    @ExpGained, @ExpGainedTime, @PointsGained, @PointsGainedTime, @CouponGained, @CouponGainedTime,
                    @HungerDelta, @MoodDelta, @StaminaDelta, @CleanlinessDelta, @StartTime, @EndTime, @Aborted)", connection);

            command.Parameters.AddWithValue("@UserId", miniGame.UserId);
            command.Parameters.AddWithValue("@PetId", miniGame.PetId);
            command.Parameters.AddWithValue("@Level", miniGame.Level);
            command.Parameters.AddWithValue("@MonsterCount", miniGame.MonsterCount);
            command.Parameters.AddWithValue("@SpeedMultiplier", miniGame.SpeedMultiplier);
            command.Parameters.AddWithValue("@Result", miniGame.Result);
            command.Parameters.AddWithValue("@ExpGained", miniGame.ExpGained);
            command.Parameters.AddWithValue("@ExpGainedTime", miniGame.ExpGainedTime);
            command.Parameters.AddWithValue("@PointsGained", miniGame.PointsGained);
            command.Parameters.AddWithValue("@PointsGainedTime", miniGame.PointsGainedTime);
            command.Parameters.AddWithValue("@CouponGained", miniGame.CouponGained);
            command.Parameters.AddWithValue("@CouponGainedTime", miniGame.CouponGainedTime);
            command.Parameters.AddWithValue("@HungerDelta", miniGame.HungerDelta);
            command.Parameters.AddWithValue("@MoodDelta", miniGame.MoodDelta);
            command.Parameters.AddWithValue("@StaminaDelta", miniGame.StaminaDelta);
            command.Parameters.AddWithValue("@CleanlinessDelta", miniGame.CleanlinessDelta);
            command.Parameters.AddWithValue("@StartTime", miniGame.StartTime);
            command.Parameters.AddWithValue("@EndTime", (object?)miniGame.EndTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@Aborted", miniGame.Aborted);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateMiniGameAsync(MiniGameEntity miniGame)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE MiniGame SET
                    UserId = @UserId,
                    PetId = @PetId,
                    Level = @Level,
                    MonsterCount = @MonsterCount,
                    SpeedMultiplier = @SpeedMultiplier,
                    Result = @Result,
                    ExpGained = @ExpGained,
                    ExpGainedTime = @ExpGainedTime,
                    PointsGained = @PointsGained,
                    PointsGainedTime = @PointsGainedTime,
                    CouponGained = @CouponGained,
                    CouponGainedTime = @CouponGainedTime,
                    HungerDelta = @HungerDelta,
                    MoodDelta = @MoodDelta,
                    StaminaDelta = @StaminaDelta,
                    CleanlinessDelta = @CleanlinessDelta,
                    StartTime = @StartTime,
                    EndTime = @EndTime,
                    Aborted = @Aborted
                WHERE PlayId = @PlayId", connection);

            command.Parameters.AddWithValue("@PlayId", miniGame.PlayId);
            command.Parameters.AddWithValue("@UserId", miniGame.UserId);
            command.Parameters.AddWithValue("@PetId", miniGame.PetId);
            command.Parameters.AddWithValue("@Level", miniGame.Level);
            command.Parameters.AddWithValue("@MonsterCount", miniGame.MonsterCount);
            command.Parameters.AddWithValue("@SpeedMultiplier", miniGame.SpeedMultiplier);
            command.Parameters.AddWithValue("@Result", miniGame.Result);
            command.Parameters.AddWithValue("@ExpGained", miniGame.ExpGained);
            command.Parameters.AddWithValue("@ExpGainedTime", miniGame.ExpGainedTime);
            command.Parameters.AddWithValue("@PointsGained", miniGame.PointsGained);
            command.Parameters.AddWithValue("@PointsGainedTime", miniGame.PointsGainedTime);
            command.Parameters.AddWithValue("@CouponGained", miniGame.CouponGained);
            command.Parameters.AddWithValue("@CouponGainedTime", miniGame.CouponGainedTime);
            command.Parameters.AddWithValue("@HungerDelta", miniGame.HungerDelta);
            command.Parameters.AddWithValue("@MoodDelta", miniGame.MoodDelta);
            command.Parameters.AddWithValue("@StaminaDelta", miniGame.StaminaDelta);
            command.Parameters.AddWithValue("@CleanlinessDelta", miniGame.CleanlinessDelta);
            command.Parameters.AddWithValue("@StartTime", miniGame.StartTime);
            command.Parameters.AddWithValue("@EndTime", (object?)miniGame.EndTime ?? DBNull.Value);
            command.Parameters.AddWithValue("@Aborted", miniGame.Aborted);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> DeleteMiniGameAsync(int gameId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("DELETE FROM MiniGame WHERE PlayId = @PlayId", connection);
            command.Parameters.AddWithValue("@PlayId", gameId);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> StartGameAsync(int userId, int petId, string gameType)
        {
            // NOTE: The MiniGame entity doesn't have GameType or SessionID properties
            // Using Level field to store game difficulty instead
            // This method may need redesign to match actual schema
            var miniGame = new MiniGameEntity
            {
                UserId = userId,
                PetId = petId,
                Level = 1, // Default to level 1, could parse from gameType
                MonsterCount = 6, // Default for Stage 1
                SpeedMultiplier = 1.0m,
                Result = "In Progress",
                ExpGained = 0,
                // NOTE: This service uses DateTime.Now for game timing (lines 285-294).
                // These timestamps are stored directly in the database without UTC conversion.
                // This service primarily uses ADO.NET with GETDATE() SQL function which returns server local time.
                // Consider refactoring to use DateTime.UtcNow and timezone conversion for consistency with other services.
                ExpGainedTime = DateTime.Now,
                PointsGained = 0,
                PointsGainedTime = DateTime.Now,
                CouponGained = "None",
                CouponGainedTime = DateTime.Now,
                HungerDelta = 0,
                MoodDelta = 0,
                StaminaDelta = 0,
                CleanlinessDelta = 0,
                StartTime = DateTime.Now,
                EndTime = null,
                Aborted = false
            };

            return await CreateMiniGameAsync(miniGame);
        }

        public async Task<bool> EndGameAsync(int gameId, string gameResult, int pointsEarned, int petExpEarned, int? couponEarned = null)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE MiniGame SET
                    EndTime = GETDATE(),
                    Result = @Result,
                    PointsGained = @PointsGained,
                    ExpGained = @ExpGained,
                    CouponGained = @CouponGained
                WHERE PlayId = @PlayId", connection);

            command.Parameters.AddWithValue("@PlayId", gameId);
            command.Parameters.AddWithValue("@Result", gameResult);
            command.Parameters.AddWithValue("@PointsGained", pointsEarned);
            command.Parameters.AddWithValue("@ExpGained", petExpEarned);
            command.Parameters.AddWithValue("@CouponGained", couponEarned?.ToString() ?? "None");

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> AbortGameAsync(int gameId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE MiniGame SET
                    EndTime = GETDATE(),
                    Result = 'Abort',
                    Aborted = 1
                WHERE PlayId = @PlayId", connection);

            command.Parameters.AddWithValue("@PlayId", gameId);

            var result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<IEnumerable<MiniGameEntity>> GetGamesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var games = new List<MiniGameEntity>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT * FROM MiniGame 
                WHERE StartTime >= @StartDate AND StartTime <= @EndDate
                ORDER BY StartTime DESC", connection);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                games.Add(new MiniGameEntity
                {
                    PlayId = reader.GetInt32(reader.GetOrdinal("PlayId")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    PetId = reader.GetInt32(reader.GetOrdinal("PetId")),
                    Level = reader.GetInt32(reader.GetOrdinal("Level")),
                    MonsterCount = reader.GetInt32(reader.GetOrdinal("MonsterCount")),
                    SpeedMultiplier = reader.GetDecimal(reader.GetOrdinal("SpeedMultiplier")),
                    Result = reader.GetString(reader.GetOrdinal("Result")),
                    ExpGained = reader.GetInt32(reader.GetOrdinal("ExpGained")),
                    ExpGainedTime = reader.GetDateTime(reader.GetOrdinal("ExpGainedTime")),
                    PointsGained = reader.GetInt32(reader.GetOrdinal("PointsGained")),
                    PointsGainedTime = reader.GetDateTime(reader.GetOrdinal("PointsGainedTime")),
                    CouponGained = reader.GetString(reader.GetOrdinal("CouponGained")),
                    CouponGainedTime = reader.GetDateTime(reader.GetOrdinal("CouponGainedTime")),
                    HungerDelta = reader.GetInt32(reader.GetOrdinal("HungerDelta")),
                    MoodDelta = reader.GetInt32(reader.GetOrdinal("MoodDelta")),
                    StaminaDelta = reader.GetInt32(reader.GetOrdinal("StaminaDelta")),
                    CleanlinessDelta = reader.GetInt32(reader.GetOrdinal("CleanlinessDelta")),
                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                    Aborted = reader.GetBoolean(reader.GetOrdinal("Aborted"))
                });
            }
            return games;
        }

        public async Task<Dictionary<string, int>> GetGameStatisticsAsync()
        {
            var statistics = new Dictionary<string, int>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT
                    COUNT(*) as TotalGames,
                    SUM(CASE WHEN Result = 'Win' THEN 1 ELSE 0 END) as WinCount,
                    SUM(CASE WHEN Result = 'Lose' THEN 1 ELSE 0 END) as LoseCount,
                    SUM(CASE WHEN Result = 'Abort' THEN 1 ELSE 0 END) as AbortCount,
                    SUM(PointsGained) as TotalPointsGained,
                    SUM(ExpGained) as TotalExpGained
                FROM MiniGame", connection);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                statistics["TotalGames"] = reader.GetInt32(reader.GetOrdinal("TotalGames"));
                statistics["WinCount"] = reader.GetInt32(reader.GetOrdinal("WinCount"));
                statistics["LoseCount"] = reader.GetInt32(reader.GetOrdinal("LoseCount"));
                statistics["AbortCount"] = reader.GetInt32(reader.GetOrdinal("AbortCount"));
                statistics["TotalPointsEarned"] = reader.GetInt32(reader.GetOrdinal("TotalPointsEarned"));
                statistics["TotalExpEarned"] = reader.GetInt32(reader.GetOrdinal("TotalExpEarned"));
            }
            return statistics;
        }

        public async Task<int> GetTodayGameCountByUserIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT COUNT(*) FROM MiniGame
                WHERE UserId = @UserId
                AND CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE)", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            var result = await command.ExecuteScalarAsync();
            return result != null ? (int)result : 0;
        }

        public async Task<bool> CanUserPlayTodayAsync(int userId)
        {
            var todayGameCount = await GetTodayGameCountByUserIdAsync(userId);
            // Assuming daily limit is 3 games per day (this could be configurable)
            return todayGameCount < 3;
        }
    }
}

