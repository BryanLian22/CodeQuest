using System;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Database operations for dbo.User. The login page can be switched from
    /// demo accounts to these methods after the database is connected.
    /// </summary>
    public sealed class UserRepository
    {
        public UserRecord FindByEmail(string email)
        {
            const string sql = @"
                SELECT TOP (1) UserID, username, [password], email, bio,
                                role, [plan], google_id
                FROM dbo.[User]
                WHERE email = @email;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email.Trim();
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new UserRecord
                    {
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Username = reader.GetString(reader.GetOrdinal("username")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("password")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        Bio = reader.IsDBNull(reader.GetOrdinal("bio")) ? null : reader.GetString(reader.GetOrdinal("bio")),
                        Role = reader.GetString(reader.GetOrdinal("role")),
                        Plan = reader.GetString(reader.GetOrdinal("plan")),
                        GoogleID = reader.IsDBNull(reader.GetOrdinal("google_id")) ? null : reader.GetString(reader.GetOrdinal("google_id"))
                    };
                }
            }
        }

        public int CreateLearner(string username, string passwordHash, string email)
        {
            const string sql = @"
                INSERT INTO dbo.[User](username, [password], email, role, [plan])
                OUTPUT INSERTED.UserID
                VALUES (@username, @password, @email, N'Learner', N'Basic');";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Trim();
                command.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = passwordHash;
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email.Trim();
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
