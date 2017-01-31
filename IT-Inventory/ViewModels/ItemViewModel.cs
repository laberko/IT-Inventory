using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class ItemViewModel
    {
        public ItemViewModel()
        {
            AttributeValues = new List<ItemAttributeValueViewModel>();
        }
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Количество")]
        public int Quantity { get; set; }
        public string ItemTypeName { get; set; }
        public int ItemTypeId { get; set; }

        [Display(Name = "Кто выдал")]
        public int WhoGaveId { get; set; }

        [Display(Name = "Кто получил")]
        public int WhoTookId { get; set; }
        public List<ItemAttributeValueViewModel> AttributeValues { get; set; }
    }
}