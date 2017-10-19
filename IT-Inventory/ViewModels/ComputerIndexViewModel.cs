using System.Collections.Generic;

namespace IT_Inventory.ViewModels
{
    public class ComputerIndexViewModel
    {
        public IEnumerable<ComputerViewModel> Computers;
        public Pager Pager { get; set; }
        public IEnumerable<string> DepCodes { get; set; }
        public string DepCode { get; set; }
        public string SearchSoft { get; set; }
        public bool PersonSearch { get; set; }
        public bool HasModifiedComputers { get; set; }
        public bool ModifiedComputers { get; set; }
        public bool Notebooks { get; set; }
    }
}