namespace WorkoutTracker
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace WorkoutTracker
    {
        public class Exercise
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
            public int ClientId { get; set; }
            public int? ProgramId { get; set; }
        }
    }
}