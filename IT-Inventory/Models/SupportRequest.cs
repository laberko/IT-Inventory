﻿using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class SupportRequest
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Текст")]
        public string Text { get; set; }

        [Display(Name = "Комментарий")]
        public string Comment { get; set; }

        [Display(Name = "Оценка")]
        public int Mark { get; set; }

        [Display(Name = "Отзыв")]
        public string FeedBack { get; set; }

        [Display(Name = "Срочность")]
        public int Urgency { get; set; }

        [Display(Name = "Категория")]
        public int Category { get; set; }

        [Display(Name = "Состояние")]
        public int State { get; set; }

        [Display(Name = "Создана")]
        public DateTime CreationTime { get; set; }

        [Display(Name = "Принята")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "Завершена")]
        public DateTime? FinishTime { get; set; }

        [Display(Name = "Установлено ПО")]
        public string SoftwareInstalled { get; set; }

        [Display(Name = "Восстановлено ПО")]

        public string SoftwareRepaired { get; set; }

        [Display(Name = "Обновлено ПО")]

        public string SoftwareUpdated { get; set; }

        [Display(Name = "Удалено ПО")]

        public string SoftwareRemoved { get; set; }

        [Display(Name = "Установлено оборудование")]

        public string HardwareInstalled { get; set; }

        [Display(Name = "Заменено оборудование")]

        public string HardwareReplaced { get; set; }

        [Display(Name = "Другие действия")]

        public string OtherActions { get; set; }

        [Display(Name = "Пользователь")]
        public virtual Person From { get; set; }

        [Display(Name = "Исполнитель")]
        public virtual Person To { get; set; }

        [Display(Name = "Компьютер")]

        public virtual Computer FromComputer { get; set; }
    }
}