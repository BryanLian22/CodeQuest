// Purpose: Defines the AdminContentSummary data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class AdminContentSummary
    {
        public int Courses { get; set; }
        public int Modules { get; set; }
        public int Chapters { get; set; }
        public int Tutorials { get; set; }
        public int Exercises { get; set; }
        public int Quizzes { get; set; }
    }
}
