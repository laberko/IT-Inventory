using System.Collections.Generic;
using System.Linq;
using IT_Inventory.Models;

namespace IT_Inventory.ViewModels
{
    public class PeopleIndexViewModel
    {
        public IEnumerable<Person> People { get; set; }
        public bool IsRefreshed { get; set; }
        public Pager Pager { get; set; }
        public IEnumerable<char> FirstLetters { get; set; }
        public string Letter { get; set; }
    }
}