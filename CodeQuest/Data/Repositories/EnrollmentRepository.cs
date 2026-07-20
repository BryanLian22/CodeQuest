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

        public IList<EnrollmentCourseRecord> GetForUser(int userID)
        {
            const string sql = @"
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
