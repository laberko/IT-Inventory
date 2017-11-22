using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class ComputerSearchViewModel
    {
        [Display(Name = "Поиск компьютеров с ПО: ")]
        public string SearchSoftString { get; set; }

        [Display(Name = "Поиск компьютеров по другим данным: ")]
        public string SearchDataString { get; set; }

    }
}