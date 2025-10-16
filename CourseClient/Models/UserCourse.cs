using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseClient.Models
{
    public class UserCourse
    {
        public int UserCourseId { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }


        public Course Course { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
