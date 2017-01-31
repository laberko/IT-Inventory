using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    //attribute name-value pair
    public class ItemAttributeValue
    {
        public ItemAttributeValue()
        {
            Value = string.Empty;
        }
        [Key]
        public int Id { get; set; }
        public virtual ItemAttribute Attribute{ get; set; }     //name
        public virtual Item ParentItem { get; set; }
        public string Value { get; set; }
    }
}