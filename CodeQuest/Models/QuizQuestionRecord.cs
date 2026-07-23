// Purpose: Defines the QuizQuestionRecord data shape shared between repositories and Web Forms pages.
using System.Collections.Generic;

namespace CodeQuest.Models
{
    public sealed class QuizQuestionRecord
    {
        public int QuizID { get; set; }
        public int ChapterID { get; set; }
        public string Description { get; set; }
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
        public IList<QuizAnswerRecord> Answers { get; set; } = new List<QuizAnswerRecord>();
    }
}
