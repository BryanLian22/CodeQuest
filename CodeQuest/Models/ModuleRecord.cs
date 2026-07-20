using System.Collections.Generic;

namespace CodeQuest.Models
{
    public sealed class ModuleRecord
    {
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public IList<ChapterRecord> Chapters { get; set; } = new List<ChapterRecord>();
    }
}
