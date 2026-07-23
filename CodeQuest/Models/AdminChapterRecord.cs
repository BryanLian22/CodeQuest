// Purpose: Defines the AdminChapterRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class AdminChapterRecord
    {
        public int ChapterID { get; set; }
        public int ModuleID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
