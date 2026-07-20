using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Ticket and reply operations for the learner and administrator support
    /// workspaces. Authorization is checked by the pages and repeated in the
    /// user-specific queries here before a ticket can be changed.
    /// </summary>
    public sealed class SupportRepository
    {
        public IList<TicketRecord> GetTicketsForUser(int userID)
        {
            return GetTickets(@"WHERE t.UserID = @userID", userID);
        }

        public IList<TicketRecord> GetAllTickets()
        {
            return GetTickets(string.Empty, null);
        }

        public TicketRecord GetTicketForUser(int ticketID, int userID)
        {
            return GetTicket(ticketID, userID, false);
        }

        public TicketRecord GetTicketForAdmin(int ticketID)
        {
            return GetTicket(ticketID, null, true);
        }

        public int CreateTicket(int userID, string name, string email, string category, string subject, string description)
        {
            const string sql = @"
                INSERT INTO dbo.Ticket(UserID, name, email, category, subject, description, status)
                OUTPUT INSERTED.TicketID
                VALUES (@userID, @name, @email, @category, @subject, @description, N'Open');";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = Clean(name);
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = Clean(email);
                command.Parameters.Add("@category", SqlDbType.NVarChar, 40).Value = Clean(category);
                command.Parameters.Add("@subject", SqlDbType.NVarChar, 200).Value = Clean(subject);
                command.Parameters.Add("@description", SqlDbType.NVarChar, -1).Value = Clean(description);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool AddReply(int ticketID, int userID, string message, bool isAdmin)
        {
            const string sql = @"
                IF EXISTS
                (
                    SELECT 1 FROM dbo.Ticket
                    WHERE TicketID = @ticketID
                      AND (@isAdmin = 1 OR UserID = @userID)
                      AND status <> N'Closed'
                )
                BEGIN
                    INSERT INTO dbo.Reply(TicketID, UserID, message)
                    VALUES (@ticketID, @userID, @message);

                    IF @isAdmin = 1
                    BEGIN
                        UPDATE dbo.Ticket
                        SET status = CASE WHEN status = N'Open' THEN N'In Progress' ELSE status END
                        WHERE TicketID = @ticketID;
                    END;

                    SELECT 1;
                END
                ELSE
                BEGIN
                    SELECT 0;
                END;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@ticketID", SqlDbType.Int).Value = ticketID;
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                command.Parameters.Add("@message", SqlDbType.NVarChar, -1).Value = Clean(message);
                command.Parameters.Add("@isAdmin", SqlDbType.Bit).Value = isAdmin;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        public bool UpdateStatus(int ticketID, string status)
        {
            const string sql = @"
                UPDATE dbo.Ticket
                SET status = @status
                WHERE TicketID = @ticketID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@ticketID", SqlDbType.Int).Value = ticketID;
                command.Parameters.Add("@status", SqlDbType.NVarChar, 20).Value = Clean(status);
                connection.Open();
                return command.ExecuteNonQuery() == 1;
            }
        }

        private static IList<TicketRecord> GetTickets(string filter, int? userID)
        {
            string sql = @"
                SELECT t.TicketID, t.UserID, t.name, t.email, t.category,
                       t.subject, t.description, t.status,
                       COUNT(r.ReplyID) AS ReplyCount,
                       MAX(r.created_at) AS LastReplyAt
                FROM dbo.Ticket t
                LEFT JOIN dbo.Reply r ON r.TicketID = t.TicketID
                " + filter + @"
                GROUP BY t.TicketID, t.UserID, t.name, t.email, t.category,
                         t.subject, t.description, t.status
                ORDER BY t.TicketID DESC;";

            List<TicketRecord> tickets = new List<TicketRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (userID.HasValue)
                {
                    command.Parameters.Add("@userID", SqlDbType.Int).Value = userID.Value;
                }

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tickets.Add(ReadTicket(reader));
                    }
                }
            }

            return tickets;
        }

        private static TicketRecord GetTicket(int ticketID, int? userID, bool admin)
        {
            string sql = @"
                SELECT t.TicketID, t.UserID, t.name, t.email, t.category,
                       t.subject, t.description, t.status,
                       COUNT(r.ReplyID) AS ReplyCount,
                       MAX(r.created_at) AS LastReplyAt
                FROM dbo.Ticket t
                LEFT JOIN dbo.Reply r ON r.TicketID = t.TicketID
                WHERE t.TicketID = @ticketID
                " + (admin ? string.Empty : "AND t.UserID = @userID") + @"
                GROUP BY t.TicketID, t.UserID, t.name, t.email, t.category,
                         t.subject, t.description, t.status;";

            TicketRecord ticket = null;
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@ticketID", SqlDbType.Int).Value = ticketID;
                if (!admin && userID.HasValue)
                {
                    command.Parameters.Add("@userID", SqlDbType.Int).Value = userID.Value;
                }

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        ticket = ReadTicket(reader);
                    }
                }
            }

            if (ticket != null)
            {
                ticket.Replies = GetReplies(ticket.TicketID);
            }

            return ticket;
        }

        private static IList<TicketReplyRecord> GetReplies(int ticketID)
        {
            const string sql = @"
                SELECT r.ReplyID, r.TicketID, r.UserID, u.username,
                       r.message, r.created_at, u.role
                FROM dbo.Reply r
                INNER JOIN dbo.[User] u ON u.UserID = r.UserID
                WHERE r.TicketID = @ticketID
                ORDER BY r.created_at, r.ReplyID;";

            List<TicketReplyRecord> replies = new List<TicketReplyRecord>();
            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@ticketID", SqlDbType.Int).Value = ticketID;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        replies.Add(new TicketReplyRecord
                        {
                            ReplyID = reader.GetInt32(reader.GetOrdinal("ReplyID")),
                            TicketID = reader.GetInt32(reader.GetOrdinal("TicketID")),
                            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            AuthorName = reader.GetString(reader.GetOrdinal("username")),
                            Message = reader.GetString(reader.GetOrdinal("message")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                            IsAdmin = string.Equals(reader.GetString(reader.GetOrdinal("role")), "Admin", StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
            }

            return replies;
        }

        private static TicketRecord ReadTicket(SqlDataReader reader)
        {
            int lastReplyOrdinal = reader.GetOrdinal("LastReplyAt");
            return new TicketRecord
            {
                TicketID = reader.GetInt32(reader.GetOrdinal("TicketID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                Subject = reader.GetString(reader.GetOrdinal("subject")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                ReplyCount = reader.GetInt32(reader.GetOrdinal("ReplyCount")),
                LastReplyAt = reader.IsDBNull(lastReplyOrdinal) ? (DateTime?)null : reader.GetDateTime(lastReplyOrdinal),
                Replies = new List<TicketReplyRecord>()
            };
        }

        private static string Clean(string value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}
