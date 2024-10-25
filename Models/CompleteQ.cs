using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tutorial.Models
{
    public class CompleteQ
    {
        [Key]
        [MaxLength(6)]
        public string QNumber { get; set; }
        [Required]
        public DateTime? QPress { get; set; }

        public DateTime? QBegin { get; set; }

        public DateTime? QEnd { get; set; }

        [MaxLength(15)]
        public  string CounterID { get; set; }

        [MaxLength(64)]
        public string UserID { get; set; }

        public string QStatus { get; set; }

        public string BranchID { get; set; }

        public string ServiceGroupID { get; set; }

        [NotMapped]
        public string TimeToBeginFormatted { get; set; }
        [NotMapped]
        public string TimeToEndFormatted { get;set; }
        [NotMapped]
        public virtual Queue_Branch Queue_Branch { get; set; }

    }
}
