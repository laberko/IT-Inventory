using System.ComponentModel.DataAnnotations;
using System.Web;
using IT_Inventory.Models;

namespace IT_Inventory.ViewModels
{
    public class SupportRequestViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Текст")]
        public string Text { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Display(Name = "Оценка")]
        public string Mark { get; set; }

        [Display(Name = "Отзыв о выполнении")]
        public string FeedBack { get; set; }

        [Display(Name = "Срочность")]
        public int Urgency { get; set; }

        [Display(Name = "Категория")]
        public int Category { get; set; }

        [Display(Name = "Состояние")]
        public int State { get; set; }

        [Display(Name = "Установлено ПО")]
        public string SoftwareInstalled { get; set; }

        [Display(Name = "Восстановлено ПО")]
        public string SoftwareRepaired { get; set; }

        [Display(Name = "Обновлено ПО")]
        public string SoftwareUpdated { get; set; }

        [Display(Name = "Удалено ПО")]
        public string SoftwareRemoved { get; set; }

        [Display(Name = "Выдано")]
        public string HardwareId { get; set; }

        [Display(Name = "Тип оборудования")]
        public int? HardwareCategory { get; set; }
        
        [Display(Name = "Количество")]
        public int HardwareQuantity { get; set; }

        [Display(Name = "Другие действия")]
        public string OtherActions { get; set; }

        [Display(Name = "Пользователь")]
        public Person From { get; set; }

        [Display(Name = "Пользователь")]
        public int FromId { get; set; }

        [Display(Name = "Исполнитель")]
        public int ToId { get; set; }

        [Display(Name = "Компьютер")]
        public Computer FromComputer { get; set; }

        public bool EditByIt { get; set; }

        [Display(Name = "Файл")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase Upload { get; set; }
    }
}