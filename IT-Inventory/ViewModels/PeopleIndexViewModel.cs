using System.Collections.Generic;
using IT_Inventory.Models;

namespace IT_Inventory.ViewModels
{
    public class PeopleIndexViewModel
    {
        public IEnumerable<Person> People { get; set; }
        public bool IsRefreshed { get; set; }
        public Pager Pager { get; set; }
    }
}