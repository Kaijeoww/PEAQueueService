using Microsoft.AspNetCore.Mvc.Rendering;

namespace tutorial.Models
{
    public class CompleteQViewModel
    {
        public IEnumerable<CompleteQ> CompleteQs { get; set; }
        public IEnumerable<SelectListItem> Branches { get; set; }
        public string SelectedBranch { get; set; }
    }
}
