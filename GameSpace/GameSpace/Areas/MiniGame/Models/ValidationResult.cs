namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 驗證結果類別
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 新增錯誤訊息
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        public void AddError(string message)
        {
            Errors.Add(message);
            IsValid = false;
        }
    }
}

