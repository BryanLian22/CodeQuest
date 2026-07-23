// Purpose: Defines the AdminCourseRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class AdminCourseRecord
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public string OwnerName { get; set; }
        public int ModuleCount { get; set; }
    }
}
