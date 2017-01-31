using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    //possible attribute
    public class ItemAttribute
    {
        public ItemAttribute()
        {
            ItemTypes = new HashSet<ItemType>();
            ItemAttributeValues = new HashSet<ItemAttributeValue>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Числовое значение")]
        public bool IsNumber { get; set; }

        public virtual ICollection<ItemType> ItemTypes { get; set; }
        public virtual ICollection<ItemAttributeValue> ItemAttributeValues { get; set; }
    }
}