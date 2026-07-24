// Purpose: Encapsulates parameterized SQL Server operations for Progress data and related transactions.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Persists learner quiz attempts and completed chapter progress.
    /// The automatic database initializer creates the required extension tables.
    /// </summary>
    public sealed class ProgressRepository
    {
        public void RecordQuizAttempt(int userID, int chapterID, int quizID, string selectedAnswer, bool isCorrect)
        {
            const string sql = @"
                INSERT INTO dbo.QuizAttempt(UserID, QuizID, ChapterID, selected_answer, is_correct)
                VALUES (@userID, @quizID, @chapterID, @selectedAnswer, @isCorrect);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@quizID", SqlDbType.Int).Value = quizID;
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                command.Parameters.Add("@selectedAnswer", SqlDbType.NVarChar, 2000).Value =
                    string.IsNullOrWhiteSpace(selectedAnswer) ? (object)DBNull.Value : selectedAnswer;
                command.Parameters.Add("@isCorrect", SqlDbType.Bit).Value = isCorrect;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void MarkChapterCompleted(int userID, int chapterID)
        {
            const string sql = @"
                IF EXISTS
                (
                    SELECT 1
                    FROM dbo.ChapterProgress
                    WHERE UserID = @userID
                      AND ChapterID = @chapterID
                )
                    UPDATE dbo.ChapterProgress
                    SET status = N'Completed', completed_at = SYSUTCDATETIME()
                    WHERE UserID = @userID
                      AND ChapterID = @chapterID
                      AND status <> N'Completed';
                ELSE
                    INSERT INTO dbo.ChapterProgress(UserID, ChapterID, status, completed_at)
                    VALUES (@userID, @chapterID, N'Completed', SYSUTCDATETIME());";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int GetCompletedChapterCount(int userID)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.ChapterProgress
                WHERE UserID = @userID AND status = N'Completed';";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool IsChapterCompleted(int userID, int chapterID)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.ChapterProgress
                WHERE UserID = @userID
                  AND ChapterID = @chapterID
                  AND status = N'Completed';";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public IDictionary<int, string> GetLatestQuizAnswers(int userID, int chapterID)
        {
            const string sql = @"
                WITH LatestAttempts AS
                (
                    SELECT QuizID, selected_answer,
                           ROW_NUMBER() OVER (PARTITION BY QuizID ORDER BY AttemptID DESC) AS row_number
                    FROM dbo.QuizAttempt
                    WHERE UserID = @userID
                      AND ChapterID = @chapterID
                )
                SELECT QuizID, selected_answer
                FROM LatestAttempts
                WHERE row_number = 1;";

            Dictionary<int, string> answers = new Dictionary<int, string>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int selectedAnswerOrdinal = reader.GetOrdinal("selected_answer");
                        answers[reader.GetInt32(reader.GetOrdinal("QuizID"))] =
                            reader.IsDBNull(selectedAnswerOrdinal) ? null : reader.GetString(selectedAnswerOrdinal);
                    }
                }
            }

            return answers;
        }

        public decimal? GetQuizAverage(int userID)
        {
            const string sql = @"
                SELECT AVG(CAST(CASE WHEN is_correct = 1 THEN 100.0 ELSE 0.0 END AS DECIMAL(5,2)))
                FROM dbo.QuizAttempt
                WHERE UserID = @userID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
            }
        }
    }
}
