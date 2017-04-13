using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.ViewModels
{
    public class ReportViewModel
    {
        [Display(Name = "Имя компьютера")]
        public string CompName;
        [Display(Name = "Дата")]
        public DateTime ReportDate;
        [Display(Name = "Пользователь")]
        public string UserName;

        public string ReportDateString => ReportDate.ToString("g");
    }
}