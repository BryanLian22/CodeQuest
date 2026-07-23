// Purpose: Defines the CourseRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    /// <summary>
    /// Application representation of dbo.Course from the CodeQuest ERD.
    /// </summary>
    public sealed class CourseRecord
    {
        public int CourseID { get; set; }
        public int OwnerUserID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }

        // Catalogue presentation values populated from the current session.
        public bool IsEnrolled { get; set; }
        public string ActionText { get; set; }
        public string ActionUrl { get; set; }
    }
}
