using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseClient.Models
{
    public class LevelAccess
    {
        public int AccessId { get; set; }
        public string? Title { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
