using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class DepartmentViewModel
    {
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Офис")]
        public int OfficeId { get; set; }

        [Required]
        public int DepId { get; set; }
    }
}