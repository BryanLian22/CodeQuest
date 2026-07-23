// Purpose: Defines the QuizAnswerRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    public sealed class QuizAnswerRecord
    {
        public int QAnsID { get; set; }
        public string Answer { get; set; }
    }
}
