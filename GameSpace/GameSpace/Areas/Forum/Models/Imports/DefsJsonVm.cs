namespace GameSpace.Areas.Forum.Models.Imports
{
    
        public class DefsJsonVm
        {
            public List<MetricJsonVm> Metrics { get; set; } = new();
            public List<MappingJsonVm> Mappings { get; set; } = new();
        }

        public class MetricJsonVm
        {
            public string SourceCode { get; set; } = "";   // 例：Steam
            public string Code { get; set; } = "";         // 例：ccu
            public string Unit { get; set; } = "";         // 例：人
            public string Description { get; set; } = "";  // 例：同時在線
            public bool IsActive { get; set; }             // true/false
        }

        public class MappingJsonVm
        {
            public string GameName { get; set; } = "";     // 例：Elden Ring
            public string SourceCode { get; set; } = "";   // 例：Steam
            public string ExternalKey { get; set; } = "";  // 例：1245620
            public string? ExternalUrl { get; set; }       // 你的 model 沒這欄就先不用
        }
    }

