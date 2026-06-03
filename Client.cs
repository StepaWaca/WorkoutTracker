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
            public virtual ICollection<WorkoutProgram> Programs { get; set; } = new List<WorkoutProgram>();
            public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
            public virtual ICollection<Workout> Workouts { get; set; } = new List<Workout>();
        }
    }
}