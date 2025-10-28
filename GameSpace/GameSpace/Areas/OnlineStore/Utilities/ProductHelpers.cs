namespace GameSpace.Areas.OnlineStore.Utilities
{
    public static class ProductHelpers
    {
        public static string NormalizeType(string? t)
        {
            if (string.IsNullOrWhiteSpace(t)) return "";
            t = t.Trim().ToLowerInvariant();
            if (t == "nogame") t = "notgame";
            return (t == "game" || t == "notgame") ? t : "";
        }

        public static int ParseCodeNumber(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return int.MaxValue;
            var s = new string(code.Trim()
                .SkipWhile(ch => !char.IsDigit(ch))
                .TakeWhile(char.IsDigit)
                .ToArray());
            return int.TryParse(s, out var n) ? n : int.MaxValue;
        }
    }
}


