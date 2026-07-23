// Purpose: Encapsulates parameterized SQL Server operations for Billing data and related transactions.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Subscription and demo-payment operations for learner accounts.
    /// The checkout is intentionally local to this prototype: no card data is
    /// accepted or stored, while the ERD Payment and Subscription rows are
    /// written atomically.
    /// </summary>
    public sealed class BillingRepository
    {
        public SubscriptionRecord GetActiveSubscription(int userID)
        {
            const string sql = @"
                SELECT TOP (1) SubscriptionID, UserID, plan_type, billing_cycle,
                               start_date, end_date, status
                FROM dbo.Subscription
                WHERE UserID = @userID
                  AND status = N'Active'
                  AND (end_date IS NULL OR end_date >= CAST(GETDATE() AS date))
                ORDER BY SubscriptionID DESC;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return reader.Read() ? ReadSubscription(reader) : null;
                }
            }
        }

        public IList<PaymentRecord> GetPaymentHistory(int userID)
        {
            const string sql = @"
                SELECT PaymentID, UserID, SubscriptionID, amount,
                       transaction_ref, status, paid_at
                FROM dbo.Payment
                WHERE UserID = @userID
                ORDER BY PaymentID DESC;";

            List<PaymentRecord> payments = new List<PaymentRecord>();

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int subscriptionOrdinal = reader.GetOrdinal("SubscriptionID");
                        int paidAtOrdinal = reader.GetOrdinal("paid_at");
                        payments.Add(new PaymentRecord
                        {
                            PaymentID = reader.GetInt32(reader.GetOrdinal("PaymentID")),
                            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            SubscriptionID = reader.IsDBNull(subscriptionOrdinal) ? (int?)null : reader.GetInt32(subscriptionOrdinal),
                            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                            TransactionReference = reader.IsDBNull(reader.GetOrdinal("transaction_ref")) ? null : reader.GetString(reader.GetOrdinal("transaction_ref")),
                            Status = reader.GetString(reader.GetOrdinal("status")),
                            PaidAt = reader.IsDBNull(paidAtOrdinal) ? (DateTime?)null : reader.GetDateTime(paidAtOrdinal)
                        });
                    }
                }
            }

            return payments;
        }

        public PremiumPurchaseResult ActivatePremium(int userID, string transactionReference)
        {
            if (string.IsNullOrWhiteSpace(transactionReference))
            {
                throw new ArgumentException("A transaction reference is required.", "transactionReference");
            }

            const string existingSql = @"
                SELECT TOP (1) SubscriptionID
                FROM dbo.Subscription
                WHERE UserID = @userID
                  AND plan_type = N'Premium'
                  AND status = N'Active'
                  AND (end_date IS NULL OR end_date >= CAST(GETDATE() AS date))
                ORDER BY SubscriptionID DESC;";

            const string insertSubscriptionSql = @"
                INSERT INTO dbo.Subscription
                    (UserID, plan_type, billing_cycle, start_date, end_date, status)
                OUTPUT INSERTED.SubscriptionID
                VALUES
                    (@userID, N'Premium', N'Monthly',
                     CAST(GETDATE() AS date),
                     DATEADD(month, 1, CAST(GETDATE() AS date)),
                     N'Active');";

            const string insertPaymentSql = @"
                INSERT INTO dbo.Payment
                    (UserID, SubscriptionID, amount, transaction_ref, status, paid_at)
                VALUES
                    (@userID, @subscriptionID, @amount, @transactionReference,
                     N'Completed', SYSDATETIME());";

            const string updateUserSql = @"
                UPDATE dbo.[User]
                SET [plan] = N'Premium'
                WHERE UserID = @userID;";

            using (SqlConnection connection = DbConnectionFactory.Create())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int existingSubscriptionID;
                        using (SqlCommand existing = new SqlCommand(existingSql, connection, transaction))
                        {
                            existing.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                            object value = existing.ExecuteScalar();
                            existingSubscriptionID = value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
                        }

                        if (existingSubscriptionID > 0)
                        {
                            using (SqlCommand updateExistingUser = new SqlCommand(updateUserSql, connection, transaction))
                            {
                                updateExistingUser.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                                updateExistingUser.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return new PremiumPurchaseResult
                            {
                                AlreadyPremium = true,
                                SubscriptionID = existingSubscriptionID,
                                TransactionReference = null
                            };
                        }

                        int subscriptionID;
                        using (SqlCommand insertSubscription = new SqlCommand(insertSubscriptionSql, connection, transaction))
                        {
                            insertSubscription.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                            subscriptionID = Convert.ToInt32(insertSubscription.ExecuteScalar());
                        }

                        using (SqlCommand insertPayment = new SqlCommand(insertPaymentSql, connection, transaction))
                        {
                            insertPayment.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                            insertPayment.Parameters.Add("@subscriptionID", SqlDbType.Int).Value = subscriptionID;
                            insertPayment.Parameters.Add("@amount", SqlDbType.Decimal).Precision = 10;
                            insertPayment.Parameters["@amount"].Scale = 2;
                            insertPayment.Parameters["@amount"].Value = 29.00m;
                            insertPayment.Parameters.Add("@transactionReference", SqlDbType.NVarChar, 100).Value = transactionReference.Trim();
                            insertPayment.ExecuteNonQuery();
                        }

                        using (SqlCommand updateUser = new SqlCommand(updateUserSql, connection, transaction))
                        {
                            updateUser.Parameters.Add("@userID", SqlDbType.Int).Value = userID;
                            if (updateUser.ExecuteNonQuery() != 1)
                            {
                                throw new InvalidOperationException("The learner account could not be updated.");
                            }
                        }

                        transaction.Commit();
                        return new PremiumPurchaseResult
                        {
                            AlreadyPremium = false,
                            SubscriptionID = subscriptionID,
                            TransactionReference = transactionReference.Trim()
                        };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static SubscriptionRecord ReadSubscription(SqlDataReader reader)
        {
            int endDateOrdinal = reader.GetOrdinal("end_date");
            return new SubscriptionRecord
            {
                SubscriptionID = reader.GetInt32(reader.GetOrdinal("SubscriptionID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                PlanType = reader.GetString(reader.GetOrdinal("plan_type")),
                BillingCycle = reader.GetString(reader.GetOrdinal("billing_cycle")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("start_date")),
                EndDate = reader.IsDBNull(endDateOrdinal) ? (DateTime?)null : reader.GetDateTime(endDateOrdinal),
                Status = reader.GetString(reader.GetOrdinal("status"))
            };
        }
    }
}
