using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class History
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Приход")]
        public bool Recieved { get; set; }

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Display(Name = "Название")]
        public virtual Item Item { get; set; }

        [Display(Name = "Кто получил")]
        public virtual Person WhoTook { get; set; }

        [Display(Name = "Кто выдал")]
        public virtual Person WhoGave { get; set; }
    }
}