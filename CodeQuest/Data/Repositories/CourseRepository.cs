// Purpose: Encapsulates parameterized SQL Server operations for Course data and related transactions.
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Read operations for the public course catalogue.
    /// </summary>
    public sealed class CourseRepository
    {
        public IList<CourseRecord> GetAllCourses()
        {
            const string sql = @"
                SELECT CourseID, UserID, course_title, description, difficulty
                FROM dbo.Course
                ORDER BY course_title;";

            List<CourseRecord> courses = new List<CourseRecord>();

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new CourseRecord
                        {
                            CourseID = reader.GetInt32(reader.GetOrdinal("CourseID")),
                            OwnerUserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            Title = reader.GetString(reader.GetOrdinal("course_title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                            Difficulty = reader.GetString(reader.GetOrdinal("difficulty"))
                        });
                    }
                }
            }

            return courses;
        }

        public CourseRecord GetByID(int courseID)
        {
            const string sql = @"
                SELECT CourseID, UserID, course_title, description, difficulty
                FROM dbo.Course
                WHERE CourseID = @courseID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@courseID", SqlDbType.Int).Value = courseID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    int descriptionOrdinal = reader.GetOrdinal("description");
                    return new CourseRecord
                    {
                        CourseID = reader.GetInt32(reader.GetOrdinal("CourseID")),
                        OwnerUserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Title = reader.GetString(reader.GetOrdinal("course_title")),
                        Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                        Difficulty = reader.GetString(reader.GetOrdinal("difficulty"))
                    };
                }
            }
        }
    }
}
