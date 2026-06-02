using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutTracker
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    namespace WorkoutTracker
    {
        public class Client
        {
            [Key]
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
        }
    }
}