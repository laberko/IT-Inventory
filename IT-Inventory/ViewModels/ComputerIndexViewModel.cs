using System.Collections.Generic;

namespace IT_Inventory.ViewModels
{
    public class ComputerIndexViewModel
    {
        public IEnumerable<ComputerViewModel> Computers;
        public Pager Pager { get; set; }
        public IEnumerable<string> DepCodes { get; set; }
        public string DepCode { get; set; }
        public bool PersonSearch { get; set; }
    }
}