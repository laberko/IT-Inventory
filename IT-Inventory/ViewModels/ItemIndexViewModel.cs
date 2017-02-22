using System.Collections.Generic;
using IT_Inventory.Models;

namespace IT_Inventory.ViewModels
{
    public class ItemIndexViewModel
    {
        public IEnumerable<Item> Items { get; set; }
        public Pager Pager { get; set; }
        public ItemType Type { get; set; }
        public bool IsUrgent { get; set; }
    }
}