using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace tutorial.Models
{
    [Keyless]
    public class Queue_Branch
    {
        [Key]
        public string BranchID { get; set; }
        public string BranchName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int QNumberCount { get; set; }
        public double AvgProcessingTimeSeconds { get; set; }
        public string AvgProcessingTime { get; set; }

        public virtual ICollection<CompleteQ> CompleteQs { get; set; }
    }
}
