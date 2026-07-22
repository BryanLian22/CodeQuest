namespace CodeQuest.Models
{
    public sealed class ChapterRecord
    {
        public int ChapterID { get; set; }
        public int ModuleID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
