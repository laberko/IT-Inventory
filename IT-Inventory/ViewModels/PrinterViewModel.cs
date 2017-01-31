using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class PrinterViewModel
    {
        public PrinterViewModel()
        {
            CartridgeIds = new List<int?>();
        }
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "IP-адрес")]
        public string Ip { get; set; }

        [Display(Name = "Расположение")]
        public string Place { get; set; }

        [Display(Name = "Департамент")]
        public int DepartmentId { get; set; }

        [Display(Name = "Картриджи")]
        public List<int?> CartridgeIds { get; set; }
    }
}