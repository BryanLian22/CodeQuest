// Purpose: Defines the AdminTutorialRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class AdminTutorialRecord
    {
        public int TutorialID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Materials { get; set; }
        public int ExerciseCount { get; set; }
    }
}
