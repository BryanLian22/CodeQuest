// Purpose: Encapsulates parameterized SQL Server operations for User data and related transactions.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Database operations for dbo.User, including local credentials, Google
    /// account linking, profile maintenance and administrator account updates.
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

                    return ReadUser(reader);
                }
            }
        }

        public UserRecord FindByGoogleID(string googleID)
        {
            const string sql = @"
                SELECT TOP (1) UserID, username, [password], email, bio,
                                role, [plan], google_id
                FROM dbo.[User]
                WHERE google_id = @googleID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@googleID", SqlDbType.NVarChar, 255).Value = googleID.Trim();
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return reader.Read() ? ReadUser(reader) : null;
                }
            }
        }

        public UserRecord FindByID(int userID)
        {
            const string sql = @"
                SELECT TOP (1) UserID, username, [password], email, bio,
                                role, [plan], google_id
                FROM dbo.[User]
                WHERE UserID = @userID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return reader.Read() ? ReadUser(reader) : null;
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

        public int CreateGoogleLearner(string username, string passwordHash, string email, string googleID)
        {
            const string sql = @"
                INSERT INTO dbo.[User](username, [password], email, role, [plan], google_id)
                OUTPUT INSERTED.UserID
                VALUES (@username, @password, @email, N'Learner', N'Basic', @googleID);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Trim();
                command.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = passwordHash;
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email.Trim();
                command.Parameters.Add("@googleID", SqlDbType.NVarChar, 255).Value = googleID.Trim();
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool LinkGoogleID(int userID, string googleID)
        {
            const string sql = @"
                UPDATE dbo.[User]
                SET google_id = @googleID
                WHERE UserID = @userID
                  AND (google_id IS NULL OR google_id = @googleID);";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@googleID", SqlDbType.NVarChar, 255).Value = googleID.Trim();
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public bool IsUsernameAvailable(string username)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.[User] WHERE username = @username;";
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Trim();
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) == 0;
            }
        }

        public bool UpdateProfile(int userID, string username, string bio)
        {
            const string sql = @"
                UPDATE dbo.[User]
                SET username = @username,
                    bio = @bio
                WHERE UserID = @userID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = username.Trim();
                command.Parameters.Add("@bio", SqlDbType.NVarChar, 1000).Value =
                    string.IsNullOrWhiteSpace(bio) ? (object)DBNull.Value : bio.Trim();
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public bool UpdateLearnerEmail(int userID, string email)
        {
            const string sql = @"
                UPDATE dbo.[User]
                SET email = @email
                WHERE UserID = @userID
                  AND role = N'Learner'
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.[User] otherUser
                      WHERE otherUser.email = @email
                        AND otherUser.UserID <> @userID
                  );";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email.Trim();
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public bool UpdatePassword(int userID, string passwordHash)
        {
            const string sql = @"
                UPDATE dbo.[User]
                SET [password] = @password
                WHERE UserID = @userID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = passwordHash;
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        public IList<UserManagementRecord> GetUsers(string search)
        {
            const string sql = @"
                SELECT u.UserID, u.username, u.email, u.bio, u.role, u.[plan], u.google_id,
                       COUNT(DISTINCT e.EID) AS EnrollmentCount,
                       COUNT(DISTINCT t.TicketID) AS TicketCount
                FROM dbo.[User] u
                LEFT JOIN dbo.Enrollment e ON e.UserID = u.UserID
                LEFT JOIN dbo.Ticket t ON t.UserID = u.UserID
                WHERE @search = N''
                   OR u.username LIKE N'%' + @search + N'%'
                   OR u.email LIKE N'%' + @search + N'%'
                GROUP BY u.UserID, u.username, u.email, u.bio, u.role, u.[plan], u.google_id
                ORDER BY CASE WHEN u.role = N'Admin' THEN 0 ELSE 1 END, u.username;";

            List<UserManagementRecord> users = new List<UserManagementRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@search", SqlDbType.NVarChar, 254).Value = (search ?? string.Empty).Trim();
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(ReadManagedUser(reader));
                    }
                }
            }

            return users;
        }

        public UserManagementRecord GetManagedUser(int userID)
        {
            const string sql = @"
                SELECT u.UserID, u.username, u.email, u.bio, u.role, u.[plan], u.google_id,
                       COUNT(DISTINCT e.EID) AS EnrollmentCount,
                       COUNT(DISTINCT t.TicketID) AS TicketCount
                FROM dbo.[User] u
                LEFT JOIN dbo.Enrollment e ON e.UserID = u.UserID
                LEFT JOIN dbo.Ticket t ON t.UserID = u.UserID
                WHERE u.UserID = @userID
                GROUP BY u.UserID, u.username, u.email, u.bio, u.role, u.[plan], u.google_id;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return reader.Read() ? ReadManagedUser(reader) : null;
                }
            }
        }

        public bool UpdateAccess(int userID, string role, string plan)
        {
            const string sql = @"
                DECLARE @updatedRows INT;

                UPDATE dbo.[User]
                SET role = @role,
                    [plan] = @plan
                WHERE UserID = @userID;

                SET @updatedRows = @@ROWCOUNT;

                IF @plan = N'Basic'
                BEGIN
                    UPDATE dbo.Subscription
                    SET status = N'Cancelled',
                        end_date = CASE
                            WHEN start_date > CAST(GETDATE() AS date) THEN start_date
                            ELSE CAST(GETDATE() AS date)
                        END
                    WHERE UserID = @userID
                      AND plan_type = N'Premium'
                      AND status = N'Active';
                END;

                SELECT @updatedRows;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@role", SqlDbType.NVarChar, 20).Value = role.Trim();
                command.Parameters.Add("@plan", SqlDbType.NVarChar, 20).Value = plan.Trim();
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        bool updated = Convert.ToInt32(command.ExecuteScalar()) == 1;
                        transaction.Commit();
                        return updated;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public int GetAdminCount()
        {
            const string sql = "SELECT COUNT(1) FROM dbo.[User] WHERE role = N'Admin';";
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static UserRecord ReadUser(SqlDataReader reader)
        {
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

        private static UserManagementRecord ReadManagedUser(SqlDataReader reader)
        {
            return new UserManagementRecord
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Username = reader.GetString(reader.GetOrdinal("username")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Bio = reader.IsDBNull(reader.GetOrdinal("bio")) ? null : reader.GetString(reader.GetOrdinal("bio")),
                Role = reader.GetString(reader.GetOrdinal("role")),
                Plan = reader.GetString(reader.GetOrdinal("plan")),
                GoogleID = reader.IsDBNull(reader.GetOrdinal("google_id")) ? null : reader.GetString(reader.GetOrdinal("google_id")),
                EnrollmentCount = reader.GetInt32(reader.GetOrdinal("EnrollmentCount")),
                TicketCount = reader.GetInt32(reader.GetOrdinal("TicketCount"))
            };
        }
    }
}
