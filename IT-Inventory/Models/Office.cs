using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class Office
    {
        public Office()
        {
            Departments = new HashSet<Department>();
        }
        [Key]
        public int Id { get; set; }

        [Display(Name = "Название офиса")]
        public string Name { get; set; }

        [Display(Name = "Департаменты")]
        public virtual ICollection<Department> Departments { get; set; }
    }
}