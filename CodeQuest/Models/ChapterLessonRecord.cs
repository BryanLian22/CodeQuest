// Purpose: Defines the ChapterLessonRecord data shape shared between repositories and Web Forms pages.
using System.Collections.Generic;

namespace CodeQuest.Models
{
    public sealed class ChapterLessonRecord
    {
        public int ChapterID { get; set; }
        public int ModuleID { get; set; }
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public string ModuleTitle { get; set; }
        public string ChapterTitle { get; set; }
        public string ChapterDescription { get; set; }
        public int? TutorialID { get; set; }
        public string TutorialTitle { get; set; }
        public string Materials { get; set; }
        public IList<ExerciseRecord> Exercises { get; set; } = new List<ExerciseRecord>();
    }
}
