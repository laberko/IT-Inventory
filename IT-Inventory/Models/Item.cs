using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    //hardware item of selected type with attribute value collection
    public class Item
    {
        public Item()
        {
            Histories = new HashSet<History>();
            AttributeValues = new HashSet<ItemAttributeValue>();
            Printers = new HashSet<Printer>();
        }
        [Key]
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Минимум")]
        public int MinQuantity { get; set; }

        [Display(Name = "Тип")]
        public virtual ItemType ItemType { get; set; }

        //attribute-value pair collection
        public virtual ICollection<ItemAttributeValue> AttributeValues { get; set; }

        public virtual ICollection<History> Histories { get; set; }

        //for cartridges
        public virtual ICollection<Printer> Printers { get; set; }
    }
}