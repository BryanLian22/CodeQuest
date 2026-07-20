using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Public Tutorial -> Exercise content. These records are intentionally
    /// separate from the learner-only Course -> Module -> Chapter hierarchy.
    /// </summary>
    public sealed class TutorialRepository
    {
        public IList<TutorialRecord> GetPublished()
        {
            return GetPublished(null);
        }

        public IList<TutorialRecord> GetPublished(string category)
        {
            const string sql = @"
                SELECT t.TutorialID, t.category, t.tutorial_title, t.materials,
                       e.ExerciseID, e.question, e.correct_answer
                FROM dbo.Tutorial t
                LEFT JOIN dbo.Exercise e ON e.TutorialID = t.TutorialID
                WHERE t.status = N'Published'
                  AND (@category IS NULL OR t.category = @category)
                ORDER BY t.TutorialID, e.ExerciseID;";

            return ReadTutorials(sql, null, category);
        }

        public TutorialRecord GetPublishedByID(int tutorialID)
        {
            const string sql = @"
                SELECT t.TutorialID, t.category, t.tutorial_title, t.materials,
                       e.ExerciseID, e.question, e.correct_answer
                FROM dbo.Tutorial t
                LEFT JOIN dbo.Exercise e ON e.TutorialID = t.TutorialID
                WHERE t.status = N'Published'
                  AND t.TutorialID = @tutorialID
                ORDER BY e.ExerciseID;";

            IList<TutorialRecord> tutorials = ReadTutorials(sql, tutorialID, null);
            return tutorials.Count == 0 ? null : tutorials[0];
        }

        private IList<TutorialRecord> ReadTutorials(string sql, int? tutorialID, string category)
        {
            List<TutorialRecord> tutorials = new List<TutorialRecord>();
            Dictionary<int, TutorialRecord> tutorialsByID = new Dictionary<int, TutorialRecord>();

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (tutorialID.HasValue)
                {
                    command.Parameters.Add("@tutorialID", SqlDbType.Int).Value = tutorialID.Value;
                }

                command.Parameters.Add("@category", SqlDbType.NVarChar, 30).Value =
                    string.IsNullOrWhiteSpace(category) ? (object)System.DBNull.Value : category;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int materialsOrdinal = reader.GetOrdinal("materials");

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("TutorialID"));
                        TutorialRecord tutorial;

                        if (!tutorialsByID.TryGetValue(id, out tutorial))
                        {
                            tutorial = new TutorialRecord
                            {
                                TutorialID = id,
                                Category = reader.IsDBNull(reader.GetOrdinal("category"))
                                    ? InferCategory(reader.GetString(reader.GetOrdinal("tutorial_title")))
                                    : reader.GetString(reader.GetOrdinal("category")),
                                Title = reader.GetString(reader.GetOrdinal("tutorial_title")),
                                Materials = reader.IsDBNull(materialsOrdinal) ? null : reader.GetString(materialsOrdinal)
                            };
                            tutorialsByID.Add(id, tutorial);
                            tutorials.Add(tutorial);
                        }

                        int exerciseIDOrdinal = reader.GetOrdinal("ExerciseID");
                        if (!reader.IsDBNull(exerciseIDOrdinal))
                        {
                            tutorial.Exercises.Add(new ExerciseRecord
                            {
                                ExerciseID = reader.GetInt32(exerciseIDOrdinal),
                                Question = reader.GetString(reader.GetOrdinal("question")),
                                CorrectAnswer = reader.GetString(reader.GetOrdinal("correct_answer"))
                            });
                        }
                    }
                }
            }

            return tutorials;
        }

        private static string InferCategory(string title)
        {
            if (title.IndexOf("CSS", System.StringComparison.OrdinalIgnoreCase) >= 0
                || title.IndexOf("Box Model", System.StringComparison.OrdinalIgnoreCase) >= 0
                || title.IndexOf("Flexbox", System.StringComparison.OrdinalIgnoreCase) >= 0
                || title.IndexOf("Responsive", System.StringComparison.OrdinalIgnoreCase) >= 0
                || title.IndexOf("Selectors", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "CSS";
            }

            if (title.IndexOf("JavaScript", System.StringComparison.OrdinalIgnoreCase) >= 0
                || title.IndexOf("DOM", System.StringComparison.OrdinalIgnoreCase) >= 0
                || title.IndexOf("Functions", System.StringComparison.OrdinalIgnoreCase) >= 0
                || title.IndexOf("Variables", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "JavaScript";
            }

            return "HTML";
        }
    }
}
