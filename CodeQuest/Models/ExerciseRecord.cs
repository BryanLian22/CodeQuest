// Purpose: Defines the ExerciseRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class ExerciseRecord
    {
        public int ExerciseID { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
