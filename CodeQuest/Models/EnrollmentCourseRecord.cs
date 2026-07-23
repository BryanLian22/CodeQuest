// Purpose: Defines the EnrollmentCourseRecord data shape shared between repositories and Web Forms pages.
namespace CodeQuest.Models
{
    /// <summary>
    /// A learner's enrollment joined with the related course record.
    /// </summary>
    public sealed class EnrollmentCourseRecord
    {
        public int EnrollmentID { get; set; }
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public string Difficulty { get; set; }
        public string Status { get; set; }
    }
}
