namespace CodeQuest.Models
{
    public sealed class AdminExerciseRecord
    {
        public int ExerciseID { get; set; }
        public int TutorialID { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
