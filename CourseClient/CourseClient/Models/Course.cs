using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseClient.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string? name_course {  get; set; }
        public DateTime data_start { get; set; }
        public DateTime data_end { get; set; }
        public double price { get; set; }
        public int user_id { get; set; }


        public User User { get; set; } = null!;
        public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
    }
}
