using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IT_Inventory.Models
{
    public class SupportRequest
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Текст")]
        public string Text { get; set; }
        [Display(Name = "Срочность")]
        public StaticData.SupportUrgency Urgency { get; set; }
        [Display(Name = "Категория")]
        public StaticData.SupportCategory Category { get; set; }
        [Display(Name = "От кого")]
        public virtual Person From { get; set; }
        [Display(Name = "Исполнитель")]
        public virtual Person To { get; set; }
    }
}