using System.ComponentModel.DataAnnotations;

namespace tutorial.Models
{
    public class QueueView
    {
        [Key]
        public string BranchID { get; set; }
        public string BranchName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int QNumberCount { get; set; }
        public string AvgProcessingTime { get; set; }
        public string AvgWaitingTime { get; set; }
        public string ProcessingTimeAfterWaiting { get; set; }
        public string MonthName { get; set; }


    }
}