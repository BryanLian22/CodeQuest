// Purpose: Encapsulates parameterized SQL Server operations for PasswordReset data and related transactions.
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using CodeQuest.Data;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// One-time password reset tokens. Only a SHA-256 token digest is stored
    /// in dbo.Token; the raw value is placed in the reset URL and never saved.
    /// </summary>
    public sealed class PasswordResetRepository
    {
        private const string TokenType = "PasswordReset";

        public PasswordResetIssue Create(string email, TimeSpan lifetime)
        {
            const string findSql = @"
                SELECT TOP (1) UserID, email
                FROM dbo.[User]
                WHERE email = @email;";

            string rawToken = CreateRawToken();
            string tokenHash = HashToken(rawToken);
            DateTime expiresAt = DateTime.UtcNow.Add(lifetime);

            using (SqlConnection connection = DbConnectionFactory.Create())
            {
                connection.Open();
                using (SqlCommand find = new SqlCommand(findSql, connection))
                {
                    find.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = (email ?? string.Empty).Trim();
                    using (SqlDataReader reader = find.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        int userID = reader.GetInt32(reader.GetOrdinal("UserID"));
                        string accountEmail = reader.GetString(reader.GetOrdinal("email"));
                        reader.Close();

                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                using (SqlCommand invalidate = new SqlCommand(@"
                                    UPDATE dbo.Token
                                    SET used = 1
                                    WHERE UserID = @userID AND token_type = @tokenType AND used = 0;", connection, transaction))
                                {
                                    invalidate.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                                    invalidate.Parameters.Add("@tokenType", SqlDbType.NVarChar, 30).Value = TokenType;
                                    invalidate.ExecuteNonQuery();
                                }

                                using (SqlCommand insert = new SqlCommand(@"
                                    INSERT INTO dbo.Token(UserID, token_type, token, expires_at, used)
                                    VALUES (@userID, @tokenType, @token, @expiresAt, 0);", connection, transaction))
                                {
                                    insert.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                                    insert.Parameters.Add("@tokenType", SqlDbType.NVarChar, 30).Value = TokenType;
                                    insert.Parameters.Add("@token", SqlDbType.NVarChar, 255).Value = tokenHash;
                                    insert.Parameters.Add("@expiresAt", SqlDbType.DateTime2).Value = expiresAt;
                                    insert.ExecuteNonQuery();
                                }

                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }

                        return new PasswordResetIssue
                        {
                            UserID = userID,
                            Email = accountEmail,
                            RawToken = rawToken,
                            ExpiresAt = expiresAt
                        };
                    }
                }
            }
        }

        public PasswordResetTarget FindValid(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return null;
            }

            const string sql = @"
                SELECT TOP (1) t.TokenID, t.UserID, u.email, t.expires_at
                FROM dbo.Token t
                INNER JOIN dbo.[User] u ON u.UserID = t.UserID
                WHERE t.token_type = @tokenType
                  AND t.token = @token
                  AND t.used = 0
                  AND t.expires_at > SYSUTCDATETIME();";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@tokenType", SqlDbType.NVarChar, 30).Value = TokenType;
                command.Parameters.Add("@token", SqlDbType.NVarChar, 255).Value = HashToken(rawToken);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new PasswordResetTarget
                    {
                        TokenID = reader.GetInt32(reader.GetOrdinal("TokenID")),
                        UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at"))
                    };
                }
            }
        }

        public bool ResetPassword(string rawToken, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(rawToken) || string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            const string selectSql = @"
                SELECT TOP (1) TokenID, UserID
                FROM dbo.Token WITH (UPDLOCK, ROWLOCK)
                WHERE token_type = @tokenType
                  AND token = @token
                  AND used = 0
                  AND expires_at > SYSUTCDATETIME();";

            using (SqlConnection connection = DbConnectionFactory.Create())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int tokenID = 0;
                        int userID = 0;
                        bool found;
                        using (SqlCommand select = new SqlCommand(selectSql, connection, transaction))
                        {
                            select.Parameters.Add("@tokenType", SqlDbType.NVarChar, 30).Value = TokenType;
                            select.Parameters.Add("@token", SqlDbType.NVarChar, 255).Value = HashToken(rawToken);
                            using (SqlDataReader reader = select.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                found = reader.Read();
                                if (found)
                                {
                                    tokenID = reader.GetInt32(reader.GetOrdinal("TokenID"));
                                    userID = reader.GetInt32(reader.GetOrdinal("UserID"));
                                }
                            }
                        }

                        if (!found)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        using (SqlCommand updateUser = new SqlCommand(@"
                            UPDATE dbo.[User]
                            SET [password] = @password
                            WHERE UserID = @userID;", connection, transaction))
                        {
                            updateUser.Parameters.Add("@password", SqlDbType.NVarChar, 255).Value = passwordHash;
                            updateUser.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                            if (updateUser.ExecuteNonQuery() != 1)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        using (SqlCommand consume = new SqlCommand(@"
                            UPDATE dbo.Token
                            SET used = 1
                            WHERE TokenID = @tokenID OR (UserID = @userID AND token_type = @tokenType);", connection, transaction))
                        {
                            consume.Parameters.Add("@tokenID", SqlDbType.Int).Value = tokenID;
                            consume.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                            consume.Parameters.Add("@tokenType", SqlDbType.NVarChar, 30).Value = TokenType;
                            consume.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static string CreateRawToken()
        {
            byte[] bytes = new byte[32];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string HashToken(string rawToken)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken)));
            }
        }
    }
}
