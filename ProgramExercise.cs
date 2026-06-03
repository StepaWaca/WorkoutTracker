using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.WorkoutTracker;

namespace WorkoutTracker
{
    public class ProgramExercise
    {
        [Key]
        public int Id { get; set; }
        public int ProgramId { get; set; }
        public int ExerciseId { get; set; }
        public int TargetSets { get; set; }
        public int TargetReps { get; set; }
        public int TargetWeight { get; set; }

        [ForeignKey("ProgramId")]
        public virtual WorkoutProgram? Program { get; set; }

        [ForeignKey("ExerciseId")]
        public virtual Exercise? Exercise { get; set; }
    }
}
