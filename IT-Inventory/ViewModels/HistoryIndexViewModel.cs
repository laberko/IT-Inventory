using System.Collections.Generic;
using IT_Inventory.Models;

namespace IT_Inventory.ViewModels
{
    public class HistoryIndexViewModel
    {
        public IEnumerable<History> Histories { get; set; }
        public Pager Pager { get; set; }
        public int MonthGrant { get; set; }
        public int MonthRecieve { get; set; }
        public int? Id { get; set; }
        public string PersonName { get; set; }
        public string ItemName { get; set; }
        public bool GrantHistory { get; set; }
    }
}