using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Reads learner enrollments through User -> Enrollment -> Course.
    /// </summary>
    public sealed class EnrollmentRepository
    {
        public ISet<int> GetCourseIDsForUser(int userID)
        {
            const string sql = @"
                SELECT CourseID
                FROM dbo.Enrollment
                WHERE UserID = @userID AND status <> N'Cancelled';";

            HashSet<int> courseIDs = new HashSet<int>();

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courseIDs.Add(reader.GetInt32(reader.GetOrdinal("CourseID")));
                    }
                }
            }

            return courseIDs;
        }

        public bool IsEnrolled(int userID, int courseID)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Enrollment
                WHERE UserID = @userID AND CourseID = @courseID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@courseID", SqlDbType.Int).Value = courseID;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public int CreateEnrollment(int userID, int courseID)
        {
            const string sql = @"
                INSERT INTO dbo.Enrollment(UserID, CourseID, status)
                VALUES (@userID, @courseID, N'Active');
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@courseID", SqlDbType.Int).Value = courseID;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// Marks an enrollment completed once every chapter inside the course's
        /// published modules has a completed ChapterProgress row.
        /// </summary>
        public bool CompleteCourseIfReady(int userID, int courseID)
        {
            const string sql = @"
                DECLARE @publishedChapterCount INT;
                DECLARE @completedChapterCount INT;

                SELECT @publishedChapterCount = COUNT(1)
                FROM dbo.Chapter c
                INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                WHERE m.CourseID = @courseID
                  AND m.status = N'Published';

                SELECT @completedChapterCount = COUNT(DISTINCT cp.ChapterID)
                FROM dbo.ChapterProgress cp
                INNER JOIN dbo.Chapter c ON c.ChapterID = cp.ChapterID
                INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                WHERE cp.UserID = @userID
                  AND cp.status = N'Completed'
                  AND m.CourseID = @courseID
                  AND m.status = N'Published';

                IF @publishedChapterCount > 0
                   AND @completedChapterCount = @publishedChapterCount
                   AND EXISTS
                   (
                       SELECT 1
                       FROM dbo.Enrollment
                       WHERE UserID = @userID
                         AND CourseID = @courseID
                         AND status <> N'Cancelled'
                   )
                BEGIN
                    UPDATE dbo.Enrollment
                    SET status = N'Completed'
                    WHERE UserID = @userID
                      AND CourseID = @courseID
                      AND status <> N'Cancelled';

                    SELECT CAST(1 AS BIT);
                END
                ELSE
                    SELECT CAST(0 AS BIT);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@courseID", SqlDbType.Int).Value = courseID;
                connection.Open();
                return Convert.ToBoolean(command.ExecuteScalar());
            }
        }

        public IList<EnrollmentCourseRecord> GetForUser(int userID)
        {
            const string sql = @"
                SET NOCOUNT ON;

                UPDATE e
                SET status = CASE
                    WHEN NOT EXISTS
                    (
                        SELECT 1
                        FROM dbo.Chapter c
                        INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                        LEFT JOIN dbo.ChapterProgress cp
                          ON cp.ChapterID = c.ChapterID
                         AND cp.UserID = e.UserID
                         AND cp.status = N'Completed'
                        WHERE m.CourseID = e.CourseID
                          AND m.status = N'Published'
                          AND cp.ProgressID IS NULL
                    ) THEN N'Completed'
                    ELSE N'Active'
                END
                FROM dbo.Enrollment e
                WHERE e.UserID = @userID
                  AND e.status <> N'Cancelled'
                  AND EXISTS
                  (
                      SELECT 1
                      FROM dbo.Chapter c
                      INNER JOIN dbo.Module m ON m.ModuleID = c.ModuleID
                      WHERE m.CourseID = e.CourseID
                        AND m.status = N'Published'
                  );

                SELECT e.EID, c.CourseID, c.course_title, c.difficulty, e.status
                FROM dbo.Enrollment e
                INNER JOIN dbo.Course c ON c.CourseID = e.CourseID
                WHERE e.UserID = @userID
                ORDER BY e.EID DESC;";

            List<EnrollmentCourseRecord> enrollments = new List<EnrollmentCourseRecord>();

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        enrollments.Add(new EnrollmentCourseRecord
                        {
                            EnrollmentID = reader.GetInt32(reader.GetOrdinal("EID")),
                            CourseID = reader.GetInt32(reader.GetOrdinal("CourseID")),
                            CourseTitle = reader.GetString(reader.GetOrdinal("course_title")),
                            Difficulty = reader.GetString(reader.GetOrdinal("difficulty")),
                            Status = reader.GetString(reader.GetOrdinal("status"))
                        });
                    }
                }
            }

            return enrollments;
        }
    }
}
