﻿using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public int Level { get; set; }
        public int Order { get; set; }
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Display(Name = "Офис")]
        public virtual Office Office { get; set; }
    }
}