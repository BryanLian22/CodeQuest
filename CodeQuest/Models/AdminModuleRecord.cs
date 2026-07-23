// Purpose: Defines the AdminModuleRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class AdminModuleRecord
    {
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ChapterCount { get; set; }
    }
}
