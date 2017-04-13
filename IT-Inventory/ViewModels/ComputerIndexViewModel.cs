using System.Collections.Generic;

namespace IT_Inventory.ViewModels
{
    public class ComputerIndexViewModel
    {
        public IEnumerable<ComputerViewModel> Computers;
        public Pager Pager { get; set; }
    }
}