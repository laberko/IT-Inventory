using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class Printer
    {
        public Printer()
        {
            Cartridges = new HashSet<Item>();
        }
        [Key]
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "IP-адрес")]
        public string Ip { get; set; }

        [Display(Name = "Расположение")]
        public string Place { get; set; }

        [Display(Name = "Департамент")]
        public virtual Department Department { get; set; }

        [Display(Name = "Картриджи")]
        public virtual ICollection<Item> Cartridges { get; set; }
    }
}