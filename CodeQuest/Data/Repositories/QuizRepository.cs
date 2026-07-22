using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Loads checkpoint questions for a published learner chapter.
    /// </summary>
    public sealed class QuizRepository
    {
        public IList<QuizQuestionRecord> GetForChapter(int chapterID)
        {
            return GetForChapter(chapterID, false);
        }

        public IList<QuizQuestionRecord> GetForChapter(int chapterID, bool includeUnpublished)
        {
            const string sql = @"
                SELECT q.QuizID, q.ChapterID, q.description, q.question, q.correct_answer,
                       a.QAnsID, a.Answer
                FROM dbo.Quiz q
                INNER JOIN dbo.Chapter c ON c.ChapterID = q.ChapterID
                INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                LEFT JOIN dbo.QuizAns a ON a.QuizID = q.QuizID
                WHERE q.ChapterID = @chapterID
                  AND (@includeUnpublished = 1 OR m.status = N'Published')
                ORDER BY q.QuizID, a.QAnsID;";

            List<QuizQuestionRecord> questions = new List<QuizQuestionRecord>();

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                command.Parameters.Add("@includeUnpublished", SqlDbType.Bit).Value = includeUnpublished;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    QuizQuestionRecord current = null;
                    while (reader.Read())
                    {
                        int quizID = reader.GetInt32(reader.GetOrdinal("QuizID"));
                        if (current == null || current.QuizID != quizID)
                        {
                            current = new QuizQuestionRecord
                            {
                                QuizID = quizID,
                                ChapterID = reader.GetInt32(reader.GetOrdinal("ChapterID")),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                    ? null : reader.GetString(reader.GetOrdinal("description")),
                                Question = reader.GetString(reader.GetOrdinal("question")),
                                CorrectAnswer = reader.GetString(reader.GetOrdinal("correct_answer"))
                            };
                            questions.Add(current);
                        }

                        int answerOrdinal = reader.GetOrdinal("QAnsID");
                        if (!reader.IsDBNull(answerOrdinal))
                        {
                            current.Answers.Add(new QuizAnswerRecord
                            {
                                QAnsID = reader.GetInt32(answerOrdinal),
                                Answer = reader.GetString(reader.GetOrdinal("Answer"))
                            });
                        }
                    }
                }
            }

            return questions;
        }

        public bool HasQuiz(int chapterID)
        {
            return HasQuiz(chapterID, false);
        }

        public bool HasQuiz(int chapterID, bool includeUnpublished)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Quiz q
                INNER JOIN dbo.Chapter c ON c.ChapterID = q.ChapterID
                INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                WHERE q.ChapterID = @chapterID
                  AND (@includeUnpublished = 1 OR m.status = N'Published');";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                command.Parameters.Add("@includeUnpublished", SqlDbType.Bit).Value = includeUnpublished;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }
}
