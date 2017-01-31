using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    //type of hardware with attribute collection
    public class ItemTypeViewModel
    {
        public int TypeId { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Характеристики")]
        public List<int?> AttributeIds { get; set; }
    }
}