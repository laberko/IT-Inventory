using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    //type of hardware with attribute collection
    public class ItemType
    {
        public ItemType()
        {
            Attributes = new HashSet<ItemAttribute>();
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Категория")]
        public string Name { get; set; }

        [Display(Name = "Характеристики")]
        public virtual ICollection<ItemAttribute> Attributes { get; set; }
    }
}