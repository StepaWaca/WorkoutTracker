namespace WorkoutTracker
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace WorkoutTracker
    {
        public class Activity
        {
            [Key]
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public int Minutes { get; set; }
            public string Notes { get; set; } = string.Empty;
            public int ExerciseId { get; set; }
            public int ClientId { get; set; }

            [ForeignKey("ExerciseId")]
            public virtual Exercise? Exercise { get; set; }
        }
    }
}