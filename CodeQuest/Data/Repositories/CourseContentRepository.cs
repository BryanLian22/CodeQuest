using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using CodeQuest.Models;

namespace CodeQuest.Data.Repositories
{
    /// <summary>
    /// Reads the published Course -> Module -> Chapter content chain.
    /// </summary>
    public sealed class CourseContentRepository
    {
        public IList<ModuleRecord> GetPublishedModules(int courseID)
        {
            const string sql = @"
                SELECT
                    m.ModuleID,
                    m.CourseID,
                    m.module_title,
                    m.description AS module_description,
                    m.status,
                    c.ChapterID,
                    c.ModuleID AS ChapterModuleID,
                    c.title AS chapter_title,
                    c.description AS chapter_description
                FROM dbo.Module m
                LEFT JOIN dbo.Chapter c ON c.ModuleID = m.ModuleID
                WHERE m.CourseID = @courseID
                  AND m.status = N'Published'
                ORDER BY m.ModuleID, c.ChapterID;";

            Dictionary<int, ModuleRecord> modulesByID = new Dictionary<int, ModuleRecord>();
            List<ModuleRecord> modules = new List<ModuleRecord>();

            using (SqlConnection connection = DbConnectionFactory.Create())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@courseID", SqlDbType.Int).Value = courseID;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int moduleIDOrdinal = reader.GetOrdinal("ModuleID");
                    int courseIDOrdinal = reader.GetOrdinal("CourseID");
                    int moduleTitleOrdinal = reader.GetOrdinal("module_title");
                    int moduleDescriptionOrdinal = reader.GetOrdinal("module_description");
                    int statusOrdinal = reader.GetOrdinal("status");
                    int chapterIDOrdinal = reader.GetOrdinal("ChapterID");
                    int chapterModuleIDOrdinal = reader.GetOrdinal("ChapterModuleID");
                    int chapterTitleOrdinal = reader.GetOrdinal("chapter_title");
                    int chapterDescriptionOrdinal = reader.GetOrdinal("chapter_description");

                    while (reader.Read())
                    {
                        int moduleID = reader.GetInt32(moduleIDOrdinal);
                        ModuleRecord module;

                        if (!modulesByID.TryGetValue(moduleID, out module))
                        {
                            module = new ModuleRecord
                            {
                                ModuleID = moduleID,
                                CourseID = reader.GetInt32(courseIDOrdinal),
                                Title = reader.GetString(moduleTitleOrdinal),
                                Description = reader.IsDBNull(moduleDescriptionOrdinal) ? null : reader.GetString(moduleDescriptionOrdinal),
                                Status = reader.GetString(statusOrdinal)
                            };
                            modulesByID.Add(moduleID, module);
                            modules.Add(module);
                        }

                        if (!reader.IsDBNull(chapterIDOrdinal))
                        {
                            module.Chapters.Add(new ChapterRecord
                            {
                                ChapterID = reader.GetInt32(chapterIDOrdinal),
                                ModuleID = reader.GetInt32(chapterModuleIDOrdinal),
                                Title = reader.GetString(chapterTitleOrdinal),
                                Description = reader.IsDBNull(chapterDescriptionOrdinal) ? null : reader.GetString(chapterDescriptionOrdinal)
                            });
                        }
                    }
                }
            }

            return modules;
        }
    }
}
