namespace DayKast_VFeb2026.Models
{
    using System;

    public class CommentViewModel
    {
        public int CommentID { get; set; }
        public string MemberName { get; set; }
        public string CommentText { get; set; }
        public int Rating { get; set; }
        public DateTime CommentDate { get; set; }
    }
}
