using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class OfficeViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }
    }
}