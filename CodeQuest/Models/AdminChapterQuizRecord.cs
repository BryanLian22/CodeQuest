namespace CodeQuest.Models
{
    public sealed class AdminChapterQuizRecord
    {
        public int QuizID { get; set; }
        public int ChapterID { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
        public int AnswerCount { get; set; }
    }
}
