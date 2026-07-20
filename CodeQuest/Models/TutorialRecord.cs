using System.Collections.Generic;

namespace CodeQuest.Models
{
    public sealed class TutorialRecord
    {
        public int TutorialID { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Materials { get; set; }
        public IList<ExerciseRecord> Exercises { get; set; } = new List<ExerciseRecord>();
    }
}
