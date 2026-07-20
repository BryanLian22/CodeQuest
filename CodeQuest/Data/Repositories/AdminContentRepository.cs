using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Read-only overview queries for the protected admin workspace.
    /// Create/edit pages can build on this repository without putting SQL in
    /// the admin page event handlers.
    /// </summary>
    public sealed class AdminContentRepository
    {
        public AdminContentSummary GetSummary()
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(1) FROM dbo.Course) AS Courses,
                    (SELECT COUNT(1) FROM dbo.Module) AS Modules,
                    (SELECT COUNT(1) FROM dbo.Chapter) AS Chapters,
                    (SELECT COUNT(1) FROM dbo.Tutorial WHERE status = N'Published') AS Tutorials,
                    (SELECT COUNT(1) FROM dbo.Exercise) AS Exercises,
                    (SELECT COUNT(1) FROM dbo.Quiz) AS Quizzes;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return new AdminContentSummary();
                    }

                    return new AdminContentSummary
                    {
                        Courses = Convert.ToInt32(reader["Courses"]),
                        Modules = Convert.ToInt32(reader["Modules"]),
                        Chapters = Convert.ToInt32(reader["Chapters"]),
                        Tutorials = Convert.ToInt32(reader["Tutorials"]),
                        Exercises = Convert.ToInt32(reader["Exercises"]),
                        Quizzes = Convert.ToInt32(reader["Quizzes"])
                    };
                }
            }
        }

        public IList<AdminCourseRecord> GetCourses()
        {
            const string sql = @"
                SELECT TOP (12) c.CourseID, c.course_title, c.difficulty,
                       u.username,
                       COUNT(m.ModuleID) AS ModuleCount
                FROM dbo.Course c
                INNER JOIN dbo.[User] u ON u.UserID = c.UserID
                LEFT JOIN dbo.Module m ON m.CourseID = c.CourseID
                GROUP BY c.CourseID, c.course_title, c.difficulty, u.username
                ORDER BY c.CourseID DESC;";

            List<AdminCourseRecord> courses = new List<AdminCourseRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new AdminCourseRecord
                        {
                            CourseID = Convert.ToInt32(reader["CourseID"]),
                            Title = Convert.ToString(reader["course_title"]),
                            Difficulty = Convert.ToString(reader["difficulty"]),
                            OwnerName = Convert.ToString(reader["username"]),
                            ModuleCount = Convert.ToInt32(reader["ModuleCount"])
                        });
                    }
                }
            }

            return courses;
        }

        public IList<AdminModuleRecord> GetModules(int courseID)
        {
            const string sql = @"
                SELECT m.ModuleID, m.CourseID, m.module_title, m.description, m.status,
                       COUNT(c.ChapterID) AS ChapterCount
                FROM dbo.Module m
                LEFT JOIN dbo.Chapter c ON c.ModuleID = m.ModuleID
                WHERE m.CourseID = @courseID
                GROUP BY m.ModuleID, m.CourseID, m.module_title, m.description, m.status
                ORDER BY m.ModuleID;";

            List<AdminModuleRecord> modules = new List<AdminModuleRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@courseID", SqlDbType.Int).Value = courseID;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        modules.Add(new AdminModuleRecord
                        {
                            ModuleID = Convert.ToInt32(reader["ModuleID"]),
                            CourseID = Convert.ToInt32(reader["CourseID"]),
                            Title = Convert.ToString(reader["module_title"]),
                            Description = reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"]),
                            Status = Convert.ToString(reader["status"]),
                            ChapterCount = Convert.ToInt32(reader["ChapterCount"])
                        });
                    }
                }
            }

            return modules;
        }

        public IList<AdminChapterRecord> GetChapters(int moduleID)
        {
            const string sql = @"
                SELECT ChapterID, ModuleID, title, description
                FROM dbo.Chapter
                WHERE ModuleID = @moduleID
                ORDER BY ChapterID;";

            List<AdminChapterRecord> chapters = new List<AdminChapterRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@moduleID", SqlDbType.Int).Value = moduleID;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        chapters.Add(new AdminChapterRecord
                        {
                            ChapterID = Convert.ToInt32(reader["ChapterID"]),
                            ModuleID = Convert.ToInt32(reader["ModuleID"]),
                            Title = Convert.ToString(reader["title"]),
                            Description = reader["description"] == DBNull.Value ? null : Convert.ToString(reader["description"])
                        });
                    }
                }
            }

            return chapters;
        }

        public int CreateCourse(int ownerUserID, string title, string description, string difficulty)
        {
            const string sql = @"
                INSERT INTO dbo.Course(UserID, course_title, description, difficulty)
                OUTPUT INSERTED.CourseID
                VALUES (@userID, @title, @description, @difficulty);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = ownerUserID;
                command.Parameters.Add("@title", SqlDbType.NVarChar, 150).Value = title.Trim();
                command.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value =
                    string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();
                command.Parameters.Add("@difficulty", SqlDbType.NVarChar, 20).Value = difficulty;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int CreateModule(int courseID, string title, string description, string status)
        {
            const string sql = @"
                INSERT INTO dbo.Module(CourseID, module_title, description, status)
                OUTPUT INSERTED.ModuleID
                VALUES (@courseID, @title, @description, @status);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@courseID", SqlDbType.Int).Value = courseID;
                command.Parameters.Add("@title", SqlDbType.NVarChar, 150).Value = title.Trim();
                command.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value =
                    string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int CreateChapter(int moduleID, string title, string description)
        {
            const string sql = @"
                INSERT INTO dbo.Chapter(ModuleID, title, description)
                OUTPUT INSERTED.ChapterID
                VALUES (@moduleID, @title, @description);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@moduleID", SqlDbType.Int).Value = moduleID;
                command.Parameters.Add("@title", SqlDbType.NVarChar, 150).Value = title.Trim();
                command.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value =
                    string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateModuleStatus(int moduleID, string status)
        {
            const string sql = @"
                UPDATE dbo.Module
                SET status = @status
                WHERE ModuleID = @moduleID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@moduleID", SqlDbType.Int).Value = moduleID;
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public IList<AdminTutorialRecord> GetTutorialsForAdmin()
        {
            const string sql = @"
                SELECT t.TutorialID, t.tutorial_title, t.category, t.status, t.materials,
                       COUNT(e.ExerciseID) AS ExerciseCount
                FROM dbo.Tutorial t
                LEFT JOIN dbo.Exercise e ON e.TutorialID = t.TutorialID
                GROUP BY t.TutorialID, t.tutorial_title, t.category, t.status, t.materials
                ORDER BY t.TutorialID DESC;";

            List<AdminTutorialRecord> tutorials = new List<AdminTutorialRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tutorials.Add(new AdminTutorialRecord
                        {
                            TutorialID = Convert.ToInt32(reader["TutorialID"]),
                            Title = Convert.ToString(reader["tutorial_title"]),
                            Category = reader["category"] == DBNull.Value ? "HTML" : Convert.ToString(reader["category"]),
                            Status = Convert.ToString(reader["status"]),
                            Materials = reader["materials"] == DBNull.Value ? null : Convert.ToString(reader["materials"]),
                            ExerciseCount = Convert.ToInt32(reader["ExerciseCount"])
                        });
                    }
                }
            }

            return tutorials;
        }

        public IList<AdminExerciseRecord> GetExercisesForTutorial(int tutorialID)
        {
            const string sql = @"
                SELECT ExerciseID, TutorialID, question, correct_answer
                FROM dbo.Exercise
                WHERE TutorialID = @tutorialID
                ORDER BY ExerciseID;";

            List<AdminExerciseRecord> exercises = new List<AdminExerciseRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@tutorialID", SqlDbType.Int).Value = tutorialID;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exercises.Add(new AdminExerciseRecord
                        {
                            ExerciseID = Convert.ToInt32(reader["ExerciseID"]),
                            TutorialID = Convert.ToInt32(reader["TutorialID"]),
                            Question = Convert.ToString(reader["question"]),
                            CorrectAnswer = Convert.ToString(reader["correct_answer"])
                        });
                    }
                }
            }

            return exercises;
        }

        public IList<AdminChapterOptionRecord> GetChapterOptions()
        {
            const string sql = @"
                SELECT c.ChapterID, m.CourseID, c.title, m.module_title, co.course_title
                FROM dbo.Chapter c
                INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                INNER JOIN dbo.Course co ON co.CourseID = m.CourseID
                ORDER BY co.CourseID, m.ModuleID, c.ChapterID;";

            List<AdminChapterOptionRecord> chapters = new List<AdminChapterOptionRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        chapters.Add(new AdminChapterOptionRecord
                        {
                            ChapterID = Convert.ToInt32(reader["ChapterID"]),
                            CourseID = Convert.ToInt32(reader["CourseID"]),
                            ChapterTitle = Convert.ToString(reader["title"]),
                            ModuleTitle = Convert.ToString(reader["module_title"]),
                            CourseTitle = Convert.ToString(reader["course_title"])
                        });
                    }
                }
            }

            return chapters;
        }

        public IList<AdminChapterQuizRecord> GetQuizzesForChapter(int chapterID)
        {
            const string sql = @"
                SELECT q.QuizID, q.ChapterID, q.question, q.correct_answer,
                       COUNT(a.QAnsID) AS AnswerCount
                FROM dbo.Quiz q
                LEFT JOIN dbo.QuizAns a ON a.QuizID = q.QuizID
                WHERE q.ChapterID = @chapterID
                GROUP BY q.QuizID, q.ChapterID, q.question, q.correct_answer
                ORDER BY q.QuizID;";

            List<AdminChapterQuizRecord> quizzes = new List<AdminChapterQuizRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        quizzes.Add(new AdminChapterQuizRecord
                        {
                            QuizID = Convert.ToInt32(reader["QuizID"]),
                            ChapterID = Convert.ToInt32(reader["ChapterID"]),
                            Question = Convert.ToString(reader["question"]),
                            CorrectAnswer = Convert.ToString(reader["correct_answer"]),
                            AnswerCount = Convert.ToInt32(reader["AnswerCount"])
                        });
                    }
                }
            }

            return quizzes;
        }

        public int CreateTutorial(string title, string category, string materials, string status)
        {
            const string sql = @"
                INSERT INTO dbo.Tutorial(tutorial_title, category, materials, status)
                OUTPUT INSERTED.TutorialID
                VALUES (@title, @category, @materials, @status);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@title", SqlDbType.NVarChar, 200).Value = title.Trim();
                command.Parameters.Add("@category", SqlDbType.NVarChar, 30).Value = category;
                command.Parameters.Add("@materials", SqlDbType.NVarChar, -1).Value =
                    string.IsNullOrWhiteSpace(materials) ? (object)DBNull.Value : materials.Trim();
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int CreateExercise(int tutorialID, string question, string correctAnswer)
        {
            const string sql = @"
                INSERT INTO dbo.Exercise(TutorialID, question, correct_answer)
                OUTPUT INSERTED.ExerciseID
                VALUES (@tutorialID, @question, @correctAnswer);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@tutorialID", SqlDbType.Int).Value = tutorialID;
                command.Parameters.Add("@question", SqlDbType.NVarChar, -1).Value = question.Trim();
                command.Parameters.Add("@correctAnswer", SqlDbType.NVarChar, 2000).Value = correctAnswer.Trim();
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateTutorialStatus(int tutorialID, string status)
        {
            const string sql = @"
                UPDATE dbo.Tutorial SET status = @status WHERE TutorialID = @tutorialID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@tutorialID", SqlDbType.Int).Value = tutorialID;
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = status;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int CreateQuiz(int chapterID, string description, string question, string correctAnswer, IList<string> answers)
        {
            const string quizSql = @"
                INSERT INTO dbo.Quiz(ChapterID, description, question, correct_answer)
                OUTPUT INSERTED.QuizID
                VALUES (@chapterID, @description, @question, @correctAnswer);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int quizID;
                        using (SqlCommand command = new SqlCommand(quizSql, connection, transaction))
                        {
                            command.Parameters.Add("@chapterID", SqlDbType.Int).Value = chapterID;
                            command.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value =
                                string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();
                            command.Parameters.Add("@question", SqlDbType.NVarChar, -1).Value = question.Trim();
                            command.Parameters.Add("@correctAnswer", SqlDbType.NVarChar, 2000).Value = correctAnswer.Trim();
                            quizID = Convert.ToInt32(command.ExecuteScalar());
                        }

                        const string answerSql = @"
                            INSERT INTO dbo.QuizAns(QuizID, Answer)
                            VALUES (@quizID, @answer);";
                        foreach (string answer in answers)
                        {
                            using (SqlCommand command = new SqlCommand(answerSql, connection, transaction))
                            {
                                command.Parameters.Add("@quizID", SqlDbType.Int).Value = quizID;
                                command.Parameters.Add("@answer", SqlDbType.NVarChar, 2000).Value = answer.Trim();
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return quizID;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
