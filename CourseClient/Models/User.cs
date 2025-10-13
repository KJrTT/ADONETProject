using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseClient.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? password { get; set; }
        public string? user_email { get; set; }
        public int level_access_id { get; set; }
        public int wallet_id { get; set; }


        public LevelAccess LevelAccess { get; set; } = null!;
        public Wallet Wallet { get; set; } = null!;
        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
    }
}
