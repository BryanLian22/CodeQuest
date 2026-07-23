// Purpose: Defines the AdminChapterQuizRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class AdminChapterQuizRecord
    {
        public int QuizID { get; set; }
        public int ChapterID { get; set; }
        public string Description { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
        public int AnswerCount { get; set; }
    }
}
