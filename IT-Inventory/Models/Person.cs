using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace IT_Inventory.Models
{
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Display(Name = "Учетная запись")]
        public string AccountName { get; set; }

        [Display(Name = "Сотрудник ДИТ")]
        public bool IsItUser { get; set; }

        [NotMapped]
        public string ShortName => FullName.GetShortName();
    }
}