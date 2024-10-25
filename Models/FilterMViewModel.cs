using Microsoft.AspNetCore.Mvc.Rendering;

namespace tutorial.Models
{
    public class FilterMViewModel
    {
        public string BranchID { get; set; }
        public string StartMonth { get; set; } 
        public int StartYear { get; set; } 
        public string EndMonth { get; set; } 
        public int EndYear { get; set; } 

        public IEnumerable<SelectListItem> Branches { get; set; } 
        public IEnumerable<SelectListItem> Months { get; set; } 
        public IEnumerable<SelectListItem> Years { get; set; } 

        public IEnumerable<QueueView> QueueViewList { get; set; }
        public List<QueueView> QueueData { get; set; }
    }
}
