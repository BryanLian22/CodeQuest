using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Loads a chapter and its matching published tutorial/exercise content.
    /// The ERD keeps Tutorial independent, so seeded content is linked by the
    /// matching tutorial_title and Chapter title.
    /// </summary>
    public sealed class ChapterContentRepository
    {
        public int? GetNextPublishedChapterID(int chapterID)
        {
            return GetNextChapterID(chapterID, false);
        }

        public int? GetNextChapterID(int chapterID, bool includeUnpublished)
        {
            const string sql = @"
                SELECT TOP (1) nextChapter.ChapterID
                FROM dbo.Chapter currentChapter
                INNER JOIN dbo.Module currentModule
                  ON currentModule.ModuleID = currentChapter.ModuleID
                INNER JOIN dbo.Module nextModule
                  ON nextModule.CourseID = currentModule.CourseID
                 AND (@includeUnpublished = 1 OR nextModule.status = N'Published')
                INNER JOIN dbo.Chapter nextChapter
                  ON nextChapter.ModuleID = nextModule.ModuleID
                WHERE currentChapter.ChapterID = @chapterID
                  AND
                  (
                      nextModule.ModuleID > currentModule.ModuleID
                      OR
                      (
                          nextModule.ModuleID = currentModule.ModuleID
                          AND nextChapter.ChapterID > currentChapter.ChapterID
                      )
                  )
                ORDER BY nextModule.ModuleID, nextChapter.ChapterID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                command.Parameters.Add("@includeUnpublished", SqlDbType.Bit).Value = includeUnpublished;
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null || value == System.DBNull.Value
                    ? (int?)null
                    : System.Convert.ToInt32(value);
            }
        }

        public ChapterLessonRecord GetChapter(int chapterID)
        {
            return GetChapter(chapterID, false);
        }

        public ChapterLessonRecord GetChapter(int chapterID, bool includeUnpublished)
        {
            const string chapterSql = @"
                SELECT c.ChapterID, c.ModuleID, m.CourseID, co.course_title,
                       m.module_title, c.title, c.description
                FROM dbo.Chapter c
                INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                INNER JOIN dbo.Course co ON co.CourseID = m.CourseID
                WHERE c.ChapterID = @chapterID
                  AND (@includeUnpublished = 1 OR m.status = N'Published');";

            ChapterLessonRecord lesson = null;

            using (SqlConnection connection = DbConnectionFactory.Create())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(chapterSql, connection))
                {
                    command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                    command.Parameters.Add("@includeUnpublished", SqlDbType.Bit).Value = includeUnpublished;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int descriptionOrdinal = reader.GetOrdinal("description");
                            lesson = new ChapterLessonRecord
                            {
                                ChapterID = reader.GetInt32(reader.GetOrdinal("ChapterID")),
                                ModuleID = reader.GetInt32(reader.GetOrdinal("ModuleID")),
                                CourseID = reader.GetInt32(reader.GetOrdinal("CourseID")),
                                CourseTitle = reader.GetString(reader.GetOrdinal("course_title")),
                                ModuleTitle = reader.GetString(reader.GetOrdinal("module_title")),
                                ChapterTitle = reader.GetString(reader.GetOrdinal("title")),
                                ChapterDescription = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal)
                            };
                        }
                    }
                }

                if (lesson == null)
                {
                    return null;
                }

                const string tutorialSql = @"
                    SELECT TOP (1) TutorialID, tutorial_title, materials
                    FROM dbo.Tutorial
                    WHERE tutorial_title = @chapterTitle
                      AND (@includeUnpublished = 1 OR status = N'Published');";

                using (SqlCommand command = new SqlCommand(tutorialSql, connection))
                {
                    command.Parameters.Add("@chapterTitle", SqlDbType.NVarChar, 200).Value = lesson.ChapterTitle;
                    command.Parameters.Add("@includeUnpublished", SqlDbType.Bit).Value = includeUnpublished;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return lesson;
                        }

                        lesson.TutorialID = reader.GetInt32(reader.GetOrdinal("TutorialID"));
                        lesson.TutorialTitle = reader.GetString(reader.GetOrdinal("tutorial_title"));
                        lesson.Materials = reader.IsDBNull(reader.GetOrdinal("materials")) ? null : reader.GetString(reader.GetOrdinal("materials"));
                    }
                }

                const string exerciseSql = @"
                    SELECT ExerciseID, question, correct_answer
                    FROM dbo.Exercise
                    WHERE TutorialID = @tutorialID
                    ORDER BY ExerciseID;";

                using (SqlCommand command = new SqlCommand(exerciseSql, connection))
                {
                    command.Parameters.Add("@tutorialID", SqlDbType.Int).Value = lesson.TutorialID.Value;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lesson.Exercises.Add(new ExerciseRecord
                            {
                                ExerciseID = reader.GetInt32(reader.GetOrdinal("ExerciseID")),
                                Question = reader.GetString(reader.GetOrdinal("question")),
                                CorrectAnswer = reader.GetString(reader.GetOrdinal("correct_answer"))
                            });
                        }
                    }
                }
            }

            return lesson;
        }
    }
}
