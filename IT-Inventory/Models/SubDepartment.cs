using System.ComponentModel.DataAnnotations;

namespace IT_Inventory.Models
{
    public class SubDepartment
    {
        [Key]
        public int Id { get; set; }
        public int Level { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public virtual Department ParentDepartment { get; set; }
    }
}